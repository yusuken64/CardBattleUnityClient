using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Networking;

// Hand-rolled client for ASP.NET Core SignalR's JSON hub protocol over a raw WebSocket - no
// Microsoft.AspNetCore.SignalR.Client dependency. Only implements what GameServer's MatchHub
// needs: negotiate -> websocket -> JSON handshake -> record-separator-delimited invoke/completion/
// ping frames. No auto-reconnect, no streaming, no MessagePack.
//
// On<T> handlers fire from the background receive loop's thread and are queued to run on the main
// thread via PumpMainThread - call that once per frame (e.g. from a MonoBehaviour's Update) or
// pushed events will never actually invoke their callbacks.
public class MiniSignalRClient
{
	private const byte RecordSeparator = 0x1E;
	private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(10);
	private static readonly TimeSpan InvokeTimeout = TimeSpan.FromSeconds(15);

	public event Action OnDisconnected;

	private readonly string _hubUrl;
	private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
	{
		ContractResolver = new CamelCasePropertyNamesContractResolver()
	};

	private ClientWebSocket _socket;
	private CancellationTokenSource _cts;
	private int _nextInvocationId;

	private readonly ConcurrentDictionary<string, TaskCompletionSource<JToken>> _pendingInvocations = new();
	private readonly Dictionary<string, List<Action<JArray>>> _handlers = new();
	private readonly ConcurrentQueue<Action> _mainThreadQueue = new();

	public bool IsConnected => _socket != null && _socket.State == WebSocketState.Open;

	public MiniSignalRClient(string hubUrl)
	{
		_hubUrl = hubUrl.TrimEnd('/');
	}

	// Must be called before ConnectAsync - handler registration isn't thread-safe against the
	// receive loop once it's running.
	public void On<T>(string target, Action<T> handler)
	{
		AddHandler(target, arguments =>
		{
			T value = arguments.Count > 0
				? arguments[0].ToObject<T>(JsonSerializer.Create(_jsonSettings))
				: default;
			handler(value);
		});
	}

	// For server push methods with more than one parameter (e.g. OnCardArtRequested(string, string)) -
	// SignalR's JSON protocol sends these as a positional arguments array, one element per parameter,
	// not a single combined object, so each argument must be deserialized independently by position.
	public void On<T1, T2>(string target, Action<T1, T2> handler)
	{
		AddHandler(target, arguments =>
		{
			var settings = JsonSerializer.Create(_jsonSettings);
			T1 arg1 = arguments.Count > 0 ? arguments[0].ToObject<T1>(settings) : default;
			T2 arg2 = arguments.Count > 1 ? arguments[1].ToObject<T2>(settings) : default;
			handler(arg1, arg2);
		});
	}

	private void AddHandler(string target, Action<JArray> handler)
	{
		if (!_handlers.TryGetValue(target, out var list))
		{
			list = new List<Action<JArray>>();
			_handlers[target] = list;
		}

		list.Add(handler);
	}

	public void PumpMainThread()
	{
		while (_mainThreadQueue.TryDequeue(out var action))
		{
			action();
		}
	}

	public async Task ConnectAsync(CancellationToken cancellationToken = default)
	{
		string connectionToken = await NegotiateAsync(cancellationToken);
		cancellationToken.ThrowIfCancellationRequested();

		string wsUrl = ToWebSocketUrl(_hubUrl) + "?id=" + Uri.EscapeDataString(connectionToken);

		_socket = new ClientWebSocket();
		_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		await _socket.ConnectAsync(new Uri(wsUrl), _cts.Token);

		await SendFrame(new JObject { ["protocol"] = "json", ["version"] = 1 }.ToString(Formatting.None));

		string handshakeResponse = await ReceiveHandshakeMessage(_cts.Token);
		var handshake = JObject.Parse(handshakeResponse);
		if (handshake["error"] != null)
		{
			throw new Exception($"SignalR handshake failed: {handshake["error"]}");
		}

		_ = ReceiveLoopAsync(_cts.Token);
		_ = KeepAliveLoopAsync(_cts.Token);
	}

	public async Task<TResult> InvokeAsync<TResult>(string target, params object[] args)
	{
		string invocationId = Interlocked.Increment(ref _nextInvocationId).ToString();

		var message = new JObject
		{
			["type"] = 1,
			["invocationId"] = invocationId,
			["target"] = target,
			["arguments"] = JArray.FromObject(args, JsonSerializer.Create(_jsonSettings))
		};

		var tcs = new TaskCompletionSource<JToken>(TaskCreationOptions.RunContinuationsAsynchronously);
		_pendingInvocations[invocationId] = tcs;

		await SendFrame(message.ToString(Formatting.None));

		Task completedTask = await Task.WhenAny(tcs.Task, Task.Delay(InvokeTimeout));
		if (completedTask != tcs.Task)
		{
			_pendingInvocations.TryRemove(invocationId, out _);
			throw new TimeoutException($"InvokeAsync timed out for target '{target}'");
		}

		JToken result = await tcs.Task;
		return result == null ? default : result.ToObject<TResult>(JsonSerializer.Create(_jsonSettings));
	}

	public async Task DisconnectAsync()
	{
		// Release callers immediately, including a JoinQueue/CreateMatch still awaiting a reply.
		foreach (var invocation in _pendingInvocations)
		{
			if (_pendingInvocations.TryRemove(invocation.Key, out var pending))
				pending.TrySetCanceled();
		}
		try
		{
			if (_socket != null && _socket.State == WebSocketState.Open)
			{
				using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
				await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", timeout.Token);
			}
		}
		finally
		{
			_cts?.Cancel();
			_socket?.Abort();
			_socket?.Dispose();
		}
	}

	private async Task<string> NegotiateAsync(CancellationToken cancellationToken)
	{
		string negotiateUrl = _hubUrl + "/negotiate?negotiateVersion=1";

		using var request = new UnityWebRequest(negotiateUrl, "POST")
		{
			downloadHandler = new DownloadHandlerBuffer(),
			uploadHandler = new UploadHandlerRaw(Array.Empty<byte>())
		};

		UnityWebRequestAsyncOperation operation = request.SendWebRequest();
		while (!operation.isDone)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await Task.Yield();
		}

		if (request.result != UnityWebRequest.Result.Success)
		{
			throw new Exception($"SignalR negotiate failed: {request.error}");
		}

		var response = JObject.Parse(request.downloadHandler.text);
		// Negotiation version 1 uses the connection token for transport requests.
		string connectionToken = (string)response["connectionToken"];
		if (string.IsNullOrEmpty(connectionToken))
		{
			throw new Exception("SignalR negotiate response did not include a connection token.");
		}
		return connectionToken;
	}

	private static string ToWebSocketUrl(string httpUrl)
	{
		if (httpUrl.StartsWith("https://")) return "wss://" + httpUrl.Substring("https://".Length);
		if (httpUrl.StartsWith("http://")) return "ws://" + httpUrl.Substring("http://".Length);
		return httpUrl;
	}

	private async Task SendFrame(string json)
	{
		byte[] payload = Encoding.UTF8.GetBytes(json + (char)RecordSeparator);
		await _socket.SendAsync(payload, WebSocketMessageType.Text, true, _cts?.Token ?? CancellationToken.None);
	}

	// Reads until exactly one record-separator-delimited frame is complete. Only used for the
	// handshake, before ReceiveLoopAsync starts owning the socket.
	private async Task<string> ReceiveHandshakeMessage(CancellationToken token)
	{
		var buffer = new byte[4096];
		var messageBytes = new List<byte>();

		while (true)
		{
			WebSocketReceiveResult result = await _socket.ReceiveAsync(buffer, token);
			for (int i = 0; i < result.Count; i++)
			{
				if (buffer[i] == RecordSeparator)
				{
					return Encoding.UTF8.GetString(messageBytes.ToArray());
				}
				messageBytes.Add(buffer[i]);
			}
		}
	}

	private async Task ReceiveLoopAsync(CancellationToken token)
	{
		var buffer = new byte[8192];
		var pending = new List<byte>();

		try
		{
			while (!token.IsCancellationRequested && _socket.State == WebSocketState.Open)
			{
				WebSocketReceiveResult result = await _socket.ReceiveAsync(buffer, token);

				if (result.MessageType == WebSocketMessageType.Close)
				{
					break;
				}

				for (int i = 0; i < result.Count; i++)
				{
					pending.Add(buffer[i]);
				}

				int separatorIndex;
				while ((separatorIndex = pending.IndexOf(RecordSeparator)) >= 0)
				{
					string json = Encoding.UTF8.GetString(pending.GetRange(0, separatorIndex).ToArray());
					pending.RemoveRange(0, separatorIndex + 1);

					if (json.Length > 0)
					{
						Dispatch(json);
					}
				}
			}
			_mainThreadQueue.Enqueue(() => OnDisconnected?.Invoke());
		}
		catch (OperationCanceledException)
		{
			// Expected on DisconnectAsync.
			_mainThreadQueue.Enqueue(() => OnDisconnected?.Invoke());
		}
		catch (Exception exception)
		{
			_mainThreadQueue.Enqueue(() => Debug.LogError($"SignalR receive loop failed: {exception}"));
			_mainThreadQueue.Enqueue(() => OnDisconnected?.Invoke());
		}
	}

	private void Dispatch(string json)
	{
		JObject message = JObject.Parse(json);
		int type = (int)message["type"];

		switch (type)
		{
			case 1: // Invocation - server pushing to a client-side handler (OnStateUpdated, etc.)
				string target = (string)message["target"];
				var arguments = (JArray)message["arguments"] ?? new JArray();
				if (target != null && _handlers.TryGetValue(target, out var handlerList))
				{
					foreach (var handler in handlerList)
					{
						_mainThreadQueue.Enqueue(() => handler(arguments));
					}
				}
				break;

			case 3: // Completion - response to one of our own InvokeAsync calls
				string invocationId = (string)message["invocationId"];
				if (invocationId != null && _pendingInvocations.TryRemove(invocationId, out var tcs))
				{
					var error = message["error"];
					if (error != null)
					{
						tcs.TrySetException(new Exception(error.ToString()));
					}
					else
					{
						tcs.TrySetResult(message["result"]);
					}
				}
				break;

			// case 6 (ping): no response required, just proof of life - nothing to do.
		}
	}

	private async Task KeepAliveLoopAsync(CancellationToken token)
	{
		try
		{
			while (!token.IsCancellationRequested && _socket.State == WebSocketState.Open)
			{
				await Task.Delay(KeepAliveInterval, token);
				if (_socket.State == WebSocketState.Open)
				{
					await SendFrame("{\"type\":6}");
				}
			}
		}
		catch (OperationCanceledException)
		{
			// Expected on DisconnectAsync.
		}
	}
}

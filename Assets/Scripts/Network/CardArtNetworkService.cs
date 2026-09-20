using System;
using System.Collections.Generic;
using UnityEngine;

public static class CardArtNetworkService
{
	// PNG expands by 4/3 in the JSON protocol. Leave ample room below the server's
	// 1 MiB message limit for the invocation envelope and identifiers.
	private const int MaxArtBytes = 512 * 1024;
	private const int MaxArtDimension = 512;
	private static MiniSignalRClient _client;
	private static string _matchId;
	private static readonly Dictionary<string, Sprite> _receivedArt = new Dictionary<string, Sprite>();
	private static readonly Dictionary<string, string> _receivedArtOwner = new Dictionary<string, string>();
	private static readonly Dictionary<string, string> _requestedIds = new Dictionary<string, string>();

	public static void Initialize(MiniSignalRClient client)
	{
		_client = client;
		// _receivedArt is intentionally NOT cleared here - it persists for the app's lifetime so
		// art already seen in a prior match against the same (or any) opponent is never re-requested.
		// Only _requestedIds resets, so a request that went unanswered in a previous match gets
		// retried rather than permanently suppressed.
		_requestedIds.Clear();

		_client.On<string, string>("OnCardArtRequested", OnCardArtRequested);
		_client.On<string, byte[]>("OnCardArtReceived", OnCardArtReceived);
	}

	public static void SetMatchId(string matchId)
	{
		_matchId = matchId;
	}

	// Fire-and-forget: asks the server to relay a request for this card's art to the other player in
	// the match. Safe to call repeatedly for the same cardId within a match - only the first call
	// actually sends a request. If the request never gets answered, the caller keeps whatever
	// placeholder sprite it already assigned; there is no timeout here by design, since a repeated
	// call to GetSpriteByCardID naturally keeps returning the placeholder until this cache is filled.
	//
	// ownerName scopes the cache/in-flight-request bookkeeping: cardId alone (an author-typed mod id)
	// is not guaranteed unique across different players' custom cards, so a cardId already cached or
	// in-flight under a different owner is treated as stale and re-requested rather than trusted.
	public static async void RequestArtIfNeeded(string cardId, string ownerName)
	{
		if (_client == null || string.IsNullOrEmpty(cardId)) return;

		if (_receivedArt.ContainsKey(cardId))
		{
			if (_receivedArtOwner.TryGetValue(cardId, out var cachedOwner) && cachedOwner == ownerName) return;

			_receivedArt.Remove(cardId);
			_receivedArtOwner.Remove(cardId);
		}

		if (_requestedIds.TryGetValue(cardId, out var pendingOwner) && pendingOwner == ownerName) return;

		_requestedIds[cardId] = ownerName;

		try
		{
			await _client.InvokeAsync<object>("RequestCardArt", _matchId, cardId);
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"CardArtNetworkService: RequestCardArt failed for '{cardId}': {ex.Message}");
		}
	}

	public static bool TryGetReceivedArt(string cardId, out Sprite sprite)
	{
		if (string.IsNullOrEmpty(cardId))
		{
			sprite = null;
			return false;
		}

		return _receivedArt.TryGetValue(cardId, out sprite);
	}

	// The other client is asking US for art for a card it doesn't recognize. Look it up locally by id
	// (built-in cards are looked up via CardManager the same way any other card id is resolved) and
	// send back its Sprite as PNG bytes if we have one. Silently does nothing if we don't have it -
	// the requester just keeps its placeholder.
	private static async void OnCardArtRequested(string matchId, string cardId)
	{
		var definition = Common.Instance != null ? Common.Instance.CardManager.GetCardByID(cardId) : null;
		if (definition == null || definition.Sprite == null) return;

		byte[] bytes = EncodeSpriteToPng(definition.Sprite);
		if (bytes == null) return;

		try
		{
			await _client.InvokeAsync<object>("SubmitCardArt", matchId, cardId, bytes);
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"CardArtNetworkService: SubmitCardArt failed for '{cardId}': {ex.Message}");
		}
	}

	// Fired after a cardId's sprite is cached, so anything already rendering that card's placeholder
	// (built before this round trip completed) can refresh without needing to be rebuilt from
	// scratch. Setup() alone only picks up art that had already arrived by the time it ran.
	public static event Action<string> OnArtReceived;

	private static void OnCardArtReceived(string cardId, byte[] imageBytes)
	{
		if (imageBytes == null || imageBytes.Length == 0) return;

		var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
		if (!texture.LoadImage(imageBytes))
		{
			Debug.LogWarning($"CardArtNetworkService: failed to decode received art for '{cardId}'.");
			return;
		}

		var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		_receivedArt[cardId] = sprite;

		if (_requestedIds.TryGetValue(cardId, out var ownerName))
		{
			_receivedArtOwner[cardId] = ownerName;
		}

		OnArtReceived?.Invoke(cardId);
	}

	// Reads pixels back through a temporary RenderTexture instead of calling EncodeToPNG directly on
	// the source texture, because most card art is imported non-readable (compressed, no CPU copy) for
	// memory/performance - this works regardless of the source texture's Read/Write import setting, so
	// no import-setting changes are needed on existing card art assets.
	private static byte[] EncodeSpriteToPng(Sprite sprite)
	{
		if (sprite == null || sprite.texture == null) return null;

		var sourceTexture = sprite.texture;
		float scale = Mathf.Min(1f, (float)MaxArtDimension / Mathf.Max(sourceTexture.width, sourceTexture.height));
		int width = Mathf.Max(1, Mathf.FloorToInt(sourceTexture.width * scale));
		int height = Mathf.Max(1, Mathf.FloorToInt(sourceTexture.height * scale));
		while (true)
		{
			var renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
			var previous = RenderTexture.active;
			Texture2D readableTexture = null;
			byte[] bytes;
			try
			{
				Graphics.Blit(sourceTexture, renderTexture);
				RenderTexture.active = renderTexture;
				readableTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
				readableTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
				readableTexture.Apply();
				bytes = readableTexture.EncodeToPNG();
			}
			finally
			{
				if (readableTexture != null)
				{
					if (Application.isPlaying) UnityEngine.Object.Destroy(readableTexture);
					else UnityEngine.Object.DestroyImmediate(readableTexture);
				}
				RenderTexture.active = previous;
				RenderTexture.ReleaseTemporary(renderTexture);
			}
			if (bytes.Length <= MaxArtBytes) return bytes;
			// Noisy/modded artwork can compress poorly even at the dimension cap.
			if (width == 1 && height == 1) return null;
			width = Mathf.Max(1, width / 2);
			height = Mathf.Max(1, height / 2);
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

public static class CardArtNetworkService
{
	private static MiniSignalRClient _client;
	private static Guid _matchId;
	private static readonly Dictionary<string, Sprite> _receivedArt = new Dictionary<string, Sprite>();
	private static readonly HashSet<string> _requestedIds = new HashSet<string>();

	public static void Initialize(MiniSignalRClient client)
	{
		_client = client;
		_receivedArt.Clear();
		_requestedIds.Clear();

		_client.On<CardArtRequestedPayload>("OnCardArtRequested", OnCardArtRequested);
		_client.On<CardArtReceivedPayload>("OnCardArtReceived", OnCardArtReceived);
	}

	public static void SetMatchId(Guid matchId)
	{
		_matchId = matchId;
	}

	// Fire-and-forget: asks the server to relay a request for this card's art to the other player in
	// the match. Safe to call repeatedly for the same cardId within a match - only the first call
	// actually sends a request. If the request never gets answered, the caller keeps whatever
	// placeholder sprite it already assigned; there is no timeout here by design, since a repeated
	// call to GetSpriteByCardID naturally keeps returning the placeholder until this cache is filled.
	public static async void RequestArtIfNeeded(string cardId)
	{
		if (_client == null || string.IsNullOrEmpty(cardId)) return;
		if (_receivedArt.ContainsKey(cardId) || _requestedIds.Contains(cardId)) return;

		_requestedIds.Add(cardId);

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
	private static async void OnCardArtRequested(CardArtRequestedPayload payload)
	{
		var definition = Common.Instance != null ? Common.Instance.CardManager.GetCardByID(payload.CardId) : null;
		if (definition == null || definition.Sprite == null) return;

		byte[] bytes = EncodeSpriteToPng(definition.Sprite);
		if (bytes == null) return;

		try
		{
			await _client.InvokeAsync<object>("SubmitCardArt", payload.MatchId, payload.CardId, bytes);
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"CardArtNetworkService: SubmitCardArt failed for '{payload.CardId}': {ex.Message}");
		}
	}

	private static void OnCardArtReceived(CardArtReceivedPayload payload)
	{
		if (payload.ImageBytes == null || payload.ImageBytes.Length == 0) return;

		var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
		if (!texture.LoadImage(payload.ImageBytes))
		{
			Debug.LogWarning($"CardArtNetworkService: failed to decode received art for '{payload.CardId}'.");
			return;
		}

		var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
		_receivedArt[payload.CardId] = sprite;
	}

	// Reads pixels back through a temporary RenderTexture instead of calling EncodeToPNG directly on
	// the source texture, because most card art is imported non-readable (compressed, no CPU copy) for
	// memory/performance - this works regardless of the source texture's Read/Write import setting, so
	// no import-setting changes are needed on existing card art assets.
	private static byte[] EncodeSpriteToPng(Sprite sprite)
	{
		if (sprite == null || sprite.texture == null) return null;

		var sourceTexture = sprite.texture;
		var renderTexture = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.ARGB32);
		var previous = RenderTexture.active;

		try
		{
			Graphics.Blit(sourceTexture, renderTexture);
			RenderTexture.active = renderTexture;

			var readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);
			readableTexture.ReadPixels(new Rect(0, 0, sourceTexture.width, sourceTexture.height), 0, 0);
			readableTexture.Apply();

			byte[] bytes = readableTexture.EncodeToPNG();
			UnityEngine.Object.Destroy(readableTexture);
			return bytes;
		}
		finally
		{
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(renderTexture);
		}
	}

	private class CardArtRequestedPayload
	{
		public Guid MatchId;
		public string CardId;
	}

	private class CardArtReceivedPayload
	{
		public string CardId;
		public byte[] ImageBytes;
	}
}

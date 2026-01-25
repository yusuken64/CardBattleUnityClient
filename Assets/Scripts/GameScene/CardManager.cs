using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CardManager : MonoBehaviour
{
    [Header("Card Data")]
    public List<CardDefinition> Cards;

    public bool AddDebugCards;
    [Header("Debug Cards")]
    public List<CardDefinition> DebugCards;

	public DeckDefinition AdventureStartDeck;

    [Header("Fallbacks")]
    public Sprite DefaultMissingSprite;

    private Dictionary<string, CardDefinition> _cardLookup;

    private void Awake()
	{
		_cardLookup = new Dictionary<string, CardDefinition>();

		foreach (var card in AllCards())
		{
			if (card == null || string.IsNullOrEmpty(card.ID))
			{
				Debug.LogWarning("CardManager: A card entry is null or missing a name.");
				continue;
			}

			if (_cardLookup.ContainsKey(card.ID))
			{
				Debug.LogWarning(
					$"CardManager: Duplicate card ID '{card.ID}' found. " +
					$"Keeping the first one and ignoring the duplicate.");
				continue;
			}

			_cardLookup.Add(card.ID, card);
		}
	}

	public List<CardDefinition> CollectableCards()
	{
		return AllCards().Where(x => x.Collectable).ToList();
	}

	public List<CardDefinition> AllCards()
	{
		var cardsToAdd = new List<CardDefinition>();
		cardsToAdd.AddRange(Cards);
		cardsToAdd.AddRange(AdventureStartDeck.Cards);
		if (AddDebugCards)
		{
			cardsToAdd.AddRange(DebugCards);
		}

		return cardsToAdd
			.GroupBy(c => c.ID)
			.Select(g => g.First())
			.ToList();
	}

	public CardDefinition GetCardByID(string id)
    {
		if (string.IsNullOrWhiteSpace(id)) { return null; }
        return _cardLookup.TryGetValue(id, out var card) ? card : null;
    }

    public Sprite GetSpriteByCardID(string id)
    {
        return GetCardByID(id)?.Sprite ?? DefaultMissingSprite;
    }

#if UNITY_EDITOR
	[ContextMenu("RebuildCardList")]
	public void RebuildCardList()
	{
		Cards.Clear();
		string[] guids = AssetDatabase.FindAssets("t:CardDefinition");

		foreach (var guid in guids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			var card = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
			if (card != null)
			{
				Cards.Add(card);
			}
		}
		EditorUtility.SetDirty(this);
	}
#endif
}
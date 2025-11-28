using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Card Data")]
    public List<CardDefinition> Cards;

    [Header("Fallbacks")]
    public Sprite DefaultMissingSprite;

    private Dictionary<string, CardDefinition> _cardLookup;

    private void Awake()
    {
        _cardLookup = new Dictionary<string, CardDefinition>();

        foreach (var card in Cards)
        {
            if (card == null || string.IsNullOrEmpty(card.CardName))
            {
                Debug.LogWarning("CardManager: A card entry is null or missing a name.");
                continue;
            }

            if (_cardLookup.ContainsKey(card.CardName))
            {
                Debug.LogWarning(
                    $"CardManager: Duplicate card name '{card.CardName}' found. " +
                    $"Keeping the first one and ignoring the duplicate.");
                continue;
            }

            _cardLookup.Add(card.CardName, card);
        }
    }

    public CardDefinition GetCardByName(string name)
    {
        return _cardLookup.TryGetValue(name, out var card) ? card : null;
    }

    public Sprite GetSpriteByCardName(string name)
    {
        return GetCardByName(name)?.Sprite ?? DefaultMissingSprite;
    }
}
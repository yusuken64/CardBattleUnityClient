using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VerticalDeckViewer : MonoBehaviour
{
    public Image HeroImage;

    public Transform VerticalContainer;
    public DeckCard DeckItemPrefab;

    private readonly List<DeckCard> _spawnedCards = new List<DeckCard>();

    public DeckCard HeroCard;

    public TextMeshProUGUI TitleText;
    public TMP_InputField TitleInput;

    public TextMeshProUGUI CountText;
	private Deck editingDeck;

	public Action<Deck> DeckClosedAction { get; internal set; }

	private void Start()
    {
        TitleInput.gameObject.SetActive(false);
    }

    internal void Setup(Deck deck)
    {
        editingDeck = deck;
        TitleText.text = deck.Title;

        Clear();
        foreach (var cardDefinition in deck.Cards)
        {
            AddCardToDeck(cardDefinition);
        }
    }

    public void Clear()
    {
        foreach (Transform child in VerticalContainer)
            Destroy(child.gameObject);

        _spawnedCards.Clear();
        SortAndReorder();
    }

    public void AddCardToDeck(CardDefinition cardDefinition)
    {
        var newDeckItem = Instantiate(DeckItemPrefab, VerticalContainer);
        newDeckItem.Setup(cardDefinition, RemoveCardFromDeck, SetCardAsHero);

        if (_spawnedCards.Count == 0)
        {
            newDeckItem.SetAsHero_Click();
        }
        else
        {
            newDeckItem.SetAsHero(false);
        }
        _spawnedCards.Add(newDeckItem);

        SortAndReorder();
    }

    private void SortAndReorder()
    {
        // Sort by ManaCost then Name
        _spawnedCards.Sort((a, b) =>
        {
            int manaCompare = a.CardDefinition.Cost.CompareTo(b.CardDefinition.Cost);
            if (manaCompare != 0) return manaCompare;

            return string.Compare(a.CardDefinition.CardName, b.CardDefinition.CardName, StringComparison.Ordinal);
        });

        // Reorder hierarchy
        for (int i = 0; i < _spawnedCards.Count; i++)
        {
            _spawnedCards[i].transform.SetSiblingIndex(i);
        }

        CountText.text = $"{_spawnedCards.Count}/30";
    }

    public void SetCardAsHero(DeckCard deckCard)
    {
        HeroCard = deckCard;
        HeroImage.sprite = deckCard.CardDefinition.Sprite;

        foreach (var card in _spawnedCards)
        {
            card.SetAsHero(HeroCard == card);
        }
    }

    public void RemoveCardFromDeck(DeckCard deckCard)
    {
        if (deckCard == null)
            return;

        if (_spawnedCards.Remove(deckCard))
        {
            Destroy(deckCard.gameObject);
            SortAndReorder();
        }
    }

    public void Title_Click()
    {
        TitleInput.gameObject.SetActive(true);
        TitleInput.text = TitleText.text;
        TitleInput.Select();
    }

    public void TitleEdit_End(string title)
    {
        TitleText.text = title;
        TitleInput.gameObject.SetActive(false);
    }

    public void DeckSave_Clicked()
    {
        editingDeck.Title = TitleText.text;
        editingDeck.Cards = _spawnedCards.Select(x => x.CardDefinition).ToList();
        editingDeck.HeroCard = _spawnedCards.FirstOrDefault(x => x.HeroIndicator.DeckCard == HeroCard).CardDefinition;
        DeckClosedAction?.Invoke(editingDeck);
    }

    public Deck GetDeck()
	{
        editingDeck.Title = TitleText.text;
        editingDeck.Cards = _spawnedCards.Select(x => x.CardDefinition).ToList();
        editingDeck.HeroCard = _spawnedCards.FirstOrDefault(x => x.HeroIndicator.DeckCard == HeroCard).CardDefinition;

        return editingDeck;
    }

    public void DeckCancel_Clicked()
    {
        //close the deckviewer without saving
        //return to deckpicker
        DeckClosedAction?.Invoke(null);
    }
}

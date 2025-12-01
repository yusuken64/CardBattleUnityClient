using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Collection : MonoBehaviour
{
    public int ItemsPerPage = 8;
    public int CurrentPage = 0;
    public Transform CardGridTransform;
    public CollectionItem CardGridItemPrefab;

    public TextMeshProUGUI PageIndicatorText;
    public Button NextButton;
    public Button PrevButton;

    public CollectionFilter CollectionFilter;

    private CardBattleEngine.Player owner = new CardBattleEngine.Player("Test Player");

	private int MaxPage => Mathf.Max(0, (CardsToShow.Count() - 1) / ItemsPerPage);

	public IEnumerable<CardDefinition> CardsToShow { get; private set; }
    public bool HideUncollectable;

	private void OnEnable()
	{
        CollectionFilter.FilterChanged += CollectionFilter_FilterChanged;
    }

	private void OnDisable()
    {
        CollectionFilter.FilterChanged -= CollectionFilter_FilterChanged;
    }

    private void CollectionFilter_FilterChanged(Func<CardDefinition, bool> filter)
    {
        ResetCardsToShow();
        CardsToShow = CardsToShow.Where(filter);
        SetToPage(CurrentPage);
    }

    private void Start()
	{
		ResetCardsToShow();
		SetToPage(CurrentPage);
	}

	private void ResetCardsToShow()
	{
		if (HideUncollectable)
		{
			CardsToShow = Common.Instance.CardManager.AllCards().Where(x => x.Collectable);
		}
		else
		{
			CardsToShow = Common.Instance.CardManager.AllCards();
		}
	}

	public void SetToPage(int page)
    {
        // Clamp page number
        int maxPage = Mathf.Max(0, (CardsToShow.Count() - 1) / ItemsPerPage);
        CurrentPage = Mathf.Clamp(page, 0, maxPage);

        // Clear existing contents
        foreach (Transform child in CardGridTransform)
            Destroy(child.gameObject);

        // Load items
        var itemsToShow = CardsToShow
            .OrderBy(x => x.Cost)
            .Skip(ItemsPerPage * CurrentPage)
            .Take(ItemsPerPage)
            .ToList();

        foreach (var item in itemsToShow)
        {
            var newGridItem = Instantiate(CardGridItemPrefab, CardGridTransform);
            newGridItem.Setup(item, owner);
        }

        UpdatePageIndicator();
    }

    public void NextPage()
    {
        if (CurrentPage < MaxPage)
            SetToPage(CurrentPage + 1);
    }

    public void PreviousPage()
    {
        if (CurrentPage > 0)
            SetToPage(CurrentPage - 1);
    }

    private void UpdatePageIndicator()
    {
        if (PageIndicatorText != null)
            PageIndicatorText.text = $"Page {CurrentPage + 1} / {MaxPage + 1}";

        NextButton.interactable = CurrentPage < MaxPage;
        PrevButton.interactable = CurrentPage > 0;
    }
}
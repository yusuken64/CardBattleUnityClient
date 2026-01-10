using System.Collections.Generic;
using UnityEngine;

public class DeckBuilderTutorial : MonoBehaviour
{
	public List<GameObject> Pages;
	private int currentPage = 0;

	public CollectionItem CollectionItem;
	public CardDefinition SampleCard;

	private void Start()
	{
		CollectionItem.Setup(SampleCard, null, new OwnedCardData()
		{
			CardID = SampleCard.ID,
			Count = 1
		});
		this.gameObject.SetActive(false);
	}

	public void Show()
	{
		this.gameObject.SetActive(true);
		currentPage = 0;
		UpdateShowingPage();
	}

	public void Next_Click()
	{
		if (currentPage == Pages.Count - 1)
		{
			Close_Click();
			return;
		}

		currentPage++;
		currentPage = Mathf.Clamp(currentPage, 0, Pages.Count - 1);
		UpdateShowingPage();
	}

	public void Prev_Click()
	{
		currentPage--;
		currentPage = Mathf.Clamp(currentPage, 0, Pages.Count - 1);
		UpdateShowingPage();
	}

	private void UpdateShowingPage()
	{
		for (int i = 0; i < Pages.Count; i++)
		{
			GameObject page = Pages[i];
			page.SetActive(i == currentPage);
		}
	}

	public void Close_Click()
	{
		this.gameObject.SetActive(false);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MulliganPrompt : MonoBehaviour
{
	public GameManager GameManager;
	public Transform Container;
	public MulliganOption MulliganItemPrefab;

	public List<MulliganOption> Items;
	public RectTransform CardArea;

	public void Setup(List<Card> cards)
	{
		ClearItems();

		foreach (var card in cards)
		{
			var newItem = Instantiate(MulliganItemPrefab, Container);
			newItem.Setup(card);

			Items.Add(newItem);
		}

		UpdatePositions();
	}

	private void ClearItems()
	{
		foreach (Transform child in Container)
		{
			Destroy(child.gameObject);
		}
		Items.Clear();
	}

	private void UpdatePositions()
	{
		float width = CardArea.rect.width;

		int count = Items.Count;
		float step = width / (count + 1);

		for (int i = 0; i < count; i++)
		{
			var item = Items[i];
			RectTransform itemRT = item.GetComponent<RectTransform>();

			// we anchor to the left-middle so x is measured from left edge
			itemRT.anchorMin = new Vector2(0, 0.5f);
			itemRT.anchorMax = new Vector2(0, 0.5f);
			itemRT.pivot = new Vector2(0.5f, 0.5f);

			float x = step * (i + 1);  // skip first gap
			itemRT.anchoredPosition = new Vector2(x, 0);
		}
	}

	public void Submit_Clicked()
	{
		var mulliganChoice = GameManager._gameState.PendingChoice;
		mulliganChoice.GetActions(GameManager._gameState);
		(CardBattleEngine.IGameAction action, CardBattleEngine.ActionContext context) = mulliganChoice.Options.First();

		var cardsToMulligen = Items.Where(x => !x.Keep);
		var replaceData = cardsToMulligen.Select(x => x.Card.Data).ToList();
		((CardBattleEngine.SubmitMulliganAction)action).CardsToReplace = replaceData;

		GameManager.ResolveAction(action, context);

		foreach (var item in Items)
		{
			if (item.Keep)
			{
				//reparent to hand
				item.Card.Dragging = false;
				item.Card.transform.parent = this.GameManager.Player.Hand.transform;
				item.Card.GetComponent<BoxCollider2D>().enabled = true;
			}
			else
			{
				this.GameManager.Player.Hand.Cards.Remove(item.Card);
				Destroy(item.gameObject);
			}
		}
		this.GameManager.Player.Hand.UpdateCardPositions();

		ClearItems();
		this.gameObject.SetActive(false);
	}
}

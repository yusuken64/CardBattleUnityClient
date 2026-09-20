using CardBattleEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MulliganPrompt : MonoBehaviour
{
	public GameManager GameManager;
	public Transform Container;
	public MulliganOption MulliganItemPrefab;
	public GameObject SubmitButton;

	public List<MulliganOption> Items;
	public RectTransform CardArea;

	public Image BG;
	public CanvasGroup CanvasGroup;

	private bool _isNetworked = false;
	private List<CardBattleEngine.Card> _stagedSelection;
	private bool _hasStaged = false;
	private bool _submitted;

	public void Setup(List<Card> cards)
	{
		_isNetworked = false;
		_hasStaged = false;
		_submitted = false;
		CanvasGroup.interactable = CanvasGroup.blocksRaycasts = true;
		ClearItems();

		foreach (var card in cards)
		{
			var newItem = Instantiate(MulliganItemPrefab, Container);
			newItem.Setup(card);

			Items.Add(newItem);
		}

		UpdatePositions();

		SubmitButton.gameObject.SetActive(true);
		CanvasGroup.alpha = 0;
		CanvasGroup.DOFade(245f / 255f, 0.5f);
	}

	public void SetupNetworked(List<CardBattleEngine.Card> cards)
	{
		gameObject.SetActive(true);
		_isNetworked = true;
		_hasStaged = false;
		_stagedSelection = null;
		_submitted = false;
		SetNetworkInteractionState(GameManager.CanChooseNetworkMulligan);
		ClearItems();

		var cardPrefab = FindFirstObjectByType<GameInteractionHandler>().CardPrefab;

		foreach (var cardData in cards)
		{
			var newCard = Instantiate(cardPrefab, Container);
			newCard.Setup(cardData);

			var newItem = Instantiate(MulliganItemPrefab, Container);
			newItem.Setup(newCard);

			Items.Add(newItem);
		}

		UpdatePositions();

		SubmitButton.gameObject.SetActive(true);
		CanvasGroup.alpha = 0;
		CanvasGroup.DOFade(245f / 255f, 0.5f);
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

	// Build the expected display string for a mulligan selection and find matching LegalActionView
	private LegalActionView FindMatchingLegalAction(List<CardBattleEngine.Card> cardsToReplace, List<LegalActionView> options)
	{
		string expectedDisplayName = cardsToReplace.Count == 0
			? "Keep all"
			: "Mulligan: " + string.Join(", ", cardsToReplace.Select(c => c.Name));

		return options.FirstOrDefault(action => action.DisplayName == expectedDisplayName);
	}

	public void SetNetworkInteractionState(bool canChoose)
	{
		CanvasGroup.interactable = canChoose && !_hasStaged && !_submitted;
		// A confirmed choice still covers the board while waiting for its turn.
		CanvasGroup.blocksRaycasts = true;
	}

	private void TrySubmitNetworked(List<CardBattleEngine.Card> cardsToMulligan)
	{
		if (_hasStaged || _submitted || !GameManager.CanChooseNetworkMulligan) return;
		_stagedSelection = cardsToMulligan.ToList();
		_hasStaged = true;
		SubmitButton.SetActive(false);
		SetNetworkInteractionState(false);
		TryHandlePendingChoice(GameManager.LastNetworkView?.PendingChoice);
	}

	public static bool CanSubmitStagedChoice(bool staged, bool submitted, PendingChoiceView choice,
		Guid? localPlayerId, bool canSubmitAction)
	{
		return staged && !submitted && canSubmitAction && choice != null &&
			choice.ChoiceKind == "MulliganChoce" && choice.SourcePlayerId == localPlayerId;
	}

	public void TryHandlePendingChoice(PendingChoiceView pendingChoice)
	{
		if (!CanSubmitStagedChoice(_hasStaged, _submitted, pendingChoice,
			GameManager.LocalPlayerId, GameManager.CanSubmitNetworkAction)) return;
		// Never submit an option from an older displayed snapshot.
		if (!ReferenceEquals(pendingChoice, GameManager.LastNetworkView?.PendingChoice)) return;
		var match = FindMatchingLegalAction(_stagedSelection, pendingChoice.Options);
		if (match == null)
		{
			// Leave the prompt usable if the server no longer offers this selection.
			_hasStaged = false;
			_stagedSelection = null;
			SubmitButton.SetActive(true);
			SetNetworkInteractionState(GameManager.CanChooseNetworkMulligan);
			Debug.LogWarning("The queued mulligan selection is no longer available. Please choose again.");
			return;
		}
		_submitted = true;
		GameManager.SubmitLocalAction(match);
		PerformVisualCleanup();
	}

	private void PerformVisualCleanup()
	{
		// Network cards here are previews; server snapshots own the actual hand.
		// Leave every preview under Container so ClearItems destroys kept cards too.
		SubmitButton.gameObject.SetActive(false);
		CanvasGroup.DOFade(0, 0.5f)
			.OnComplete(() =>
			{
				ClearItems();
				this.gameObject.SetActive(false);
			});
	}

	public void Submit_Clicked()
	{
		if (!_isNetworked)
		{
			// Local mode - existing behavior
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

			SubmitButton.gameObject.SetActive(false);
			CanvasGroup.DOFade(0, 0.5f)
				.OnComplete(() =>
				{
					this.GameManager.Player.Hand.UpdateCardPositions();
					ClearItems();
					this.gameObject.SetActive(false);
				});
		}
		else
		{
			// Networked mode - stage or submit immediately
			var cardsToMulligen = Items.Where(x => !x.Keep).Select(x => x.Card.Data).ToList();
			TrySubmitNetworked(cardsToMulligen);
		}
	}
}

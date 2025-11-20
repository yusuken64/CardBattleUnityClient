using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class GainCardAnimation : GameActionAnimation<GainCardAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public GainCardAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		var player = gameManager.GetPlayerFor(current.context.SourcePlayer);
		var cardPrefab = gameManager.PlayResolver.CardPrefab;
		var newCard = Object.Instantiate(cardPrefab, player.Hand.transform);
		var cardData = (current.action as GainCardAction).Card;
		cardData.Owner = current.context.SourcePlayer;
		newCard.Setup(cardData);
		player.Hand.AddCard(newCard);

		Vector3 worldPos = player.DrawPile.transform.position;
		newCard.transform.position = worldPos;

		yield return null;
	}
}

public class StartTurnAnimation : GameActionAnimation<StartTurnAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public StartTurnAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		if (current.context.SourcePlayer.Name == gameManager.Player.Data.Name)
		{
			var turnStartObject = Object.FindFirstObjectByType<UI>().TurnStartObject;
			turnStartObject.gameObject.SetActive(true);

			// Ensure it starts at normal scale
			turnStartObject.transform.localScale = Vector3.one;

			// Optional: add a CanvasGroup if you want to fade out
			CanvasGroup cg = turnStartObject.GetComponent<CanvasGroup>();
			if (cg == null) cg = turnStartObject.gameObject.AddComponent<CanvasGroup>();
			cg.alpha = 1f;

			// 1. Punch scale for a quick pop
			Tween punch = turnStartObject.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0), 0.3f, 1, 1f);

			// Wait for the punch to finish
			yield return punch.WaitForCompletion();

			yield return new WaitForSecondsRealtime(0.5f);

			// 2. Fast fadeout
			Tween fade = cg.DOFade(0f, 0.2f).SetEase(Ease.InSine);
			yield return fade.WaitForCompletion();

			// Hide the UI
			turnStartObject.gameObject.SetActive(false);

			// Reset alpha
			cg.alpha = 1f;
		}

		yield return null;
	}
}

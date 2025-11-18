using CardBattleEngine;
using System.Collections;
using UnityEngine;

public class DrawCardAnimation : GameActionAnimation<DrawCardFromDeckAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public DrawCardAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
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
		player.Hand.AddCard(newCard);

		Vector3 worldPos = player.DrawPile.transform.position;
		newCard.transform.position = worldPos;

		yield return null;

	}
}

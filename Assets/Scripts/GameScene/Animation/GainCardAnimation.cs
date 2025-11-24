using CardBattleEngine;
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
		var cardPrefab = Object.FindAnyObjectByType<GameInteractionHandler>().CardPrefab;
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

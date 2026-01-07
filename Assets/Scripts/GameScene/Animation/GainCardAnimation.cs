using CardBattleEngine;
using System.Collections;
using UnityEngine;

public class GainCardAnimation : GameActionAnimation<GainCardAction>
{
	public override IEnumerator Play()
	{
		var player = GameManager.GetPlayerFor(Context.SourcePlayer);
		var cardPrefab = Object.FindAnyObjectByType<GameInteractionHandler>().CardPrefab;
		var newCard = Object.Instantiate(cardPrefab, player.Hand.transform);
		var cardData = Action.Card;
		cardData.Owner = Context.SourcePlayer;
		newCard.Setup(cardData);
		player.Hand.AddCard(newCard);

		Vector3 worldPos = player.DrawPile.transform.position;
		newCard.transform.position = worldPos;

		yield return null;
	}
}

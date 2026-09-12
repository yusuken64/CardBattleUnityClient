using CardBattleEngine;
using System.Collections;
using UnityEngine;

public class GainCardAnimation : GameActionAnimation<GainCardAction>
{
	public override IEnumerator Play()
	{
		var player = GameManager.GetPlayerFor(Presentation.SourcePlayer);
		var cardPrefab = Object.FindAnyObjectByType<GameInteractionHandler>().CardPrefab;
		var newCard = Object.Instantiate(cardPrefab, player.Hand.transform);
		var cardData = Presentation.CardGained;
        if (cardData != null) newCard.Setup(cardData);
		player.Hand.AddCard(newCard);

		Vector3 worldPos = player.DrawPile.transform.position;
		newCard.transform.position = worldPos;

		yield return null;
	}
}

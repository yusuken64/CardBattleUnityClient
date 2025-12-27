using CardBattleEngine;
using System.Collections;

public class DrawCardFromDeckAnimation : GameActionAnimation<DrawCardFromDeckAction>
{
	public override IEnumerator Play()
	{
		var player = GameManager.GetPlayerFor(Context.SourcePlayer);
		player.CardsLeftInDeck = Context.CardsLeftInDeck;
		player.UpdateUI();

		yield return null;
	}
}

using CardBattleEngine;
using System.Collections;
using UnityEngine;

public class EndTurnAnimation : GameActionAnimation<EndTurnAction>
{
	public override IEnumerator Play()
	{
		yield return new WaitForSecondsRealtime(0.5f);

		var player = GameManager.GetPlayerFor(Context.SourcePlayer);
		if (Context.SourcePlayer == GameManager.Opponent.Data)
		{
			GameManager.OpponentTurn = false;
			yield return new WaitForSecondsRealtime(1.0f);
		}

		//player.RefreshData();
	}
}
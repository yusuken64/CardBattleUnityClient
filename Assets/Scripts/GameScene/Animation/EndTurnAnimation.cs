using CardBattleEngine;
using System.Collections;
using UnityEngine;

public class EndTurnAnimation : GameActionAnimation<EndTurnAction>
{
	public override IEnumerator Play()
	{
		var player = GameManager.GetPlayerFor(Context.SourcePlayer);
		if (Context.SourcePlayer == GameManager.Opponent.Data)
		{
			yield return new WaitForSecondsRealtime(0.5f);

			GameManager.OpponentTurn = false;
			yield return new WaitForSecondsRealtime(1.0f);
		}
		else
		{
			var endTurnButton = FindFirstObjectByType<UI>().EndTurnButton;
			endTurnButton.SetToEnemyTurn();
		}

		//player.RefreshData();
	}
}
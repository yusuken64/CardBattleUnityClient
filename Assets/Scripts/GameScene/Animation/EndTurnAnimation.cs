using CardBattleEngine;
using System.Collections;
using UnityEngine;

public class EndTurnAnimation : GameActionAnimation<EndTurnAction>
{
	public override IEnumerator Play()
	{
		var player = GameManager.GetPlayerFor(Presentation.SourcePlayer);
		if (Presentation.SourcePlayer.Id == GameManager.Opponent.Data.Id)
		{
			yield return new WaitForSecondsRealtime(0.5f);


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
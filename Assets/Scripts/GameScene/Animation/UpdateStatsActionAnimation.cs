using CardBattleEngine;
using System.Collections;
using System.Linq;

public class UpdateStatsActionAnimation : GameActionAnimation<AddStatModifierAction>
{
	public override IEnumerator Play()
	{
		var player = GameManager.GetPlayerFor(Context.SourcePlayer);
		var minion = player.Board.Minions.FirstOrDefault(x => x.Data == Context.Target);
		if (minion != null)
		{
			minion.RefreshData();
		}

		yield return null;
	}
}

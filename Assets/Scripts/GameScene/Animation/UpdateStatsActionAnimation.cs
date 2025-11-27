using CardBattleEngine;
using System.Collections;
using System.Linq;

public class UpdateStatsActionAnimation : GameActionAnimation<AddStatModifierAction>
{
	public UpdateStatsActionAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current) : base(gameManager, state, current)
	{
	}

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

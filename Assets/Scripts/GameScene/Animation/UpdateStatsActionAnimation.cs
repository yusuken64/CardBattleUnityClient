using CardBattleEngine;
using System.Collections;
using System.Linq;

public class UpdateStatsActionAnimation : GameActionAnimation<AddStatModifierAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public UpdateStatsActionAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}
	public override IEnumerator Play()
	{
		var player = gameManager.GetPlayerFor(current.context.SourcePlayer);
		var minion = player.Board.Minions.FirstOrDefault(x => x.Data == current.context.Target);
		if (minion != null)
		{
			minion.RefreshData(true);
		}

		yield return null;
	}
}

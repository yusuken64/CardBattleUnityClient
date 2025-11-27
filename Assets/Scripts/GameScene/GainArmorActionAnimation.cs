using CardBattleEngine;
using System.Collections;

public class GainArmorActionAnimation : GameActionAnimation<GainArmorAction>
{
	public GainArmorActionAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current) : base(gameManager, state, current)
	{
	}

	public override IEnumerator Play()
	{
		var player = GameManager.GetPlayerFor(Context.Target as CardBattleEngine.Player);
		player.RefreshData();

		yield return null;
	}
}
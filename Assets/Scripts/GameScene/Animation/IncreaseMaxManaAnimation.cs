using CardBattleEngine;
using System.Collections;

public class IncreaseMaxManaAnimation : GameActionAnimation<IncreaseMaxManaAction>
{
	public IncreaseMaxManaAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current) : base(gameManager, state, current)
	{
	}

	public override IEnumerator Play()
	{
		var player = this.GameManager.GetPlayerFor(Context.SourcePlayer);
		player.MaxMana = Context.SourcePlayer.MaxMana;
		player.UpdateUI();
		yield return null;
	}
}

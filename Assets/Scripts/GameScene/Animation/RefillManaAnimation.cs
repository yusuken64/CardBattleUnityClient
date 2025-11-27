using CardBattleEngine;
using System.Collections;

public class RefillManaAnimation : GameActionAnimation<RefillManaAction>
{
	public RefillManaAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current) : base(gameManager, state, current)
	{
	}

	public override IEnumerator Play()
	{
		var player = this.GameManager.GetPlayerFor(Context.SourcePlayer);
		player.Mana = Context.SourcePlayer.MaxMana;
		player.UpdateUI();
		yield return null;
	}
}

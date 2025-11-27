using CardBattleEngine;
using System.Collections;

public class SpendManaAnimation : GameActionAnimation<SpendManaAction>
{
	public SpendManaAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current) : base(gameManager, state, current)
	{
	}

	public override IEnumerator Play()
	{
		var spendManaAction = Action;
		var player = GameManager.GetPlayerFor(Context.SourcePlayer);
		player.Mana -= spendManaAction.Amount;
		player.UpdateUI();

		yield return null;
	}
}

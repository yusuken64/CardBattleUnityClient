using CardBattleEngine;
using System.Collections;

public class SpendManaAnimation : GameActionAnimation<SpendManaAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public SpendManaAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		var spendManaAction = current.action as SpendManaAction;
		var player = gameManager.GetPlayerFor(current.context.SourcePlayer);
		player.Mana -= spendManaAction.Amount;
		player.UpdateUI();

		yield return null;
	}
}

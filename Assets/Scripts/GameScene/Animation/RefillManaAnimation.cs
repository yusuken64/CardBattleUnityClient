using CardBattleEngine;
using System.Collections;

public class RefillManaAnimation : GameActionAnimation<RefillManaAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public RefillManaAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		var player = this.gameManager.GetPlayerFor(current.context.SourcePlayer);
		player.Mana = current.context.SourcePlayer.MaxMana;
		player.UpdateUI();
		yield return null;
	}
}

public class IncreaseMaxManaAnimation : GameActionAnimation<IncreaseMaxManaAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public IncreaseMaxManaAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}
	public override IEnumerator Play()
	{
		var player = this.gameManager.GetPlayerFor(current.context.SourcePlayer);
		player.MaxMana = current.context.SourcePlayer.MaxMana;
		player.UpdateUI();
		yield return null;
	}
}

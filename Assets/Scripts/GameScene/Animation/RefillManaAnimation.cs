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
		player.ManaText.text = $"{player.Data.Mana}/{player.Data.MaxMana}";
		yield return null;
	}
}

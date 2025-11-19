using CardBattleEngine;
using System.Collections;

public class PlayCardAnimation : GameActionAnimation<PlayCardAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public PlayCardAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		var player = this.gameManager.GetPlayerFor((current.action as PlayCardAction).Card.Owner);
		player.ManaText.text = $"{player.Data.Mana}/{player.Data.MaxMana}";
		yield return null;
	}
}

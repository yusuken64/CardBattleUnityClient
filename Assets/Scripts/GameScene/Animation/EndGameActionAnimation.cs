using CardBattleEngine;
using System.Collections;
using UnityEngine;
public class EndGameActionAnimation : GameActionAnimation<EndGameAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public EndGameActionAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		if (current.action is EndGameAction endGameAction)
		{
			var ui = Object.FindFirstObjectByType<UI>();
			var gameManager = Object.FindFirstObjectByType<GameManager>();
			var isWin = gameManager.Player.Data == current.context.SourcePlayer;
			yield return ui.StartCoroutine(ui.DoGameEndRoutine(isWin));
		}
	}
}

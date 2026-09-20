using System;
using CardBattleEngine;
using UnityEngine;

public static class GameActionSubmitter
{
	/// <summary>
	/// Routes an action submission through the appropriate path: local resolution for LocalTest mode,
	/// or network submission for Networked mode.
	/// </summary>
	/// <param name="gameManager">The GameManager instance managing the current match</param>
	/// <param name="action">The game action to submit</param>
	/// <param name="context">The action context (source, target, etc.)</param>
	public static void Submit(GameManager gameManager, IGameAction action, ActionContext context)
	{
		gameManager.QueuePlayerAction(action, context);
	}
}

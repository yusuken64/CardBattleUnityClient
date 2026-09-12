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
		// Local mode: args not set, or explicitly LocalTest
		if (gameManager.Args == null || gameManager.Args.Mode == GameMode.LocalTest)
		{
			gameManager.ResolveAction(action, context);
			return;
		}

		if (gameManager.TryFindNetworkAction(action, context, out var legalAction))
		{
			gameManager.SubmitLocalAction(legalAction);
			return;
		}

		string actionType = action.GetType().Name;
		Guid? sourceId = context.Source?.Id ?? context.SourceCard?.Id;
		Guid? targetId = context.Target?.Id;
		Debug.LogWarning($"No matching legal action for {actionType} (source={sourceId}, target={targetId})");
	}
}

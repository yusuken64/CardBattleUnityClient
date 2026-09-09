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

		// Networked mode: find matching legal action and submit it
		if (gameManager.LastNetworkView?.LegalActions == null)
		{
			Debug.LogWarning("No legal actions available from network view.");
			return;
		}

		string actionType = action.GetType().Name;
		Guid? sourceId = (context.Source as IGameEntity)?.Id;
		Guid? targetId = (context.Target as IGameEntity)?.Id;

		// Search for the first matching legal action
		foreach (var legalAction in gameManager.LastNetworkView.LegalActions)
		{
			if (legalAction.ActionType == actionType &&
				legalAction.SourceEntityId == sourceId &&
				legalAction.TargetEntityId == targetId)
			{
				gameManager.SubmitLocalAction(legalAction);
				return;
			}
		}

		// No matching legal action found
		Debug.LogWarning($"No matching legal action for {actionType} (source={sourceId}, target={targetId})");
	}
}

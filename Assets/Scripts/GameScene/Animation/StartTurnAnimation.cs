using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class StartTurnAnimation : GameActionAnimation<StartTurnAction>
{
	public StartTurnAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current) : base(gameManager, state, current)
	{
	}

	public override IEnumerator Play()
	{
		var player = GameManager.GetPlayerFor(Context.SourcePlayer);
		var opponent = GameManager.GetPlayerFor(State.OpponentOf(Context.SourcePlayer));

		if (Context.SourcePlayer == GameManager.Player.Data)
		{
			var turnStartObject = Object.FindFirstObjectByType<UI>().TurnStartObject;
			turnStartObject.gameObject.SetActive(true);

			// Ensure it starts at normal scale
			turnStartObject.transform.localScale = Vector3.one;

			// Optional: add a CanvasGroup if you want to fade out
			CanvasGroup cg = turnStartObject.GetComponent<CanvasGroup>();
			if (cg == null) cg = turnStartObject.gameObject.AddComponent<CanvasGroup>();
			cg.alpha = 1f;

			// 1. Punch scale for a quick pop
			Tween punch = turnStartObject.transform.DOPunchScale(new Vector3(0.5f, 0.5f, 0), 0.3f, 1, 1f);

			// Wait for the punch to finish
			yield return punch.WaitForCompletion();

			yield return new WaitForSecondsRealtime(0.5f);

			// 2. Fast fadeout
			Tween fade = cg.DOFade(0f, 0.2f).SetEase(Ease.InSine);
			yield return fade.WaitForCompletion();

			// Hide the UI
			turnStartObject.gameObject.SetActive(false);

			// Reset alpha
			cg.alpha = 1f;

			yield return new WaitForSecondsRealtime(0.3f);

			Object.FindFirstObjectByType<UI>().EndTurnButton.SetToReady();
		}

		yield return new WaitForSecondsRealtime(0.5f);

		player.RefreshData();
		opponent.RefreshData();
	}
}

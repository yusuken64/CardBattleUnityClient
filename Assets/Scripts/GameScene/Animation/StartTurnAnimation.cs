using CardBattleEngine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StartTurnAnimation : GameActionAnimation<StartTurnAction>
{
	public AudioClip StartTurnSound;
	public override IEnumerator Play()
	{
		yield return new WaitForSecondsRealtime(0.5f);
		var player = GameManager.GetPlayerFor(Context.SourcePlayer);
		var opponent = GameManager.GetPlayerFor(ClonedState.OpponentOf(Context.SourcePlayer));

		if (Context.SourcePlayer == GameManager.Player.Data)
		{
			player.RefreshData();
			Common.Instance.AudioManager.PlaySound(StartTurnSound);
			var turnStartObject = FindFirstObjectByType<UI>().TurnStartObject;
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

			FindFirstObjectByType<UI>().EndTurnButton.SetToReady();
			GameManager.ActivePlayerTurn = true;
			player.UpdatePlayableActions(GameManager.ActivePlayerTurn);
#if UNITY_EDITOR
			ValidateState(Context.SourcePlayer, player);
#endif
		}
		else
		{
			GameManager.OpponentTurn = true;
			GameManager.ActivePlayerTurn = false;
			GameManager.Player.UpdatePlayableActions(GameManager.ActivePlayerTurn);
			yield return new WaitForSecondsRealtime(1.0f);

			GameManager.ProcessEnemyMove(ClonedState);
		}
	}

	public void ValidateState(CardBattleEngine.Player data, Player player)
	{
		if (player.Board.Minions.Any(x => x == null))
		{
			throw new Exception($"null minion");
		}

		if (player.Board.Minions.Count() != data.Board.Count())
		{
			Debug.LogError("Minion count mismatch");
			return;
		}

		for (int i = 0; i < data.Board.Count; i++)
		{
			CardBattleEngine.Minion minionData = data.Board[i];
			var boardMinion = player.Board.Minions[i];

			AssertAreEqual(boardMinion.Data, minionData);
			AssertAreEqual(boardMinion.Attack, minionData.Attack);
			AssertAreEqual(boardMinion.Health, minionData.Health);
			AssertAreEqual(boardMinion.CanAttack, minionData.CanAttack());
		}
	}

	private void AssertAreEqual<T>(T a, T b)
	{
		if (!EqualityComparer<T>.Default.Equals(a, b))
		{
			throw new Exception($"Validation exception: {a} != {b}");
		}
	}
}

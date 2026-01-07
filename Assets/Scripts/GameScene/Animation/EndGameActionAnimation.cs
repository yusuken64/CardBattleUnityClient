using CardBattleEngine;
using System.Collections;
using UnityEngine;

public class EndGameActionAnimation : GameActionAnimation<EndGameAction>
{
	public override IEnumerator Play()
	{
		var ui = Object.FindFirstObjectByType<UI>();
		var gameManager = Object.FindFirstObjectByType<GameManager>();
		var isWin = gameManager.Player.Data == Context.SourcePlayer;
		yield return ui.StartCoroutine(ui.DoGameEndRoutine(isWin));
	}
}

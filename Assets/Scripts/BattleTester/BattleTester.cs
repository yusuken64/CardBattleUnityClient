using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTester : MonoBehaviour
{
	public static int battleCount = 0;

	public List<DeckDefinition> Decks;

	private void Start()
	{
		StartCoroutine(WaitToStart());

		IEnumerator WaitToStart()
		{
			var sceneTransition = Common.Instance.SceneTransition;
			while (sceneTransition.transitionInProgress)
			{
				yield return null;
			}

			yield return null;
			StartFight();
		}
	}

	public void StartFight()
	{
		var firstDeck = Decks[UnityEngine.Random.Range(0, Decks.Count())];
		var secondDeck = Decks[UnityEngine.Random.Range(0, Decks.Count())];
		GameStartParams gameStartParams = new()
		{
			InitialCards = 6,
			SkipMulligan = true,
			SkipShuffle = false,

			CombatDeck = firstDeck.ToDeck(),
			Health = 60,
			AutoPlayer = true,

			CombatDeckEnemy = secondDeck.ToDeck(),
			OpponentHealth = 60,
		};

		GameManager.GameStartParams = gameStartParams;
		GameManager.GameResultRoutine = GameResult;

		IEnumerator GameResult(bool isWin)
		{
			Debug.Log($"battleCount {battleCount++}");
			yield return null;
		}

		GameManager.ReturnScreenName = "BattleTester";

		Common.Instance.SceneTransition.DoTransition(() =>
		{
			SceneManager.LoadScene("GameScene");
		});
	}
}

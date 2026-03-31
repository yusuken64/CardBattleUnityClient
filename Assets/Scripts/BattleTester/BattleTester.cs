using CardBattleEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTester : MonoBehaviour
{
	public bool SimualteInClient;
	public int AutoCount = 1;
	public static int battleCount = 0;

	public DeckDefinition P1Deck;
	public DeckDefinition P2Deck;
	public List<DeckDefinition> Decks;

	public GameRecorder GameRecorder;

	private void Start()
	{
		if (SimualteInClient)
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
		else
		{
			StartCoroutine(RunBattlesRoutine());
		}
	}

	private IEnumerator RunBattlesRoutine()
	{
		int p1Wins = 0;
		int p2Wins = 0;

		for (int i = 0; i < AutoCount; i++)
		{
			DeckDefinition firstDeck = P1Deck != null ? P1Deck : Decks[UnityEngine.Random.Range(0, Decks.Count())];
			DeckDefinition secondDeck = P2Deck != null ? P2Deck : Decks[UnityEngine.Random.Range(0, Decks.Count())];

			IGameAgent firstAI = new AdvancedAI(null, new SystemRNG());
			IGameAgent secondAI = new AdvancedAI(null, new SystemRNG());
			GameState result = RunFight(firstDeck, firstAI, secondDeck, secondAI);

			if (result.Winner == result.Players[0])
			{
				p1Wins++;
			}
			else
			{
				p2Wins++;
			}

			string gameId = Guid.NewGuid().ToString();
			var records = FromHistory(result.History, gameId, result.turn, result.Winner, result.Players[0]);
			foreach (var record in records)
			{
				_ = GameRecorder.SendRecordAsync(record);
			}
			yield return null;
		}

		Debug.Log($"p1[{p1Wins}] vs p2[{p2Wins}]");
	}

	public static List<GameRecordDto> FromHistory(
		List<HistoryEntry> history, 
		string gameId, 
		int totalTurns, 
		CardBattleEngine.Player winner,
		CardBattleEngine.Player firstPlayer)
	{
		var records = history
			.Where(x => x.Action is PlayCardAction) // only include PlayCardAction
			.Select(x => new GameRecordDto
			{
				GameId = gameId,
				CardID = (x.Action as PlayCardAction).Card.Name,
				DidWin = x.Player == winner,
				PlayerOrder = x.Player == firstPlayer ? (byte)1 : (byte)2,
				//WasDrawn = x.Action.WasDrawn(), // you may need an extension or bool from Action
				WasPlayed = true,               // always true for PlayCardAction
				//CopiesPlayed = x.Context.Copies, // track copies from context
				TurnPlayed = x.Turn,
				TotalTurns = totalTurns
			})
			.ToList();

		return records;
	}

	public GameState RunFight(
		DeckDefinition firstDeck,
		IGameAgent firstAI,
		DeckDefinition secondDeck,
		IGameAgent secondAI)
	{
		//Debug.Log($"StartingFight {firstDeck.Title} vs {secondDeck.Title}");

		GameEngine engine = new GameEngine();
		CardBattleEngine.Player p1 = new CardBattleEngine.Player(firstDeck.Title);
		p1.Deck.AddRange(firstDeck.ToDeck().Cards.Select(x => x.CreateCard()).ToList());
		p1.Deck.ForEach(x => x.Owner = p1);
		firstAI.SetPlayer(p1);

		CardBattleEngine.Player p2 = new CardBattleEngine.Player(secondDeck.Title);
		p2.Deck.AddRange(secondDeck.ToDeck().Cards.Select(x => x.CreateCard()).ToList());
		p2.Deck.ForEach(x => x.Owner = p2);
		secondAI.SetPlayer(p2);

		GameState gamestate = new GameState(p1, p2, new SystemRNG(), new List<CardBattleEngine.Card>());

		var p1AI = firstAI;
		var p2AI = secondAI;

		while (!gamestate.IsGameOver())
		{
			IGameAgent ai = gamestate.CurrentPlayer == p1 ? p1AI : p2AI;
			var nextAction = ai.GetNextAction(gamestate);
			engine.Resolve(gamestate, nextAction.Item2, nextAction.Item1);
		}

		Debug.Log($"{firstDeck.Title}[{p1AI.GetType().Name}]({p1.Health}) vs {secondDeck.Title}[{p2AI.GetType().Name}]({p2.Health}) : [{gamestate.Winner?.Name ?? "Draw"}] turn {gamestate.turn}");

		return gamestate;
	}

	public void StartFight()
	{
		DeckDefinition firstDeck = P1Deck != null ? P1Deck : Decks[UnityEngine.Random.Range(0, Decks.Count())];
		DeckDefinition secondDeck = P2Deck != null ? P2Deck : Decks[UnityEngine.Random.Range(0, Decks.Count())];

		GameStartParams gameStartParams = new()
		{
			InitialCards = 6,
			SkipMulligan = true,
			SkipShuffle = false,

			CombatDeck = firstDeck.ToDeck(),
			Health = 60,
			AutoPlayer = true,
			//PlayerAgent = new Adv(null, new SystemRNG(), 50000),

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

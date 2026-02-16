using CardBattleEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TournamentRunner : MonoBehaviour
{
    public List<DeckDefinition> Decks;

	public GameState RunFight()
	{
		DeckDefinition firstDeck = Decks[UnityEngine.Random.Range(0, Decks.Count())];
		DeckDefinition secondDeck = Decks[UnityEngine.Random.Range(0, Decks.Count())];
		GameState resultState = RunFight(firstDeck, secondDeck);
		return resultState;
	}

	public GameState RunFight(DeckDefinition firstDeck, DeckDefinition secondDeck)
	{
		Debug.Log($"StartingFight {firstDeck.Title} vs {secondDeck.Title}");

		GameEngine engine = new GameEngine();
		CardBattleEngine.Player p1 = new CardBattleEngine.Player(firstDeck.Title);
		p1.Deck.AddRange(firstDeck.Cards.Select(x => x.CreateCard()).ToList());
		p1.Deck.ForEach(x => x.Owner = p1);

		CardBattleEngine.Player p2 = new CardBattleEngine.Player(secondDeck.Title);
		p2.Deck.AddRange(secondDeck.Cards.Select(x => x.CreateCard()).ToList());
		p2.Deck.ForEach(x => x.Owner = p2);

		GameState gamestate = new GameState(p1, p2, new SystemRNG(), new List<CardBattleEngine.Card>());

		var p1AI = new AdvancedAI(p1, new SystemRNG());
		var p2AI = new AdvancedAI(p2, new SystemRNG());

		while (!gamestate.IsGameOver())
		{
			var ai = gamestate.CurrentPlayer == p1 ? p1AI : p2AI;
			var nextAction = ai.GetNextAction(gamestate);
			engine.Resolve(gamestate, nextAction.Item2, nextAction.Item1);
		}

		Debug.Log($"{firstDeck.Title}({p1.Health}) vs {secondDeck.Title}({p2.Health}) : [{gamestate.Winner?.Name ?? "Draw"}] turn {gamestate.turn}");

		return gamestate;
	}

	public void RunFightAndReturnWinner(out string deckA, out string deckB, out string winner)
	{
		DeckDefinition firstDeck = Decks[UnityEngine.Random.Range(0, Decks.Count())];
		DeckDefinition secondDeck = Decks[UnityEngine.Random.Range(0, Decks.Count())];
		GameState resultState = RunFight(firstDeck, secondDeck);

		deckA = firstDeck.Title;
		deckB = secondDeck.Title;
		winner = resultState.Winner?.Name;
	}
}

using CardBattleEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public Player Player;
	public Player Opponent;

	public AnimationQueue AnimationQueue;
	public DeckDefinition TestDeck;

	public GameEngine _engine { get; private set; }

	private RandomAI _opponentAgent;
	public GameState _gameState { get; private set; }

	void Start()
	{
		ClearBoard();
		InitializeGame();
	}

	private void ClearBoard()
	{
		Player.Clear();
		Opponent.Clear();
	}

	private void InitializeGame()
	{
		_engine = new GameEngine();

		Deck deck;
		if (Common.Instance.SaveData.GameSaveData.CombatDeck != null)
		{
			deck = Common.Instance.SaveData.GameSaveData.CombatDeck.ToDeck();
			Common.Instance.SaveData.GameSaveData.CombatDeck = null;
		}
		else
		{
			deck = TestDeck.ToDeck();
		}

		Deck enemyDeck;
		if (Common.Instance.SaveData.GameSaveData.CombatDeckEnemy != null)
		{
			enemyDeck = Common.Instance.SaveData.GameSaveData.CombatDeckEnemy.ToDeck();
			Common.Instance.SaveData.GameSaveData.CombatDeckEnemy = null;
		}
		else
		{
			enemyDeck = TestDeck.ToDeck();
		}

		_gameState = CreateTestGame(deck, enemyDeck);
		Player.Data = _gameState.Players[0];
		Opponent.Data = _gameState.Players[1];
		_opponentAgent = new RandomAI(Opponent.Data, new UnityRNG());

		Player.HeroImage.sprite = deck.HeroCard.Sprite;
		Player.HeroPower.OriginalCard = deck.HeroCard.CreateCard();
		Player.HeroPower.Data = Player.Data.HeroPower;
		Player.RefreshData();
		Opponent.RefreshData();

		_engine.ActionPlaybackCallback = ActionPlaybackCallback;
		_engine.ActionResolvedCallback = ActionResolvedCallback;
		_engine.StartGame(_gameState);

	}

	private GameState CreateTestGame(Deck playerDeck, Deck enemyDeck)
	{
		var cardManager = Common.Instance.CardManager;

		CardBattleEngine.Player p1 = new CardBattleEngine.Player("Alice");
		p1.Deck.AddRange(playerDeck.Cards.Select(x => x.CreateCard()).ToList());
		p1.Deck.ForEach(x => x.Owner = p1);
		p1.HeroPower = HeroPowerDefinition.CreateHeroPowerFromHeroCard(playerDeck.HeroCard as MinionCardDefinition);

		CardBattleEngine.Player p2 = new CardBattleEngine.Player("Bob");
		p2.Deck.AddRange(enemyDeck.Cards.Select(x => x.CreateCard()).ToList());
		p2.Deck.ForEach(x => x.Owner = p2);
		p2.HeroPower = HeroPowerDefinition.CreateHeroPowerFromHeroCard(enemyDeck.HeroCard as MinionCardDefinition);

		List<CardBattleEngine.Card> cardDB = Common.Instance.CardManager.AllCards()
			.Select(x => x.CreateCard()).ToList();
		return new GameState(p1, p2, new UnityRNG(), cardDB);
	}

	internal GameObject GetObjectFor(IGameEntity source)
	{
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
		if (source == Player.Data) { return Player.HeroPortrait.gameObject; }
		if (source == Opponent.Data) { return Opponent.HeroPortrait.gameObject; }
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast

		if (source is CardBattleEngine.Minion minion)
		{
			var allMinions = new List<Minion>();
			allMinions.AddRange(Player.Board.Minions);
			allMinions.AddRange(Opponent.Board.Minions);

			var first = allMinions.FirstOrDefault(x => x.Data == minion);
			if (first != null)
			{
				return first.gameObject;
			}
		}

		return null;
	}

	public bool CheckIsValid(IGameAction action, ActionContext context)
	{
		return action.IsValid(_gameState, context);
	}

	public void ResolveAction(IGameAction action, ActionContext context)
	{
		if (action.IsValid(_gameState, context))
		{
			try
			{
				_engine.Resolve(_gameState, context, action);
			}
			catch (System.Exception exception)
			{
				Debug.LogError(exception);
			}
		}
	}

	internal Player GetPlayerFor(CardBattleEngine.Player sourcePlayer)
	{
		return Player.Data.Name == sourcePlayer.Name ? Player : Opponent;
	}

	internal CardBattleEngine.Player GetDataFor(GameState state, Player player)
	{
		return Player.Data.Name == state.Players[0].Name ? state.Players[0] : state.Players[1];
	}

	private void ActionPlaybackCallback(GameState state, (IGameAction action, ActionContext context) current)
	{
		Debug.Log(current);
		AnimationQueue.EnqueueAnimation(this, state, current);

		//UpdateAllEntities();
	}

	private void UpdateAllEntities()
	{
		var activePlayer = GetPlayerFor(_gameState.CurrentPlayer);

		Player.RefreshData();
		Opponent.RefreshData();
	}

	private void ActionResolvedCallback(GameState state)
	{
		if (state.CurrentPlayer == Opponent.Data)
		{
			(IGameAction, ActionContext) nextAction = ((IGameAgent)_opponentAgent).GetNextAction(state);
			ResolveAction(nextAction.Item1, nextAction.Item2);
		}

		Player.UpdatePlayableActions(state.CurrentPlayer == Player.Data);
	}
}

internal class UnityRNG : IRNG
{
	public UnityRNG()
	{
	}

	public IRNG Clone()
	{
		return new UnityRNG();//This doesn't work
	}

	public double NextDouble()
	{
		return UnityEngine.Random.Range(0.0f, float.MaxValue);
	}

	public int NextInt(int maxExclusive)
	{
		return UnityEngine.Random.Range(0, maxExclusive);
	}

	public int NextInt(int minInclusive, int maxExclusive)
	{
		return UnityEngine.Random.Range(minInclusive, maxExclusive);
	}
}
using CardBattleEngine;
using System.Collections;
using System.Linq;
using UnityEngine;

public class PlayCardAnimation : GameActionAnimation<PlayCardAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public PlayCardAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		var player = this.gameManager.GetPlayerFor((current.action as PlayCardAction).Card.Owner);
		player.ManaText.text = $"{player.Data.Mana}/{player.Data.MaxMana}";
		yield return null;
	}
}


public class SummonMinionAnimation : GameActionAnimation<SummonMinionAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public SummonMinionAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		var player = gameManager.GetPlayerFor(current.context.SourcePlayer);
		CardBattleEngine.Minion minionData = current.context.SummonedMinion;

		var existingMinion = player.Board.Minions.FirstOrDefault(minion => minion.SummonedCard == (current.action as SummonMinionAction).Card);
		if (existingMinion == null)
		{
			var index = player.Data.Board.IndexOf(current.context.SummonedMinion);

			//play summon animation and set existingMinion
			var newMinion = Object.Instantiate(gameManager.PlayResolver.MinionPrefab, player.Board.transform);
			newMinion.Setup(minionData);
			player.Board.Minions.Insert(index, newMinion);
			player.Board.UpdateMinionPositions();

			var animator = newMinion.GetComponent<Animator>();
			animator.Play("MinionAppear");

			existingMinion = newMinion;
		}

		existingMinion.Setup(minionData);

		yield return null;
	}
}

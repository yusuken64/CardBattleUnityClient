using CardBattleEngine;
using System.Collections;
using System.Linq;
using UnityEngine;

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
			if (index != -1)
			{
				player.Board.Minions.Insert(index, newMinion);
			} else
			{
				player.Board.Minions.Add(newMinion);
			}
			player.Board.UpdateMinionPositions();

			var animator = newMinion.GetComponent<Animator>();
			animator.Play("MinionAppear");

			existingMinion = newMinion;
		}

		existingMinion.Setup(minionData);

		yield return null;
	}
}

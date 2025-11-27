using CardBattleEngine;
using System.Collections;
using System.Linq;
using UnityEngine;

public class SummonMinionAnimation : GameActionAnimation<SummonMinionAction>
{
	public SummonMinionAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current) : base(gameManager, state, current)
	{
	}

	public override IEnumerator Play()
	{
		var player = GameManager.GetPlayerFor(Context.SourcePlayer);
		CardBattleEngine.Minion minionData = Context.SummonedMinion;

		var existingMinion = player.Board.Minions.FirstOrDefault(minion => minion.SummonedCard == Action.Card);
		if (existingMinion == null)
		{
			var index = Context.PlayIndex;

			//play summon animation and set existingMinion
			var minionPrefab = Object.FindFirstObjectByType<GameInteractionHandler>().MinionPrefab;
			var newMinion = Object.Instantiate(minionPrefab, player.Board.transform);
			newMinion.Setup(minionData);
			player.Board.Minions.Insert(index, newMinion);
			player.Board.UpdateMinionPositions();

			var animator = newMinion.GetComponent<Animator>();
			animator.Play("MinionAppear");

			existingMinion = newMinion;
		}

		existingMinion.SummonedCard = null;
		existingMinion.Setup(minionData);

		yield return null;
	}
}

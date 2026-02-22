using CardBattleEngine;
using System.Collections;
using System.Linq;
using UnityEngine;

public class SummonMinionAnimation : GameActionAnimation<SummonMinionAction>
{
	public AudioClip SummonMinionClip;
	public GameObject SummonParticlePrefab;

	public override IEnumerator Play()
	{
		Common.Instance.AudioManager.PlayClip(SummonMinionClip);

		var player = GameManager.GetPlayerFor(Context.SourcePlayer);
		CardBattleEngine.Minion minionData = Context.SummonedMinion;
		CardBattleEngine.Minion minionDataSnapShot = Context.SummonedMinionSnapShot;

		Debug.Log($"{minionData} at {Context.PlayIndex}");
		var existingMinion = player.Board.Minions
			.FirstOrDefault(minion => minion.SummonedCard == Context.SourceCard &&
			Context.SourceCard != null);
		if (existingMinion == null)
		{
			var index = Context.PlayIndex;

			//play summon animation and set existingMinion
			var minionPrefab = Object.FindFirstObjectByType<GameInteractionHandler>().MinionPrefab;
			var newMinion = Object.Instantiate(minionPrefab, player.Board.transform);
			var clampedIndex = Mathf.Clamp(index, 0, player.Board.Minions.Count());
			player.Board.Minions.Insert(clampedIndex, newMinion);
			player.Board.UpdateMinionPositions();

			var animator = newMinion.GetComponent<Animator>();
			animator.Play("MinionAppear");

			existingMinion = newMinion;
		}
		else
		{
			player.Board.Minions.Remove(existingMinion);
			player.Board.Minions.Insert(Context.PlayIndex, existingMinion);
			player.Board.UpdateMinionPositions();
		}

		if (existingMinion != null)
		{
			var particles = Instantiate(SummonParticlePrefab, existingMinion.transform);
			particles.transform.localPosition = Vector3.zero;
			Destroy(particles.gameObject, 3f);
		}

		existingMinion.SummonedCard = null;
		existingMinion.Setup(minionData);
		//existingMinion.RefreshData(minionDataSnapShot);

		yield return null;
	}
}

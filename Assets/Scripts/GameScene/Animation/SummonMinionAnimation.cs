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
		Common.Instance.AudioManager.PlaySound(SummonMinionClip);

		var player = GameManager.GetPlayerFor(Presentation.SourcePlayer);
		CardBattleEngine.Minion minionData = Presentation.SummonedMinion;
        if (minionData == null) yield break;


		Debug.Log($"{minionData} at {Presentation.PlayIndex}");
		var existingMinion = player.Board.Minions
			.FirstOrDefault(minion => minion.SummonedCard?.Id == Presentation.SourceCard?.Id &&
			Presentation.SourceCard != null);
		if (existingMinion == null)
		{
			var index = Presentation.PlayIndex;

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
			player.Board.Minions.Insert(Presentation.PlayIndex, existingMinion);
			player.Board.UpdateMinionPositions();
		}

		if (existingMinion != null)
		{
			var particles = Presentation.Own(Instantiate(SummonParticlePrefab, existingMinion.transform));
			particles.transform.localPosition = Vector3.zero;
			Destroy(particles.gameObject, 3f);
		}

		existingMinion.SummonedCard = null;
		existingMinion.Setup(minionData);
		//existingMinion.RefreshData(minionDataSnapShot);

		var appearance = existingMinion.GetComponent<Animator>();
        if (appearance != null)
        {
            yield return null;
            while (appearance != null && appearance.GetCurrentAnimatorStateInfo(0).IsName("MinionAppear") &&
                   appearance.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
        }
	}
}

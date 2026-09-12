using CardBattleEngine;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class DeathAnimation : GameActionAnimation<DeathAction>
{
	public AudioClip MinionDie;
	public GameObject MinionDieParticles;

	public override IEnumerator Play()
	{
		if (Presentation.Target == null) yield break;
		var owner = GameManager.GetPlayerFor(Presentation.Target.Owner);

		if (Presentation.Target is CardBattleEngine.Minion minion)
		{
			var deadMinion = GameManager.GetObjectFor(Presentation.Target)?.GetComponent<Minion>();
			if (deadMinion == null) yield break;

			var particles = Presentation.Own(Instantiate(MinionDieParticles, deadMinion.transform.position, Quaternion.identity));
			Destroy(particles, 3f);

			if (owner.Board.Minions.Contains(deadMinion))
			{
				owner.Board.Minions.Remove(deadMinion);
			}
			else
			{
				throw new Exception("Invalid dead minion");
			}

			Transform t = deadMinion.transform;

			// Animate: scale down + move down + fade out
			var seq = DOTween.Sequence().SetId(Presentation);

			// Try to fetch optional CanvasGroup or SpriteRenderer for fading
			CanvasGroup cg = deadMinion.GetComponent<CanvasGroup>();
			SpriteRenderer sr = deadMinion.GetComponentInChildren<SpriteRenderer>();

			seq.Append(t.DOScale(0f, 0.25f).SetId(Presentation).SetEase(Ease.InBack))
			   .Join(t.DOMoveY(t.position.y - 0.3f, 0.25f).SetId(Presentation));

			if (cg != null)
				seq.Join(cg.DOFade(0f, 0.25f).SetId(Presentation));
			else if (sr != null)
				seq.Join(sr.DOFade(0f, 0.25f).SetId(Presentation));

			seq.OnComplete(() =>
			{
				deadMinion.gameObject.gameObject.SetActive(false);
				GameObject.Destroy(deadMinion.gameObject);
				owner.Board.UpdateMinionPositions();
			});

			yield return seq.WaitForCompletion();
		}
		else if (Presentation.Target is CardBattleEngine.Player player)
		{
			yield return owner.DoDeathRoutine(Presentation);
		}
	}
}

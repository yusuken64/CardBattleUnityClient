using CardBattleEngine;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class DeathAnimation : GameActionAnimation<DeathAction>
{
	public AudioClip MinionDie;
	public override IEnumerator Play()
	{
		var owner = GameManager.GetPlayerFor(Context.Target.Owner);

		if (Context.Target is CardBattleEngine.Minion minion)
		{
			var deadMinion = GameManager.GetObjectFor(Context.Target)
				.GetComponent<Minion>();

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
			var seq = DOTween.Sequence();

			// Try to fetch optional CanvasGroup or SpriteRenderer for fading
			CanvasGroup cg = deadMinion.GetComponent<CanvasGroup>();
			SpriteRenderer sr = deadMinion.GetComponentInChildren<SpriteRenderer>();

			seq.Append(t.DOScale(0f, 0.25f).SetEase(Ease.InBack))
			   .Join(t.DOMoveY(t.position.y - 0.3f, 0.25f));

			if (cg != null)
				seq.Join(cg.DOFade(0f, 0.25f));
			else if (sr != null)
				seq.Join(sr.DOFade(0f, 0.25f));

			seq.OnComplete(() =>
			{
				deadMinion.gameObject.gameObject.SetActive(false);
				GameObject.Destroy(deadMinion.gameObject);
				owner.Board.UpdateMinionPositions();
			});

			yield return seq.WaitForCompletion();
		}
		else if (Context.Target is CardBattleEngine.Player player)
		{
			yield return owner.DoDeathRoutine();
		}
	}
}

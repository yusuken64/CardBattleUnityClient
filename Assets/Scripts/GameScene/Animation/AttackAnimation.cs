using CardBattleEngine;
using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class AttackAnimation : GameActionAnimation<AttackAction>
{
	public float Duration = 0.15f;
	public AnimationCurve AttackCurve;

	public GameObject AttackParticlePrefab;

	public AttackTier[] AttackTiers;

	public override IEnumerator Play()
	{
		var sourceObject = GameManager.GetObjectFor(Presentation.Source);
        var targetObject = GameManager.GetObjectFor(Presentation.Target);
        if (sourceObject == null || targetObject == null) yield break;
        Transform attacker = sourceObject.transform;
		Transform target = targetObject.transform;
		var movingMinion = sourceObject.GetComponent<Minion>();
		if (movingMinion != null) movingMinion.Moving = false;

		Vector3 startPos = attacker.position;
		Vector3 dir = (target.position - attacker.position).normalized;
		Vector3 bumpPos = target.position - (dir * 0.4f) + new Vector3(0, 0, -0.1f); // distance of bump

		// forward bump
		Tween forward = attacker.DOMove(bumpPos, Duration).SetId(Presentation).SetEase(AttackCurve)
			.OnComplete(() =>
			{
				Vector3 dir = (target.position - attacker.position).normalized;
				Quaternion rotation = Quaternion.LookRotation(dir);

				var attackParticle = Presentation.Own(Instantiate(AttackParticlePrefab, attacker.position, rotation));
				Destroy(attackParticle, 3f);

			});

		// wait
		yield return forward.WaitForCompletion();

		// backward bump
		Tween back = attacker.DOMove(startPos, 0.15f).SetId(Presentation).SetEase(Ease.Linear);

		yield return back.WaitForCompletion();

	}
}

[Serializable]
public class AttackTier
{
	public int MinAttack;   // inclusive
	public int MaxAttack = -1;   // -1 means no upper limit (8+ etc)

	public float ShakeStrength = 0.05f;
	public float ShakeDuration = 0.08f;

	public AudioClip AttackSound;

	public bool Matches(int attack)
	{
		return attack >= MinAttack &&
			   (MaxAttack < 0 || attack <= MaxAttack);
	}
}

using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TakeDamageAnimation : GameActionAnimation<DamageAction>
{
	public AudioClip DamageSound;
	public GameObject DamageParticles;

	public AttackTier[] AttackTiers;
	public override IEnumerator Play()
	{
		List<(IGameEntity, int)> targets;
		if (Context.AffectedEntities != null &
			Context.AffectedEntities.Count() > 0)
		{
			targets = Context.AffectedEntities.ToList();
		}
		else
		{
			targets = new() { (Context.Target, Context.DamageDealt) };
		}

		foreach (var target in targets)
		{
			GameObject gameObject = GameManager.GetObjectFor(target.Item1);

			if (gameObject is null)
			{
				throw new System.Exception("No damage object");
			}

			Transform targetTransform = gameObject.transform;

			var particles = Instantiate(DamageParticles, targetTransform.transform.position, Quaternion.identity);

			// simple shake: short, small, no fancy stuff
			Tween shake = targetTransform.DOShakePosition(
				duration: 0.2f,
				strength: 0.15f,   // how far it shakes
				vibrato: 20,       // how fast it vibrates
				randomness: 90f,   // random direction
				fadeOut: true
			);

			Common.Instance.AudioManager.PlaySound(DamageSound);
			Object.FindFirstObjectByType<UI>().ShowDamage(target.Item2, targetTransform);
			if (Context.IsAttack)
			{
				PlayAttackEffects(target.Item2);
			}

			yield return shake.WaitForCompletion();
		}
	}

	void PlayAttackEffects(int attack)
	{
		// find the tier that matches this attack
		AttackTier tier = AttackTiers
			.OrderByDescending(x => x.MinAttack)
			.FirstOrDefault(x => x.Matches(attack));

		if (tier == null) return;

		//Debug.Log($"Tier {tier.MinAttack}~{tier.MaxAttack}");

		// Shake camera safely
		DoCameraShake(tier.ShakeStrength, tier.ShakeDuration);

		// Play sound
		if (tier.AttackSound != null)
		{
			Common.Instance.AudioManager.PlaySound(tier.AttackSound);
		}
	}

	Tween cameraShakeTween;
	Vector3 cameraOriginalLocalPos;

	void DoCameraShake(float strength, float duration)
	{
		Transform cam = Camera.main.transform;

		// Cache original position once
		if (cameraShakeTween == null)
		{
			cameraOriginalLocalPos = cam.localPosition;
		}

		// Kill any ongoing shake and reset immediately
		if (cameraShakeTween != null && cameraShakeTween.IsActive())
		{
			cameraShakeTween.Kill();
			cam.localPosition = cameraOriginalLocalPos;
		}

		cameraShakeTween = cam.DOShakePosition(
			duration,
			strength,
			vibrato: 20,
			randomness: 90,
			fadeOut: true
		)
		.OnComplete(() =>
		{
			cam.localPosition = cameraOriginalLocalPos;
			cameraShakeTween = null;
		})
		.OnKill(() =>
		{
			cam.localPosition = cameraOriginalLocalPos;
			cameraShakeTween = null;
		});
	}

	[ContextMenu("TestAttack1")]
	public void TestAttack1()
	{
		PlayAttackEffects(1);
	}
	[ContextMenu("TestAttack2")]
	public void TestAttack2()
	{
		PlayAttackEffects(2);
	}
	[ContextMenu("TestAttack4")]
	public void TestAttack4()
	{
		PlayAttackEffects(4);
	}
	[ContextMenu("TestAttack6")]
	public void TestAttack6()
	{
		PlayAttackEffects(6);
	}
	[ContextMenu("TestAttack8")]
	public void TestAttack8()
	{
		PlayAttackEffects(8);
	}
}

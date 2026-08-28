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
		Transform attacker = GameManager.GetObjectFor(Context.Source).transform;
		Transform target = GameManager.GetObjectFor(Context.Target).transform;

		Vector3 startPos = attacker.position + new Vector3(0, 0, -0.5f);
		attacker.transform.position = startPos;
		Vector3 dir = (target.position - attacker.position).normalized;
		Vector3 bumpPos = target.position - (dir * 0.4f) + new Vector3(0, 0, -0.1f); // distance of bump

		// forward bump
		Tween forward = attacker.DOMove(bumpPos, Duration).SetEase(AttackCurve)
			.OnComplete(() =>
			{
				Vector3 dir = (target.position - attacker.position).normalized;
				Quaternion rotation = Quaternion.LookRotation(dir);

				var attackParticle = Instantiate(AttackParticlePrefab, attacker.position, rotation);
				Destroy(attackParticle, 3f);

				int attack = Context.Source.Attack;
			});

		// wait
		yield return forward.WaitForCompletion();

		if (Context.Source is CardBattleEngine.Player player &&
			player.EquippedWeapon != null)
		{
			var gamePlayer = GameManager.GetObjectFor(Context.Source).GetComponentInParent<Player>();
			gamePlayer.Weapon.Durability--;
			gamePlayer.Weapon.UpdateUI();
		}

		// backward bump
		Tween back = attacker.DOMove(startPos, 0.15f).SetEase(Ease.Linear);

		yield return back.WaitForCompletion();

		var heroPortrait = attacker.gameObject.GetComponent<HeroPortrait>();
		var minion = attacker.gameObject.GetComponent<Minion>();
		if (heroPortrait != null)
		{
			heroPortrait.Player.CanAttack = (Context.Source as CardBattleEngine.Player).CanAttack();
			heroPortrait.Player.UpdateUI();
		}
		else if (minion != null)
		{
			minion.CanAttack = (Context.Source as CardBattleEngine.Minion).CanAttack();
			minion.UpdateUI();
		}
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
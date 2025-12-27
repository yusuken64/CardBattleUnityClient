using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class StatusChangeAnimation : GameActionAnimation<StatusChangeAction>
{
	public override IEnumerator Play()
	{
		Transform target = GameManager.GetObjectFor(Context.Target).transform;

		if (Action.LostStealth)
		{
			target.Stealth = false;
			target.UpdateUI();
		}
		//.. handle other status like freeze
	}
}

	public class AttackAnimation : GameActionAnimation<AttackAction>
{
	public float Duration = 0.15f;
	public AnimationCurve AttackCurve;

	public override IEnumerator Play()
	{
		Transform attacker = GameManager.GetObjectFor(Context.Source).transform;
		Transform target = GameManager.GetObjectFor(Context.Target).transform;

		Vector3 startPos = attacker.position;
		Vector3 dir = (target.position - attacker.position).normalized;
		Vector3 bumpPos = target.position - dir * 0.4f; // distance of bump

		// forward bump
		Tween forward = attacker.DOMove(bumpPos, Duration).SetEase(AttackCurve);

		// wait
		yield return forward.WaitForCompletion();

		if (Context.Source is CardBattleEngine.Player player &&
			player.EquippedWeapon != null)
		{
			var gamePlayer = GameManager.GetObjectFor(Context.Source).GetComponentInParent<Player>();
			gamePlayer.Weapon.Durablity--;
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

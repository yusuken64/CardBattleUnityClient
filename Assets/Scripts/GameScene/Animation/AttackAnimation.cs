using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class AttackAnimation : GameActionAnimation<AttackAction>
{
	public AttackAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current) : base(gameManager, state, current)
	{
	}

	public override IEnumerator Play()
	{
		Transform attacker = GameManager.GetObjectFor(Context.Source).transform;
		Transform target = GameManager.GetObjectFor(Context.Target).transform;

		Vector3 startPos = attacker.position;
		Vector3 dir = (target.position - attacker.position).normalized;
		Vector3 bumpPos = startPos + dir * 0.4f; // distance of bump

		// forward bump
		Tween forward = attacker.DOMove(bumpPos, 0.15f).SetEase(Ease.Linear);

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

		var playerObject = attacker.gameObject.GetComponentInParent<Player>();
		var minion = attacker.gameObject.GetComponent<Minion>();
		if (playerObject != null)
		{
			playerObject.RefreshData();
		}
		else if (minion != null)
		{
			minion.RefreshData();
		}

		yield return back.WaitForCompletion();
	}
}

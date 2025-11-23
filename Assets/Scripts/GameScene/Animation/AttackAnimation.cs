using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class AttackAnimation : GameActionAnimation<AttackAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public AttackAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		Transform attacker = gameManager.GetObjectFor(current.context.Source).transform;
		Transform target = gameManager.GetObjectFor(current.context.Target).transform;

		Vector3 startPos = attacker.position;
		Vector3 dir = (target.position - attacker.position).normalized;
		Vector3 bumpPos = startPos + dir * 0.4f; // distance of bump

		// forward bump
		Tween forward = attacker.DOMove(bumpPos, 0.15f).SetEase(Ease.Linear);

		// wait
		yield return forward.WaitForCompletion();

		if (current.context.Source is CardBattleEngine.Player player &&
			player.EquippedWeapon != null)
		{
			var gamePlayer = gameManager.GetObjectFor(current.context.Source).GetComponentInParent<Player>();
			gamePlayer.Weapon.Durablity--;
			gamePlayer.Weapon.UpdateUI();
		}

		// backward bump
		Tween back = attacker.DOMove(startPos, 0.15f).SetEase(Ease.Linear);

		var playerObject = attacker.gameObject.GetComponentInParent<Player>();
		var minion = attacker.gameObject.GetComponent<Minion>();
		if (playerObject != null)
		{
			playerObject.RefreshData(playerObject.Data == state.CurrentPlayer);
		}
		else if (minion != null)
		{
			minion.RefreshData(minion.Data.Owner == state.CurrentPlayer);
		}

		yield return back.WaitForCompletion();
	}
}

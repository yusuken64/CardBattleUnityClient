using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class TakeDamageAnimation : GameActionAnimation<DamageAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public TakeDamageAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		GameObject gameObject = gameManager.GetObjectFor(current.context.Target);
		Transform target = gameObject.transform;

		// simple shake: short, small, no fancy stuff
		Tween shake = target.DOShakePosition(
			duration: 0.2f,
			strength: 0.15f,   // how far it shakes
			vibrato: 20,       // how fast it vibrates
			randomness: 90f,   // random direction
			fadeOut: true
		);

		Object.FindFirstObjectByType<UI>().ShowDamage((current.action as DamageAction).Damage, target);

		var player = gameObject.GetComponentInParent<Player>();
		var minion = gameObject.GetComponent<Minion>();
		if (player != null)
		{
			player.RefreshData(player.Data == state.CurrentPlayer);
		}
		else if (minion != null)
		{
			minion.RefreshData(minion.Data.Owner == state.CurrentPlayer);
		}

		yield return shake.WaitForCompletion();
	}
}

public class TakeHealAnimation : GameActionAnimation<HealAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public TakeHealAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		GameObject gameObject = gameManager.GetObjectFor(current.context.Target);
		Transform target = gameObject.transform;

		Object.FindFirstObjectByType<UI>().ShowHeal((current.action as HealAction).Amount, target);

		var player = gameObject.GetComponentInParent<Player>();
		var minion = gameObject.GetComponent<Minion>();
		if (player != null)
		{
			player.RefreshData(player.Data == state.CurrentPlayer);
		}
		else if (minion != null)
		{
			minion.RefreshData(minion.Data.Owner == state.CurrentPlayer);
		}

		yield return null;
	}
}

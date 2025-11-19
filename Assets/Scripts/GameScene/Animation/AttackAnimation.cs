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

		// backward bump
		Tween back = attacker.DOMove(startPos, 0.15f).SetEase(Ease.Linear);

		yield return back.WaitForCompletion();
	}
}

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
		Transform target = gameManager.GetObjectFor(current.context.Target).transform;

		// simple shake: short, small, no fancy stuff
		Tween shake = target.DOShakePosition(
			duration: 0.2f,
			strength: 0.15f,   // how far it shakes
			vibrato: 20,       // how fast it vibrates
			randomness: 90f,   // random direction
			fadeOut: true
		);

		Object.FindFirstObjectByType<UI>().ShowDamage((current.action as DamageAction).Damage, target);

		yield return shake.WaitForCompletion();
	}
}

public class DeathAnimation : GameActionAnimation<DeathAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public DeathAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		var owner = gameManager.GetPlayerFor(current.context.Target.Owner);
		var deadMinion = gameManager.GetObjectFor(current.context.Target)
			.GetComponent<Minion>();

		if (owner.Board.Minions.Contains(deadMinion))
		{
			owner.Board.Minions.Remove(deadMinion);
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
			GameObject.Destroy(deadMinion.gameObject);
			owner.Board.UpdateMinionPositions();
		});

		yield return seq.WaitForCompletion();
	}
}
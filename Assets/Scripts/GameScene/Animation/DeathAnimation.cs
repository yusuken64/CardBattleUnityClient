using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

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

		if (current.context.Target is CardBattleEngine.Minion minion)
		{
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
				deadMinion.gameObject.gameObject.SetActive(false);
				//GameObject.Destroy(deadMinion.gameObject);
				owner.Board.UpdateMinionPositions();
			});

			yield return seq.WaitForCompletion();
		}
		else if (current.context.Target is CardBattleEngine.Player player)
		{
			yield return owner.DoDeathRoutine();
		}
	}
}

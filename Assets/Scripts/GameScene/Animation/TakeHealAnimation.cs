using CardBattleEngine;
using System.Collections;
using UnityEngine;

public class TakeHealAnimation : GameActionAnimation<HealAction>
{
	public TakeHealAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current) : base(gameManager, state, current)
	{
	}

	public override IEnumerator Play()
	{
		GameObject gameObject = GameManager.GetObjectFor(Context.Target);
		Transform target = gameObject.transform;

		//TODO get healedamount from context;
		Object.FindFirstObjectByType<UI>().ShowHeal(Context.HealedAmount, target);

		var player = gameObject.GetComponentInParent<Player>();
		var minion = gameObject.GetComponent<Minion>();
		if (player != null)
		{
			player.RefreshData();
		}
		else if (minion != null)
		{
			minion.RefreshData();
		}

		yield return null;
	}
}

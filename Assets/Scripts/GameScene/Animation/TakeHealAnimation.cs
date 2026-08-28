using CardBattleEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TakeHealAnimation : GameActionAnimation<HealAction>
{
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
			if (gameObject == null) { yield break; }
			Transform targetTransforms = gameObject.transform;

			Object.FindFirstObjectByType<UI>().ShowHeal(target.Item2, targetTransforms);

			//var portrait = gameObject.GetComponent<HeroPortrait>();
			//var minion = gameObject.GetComponent<Minion>();
			//if (portrait != null)
			//{
			//	portrait.Player.Health += Context.HealedAmount;
			//	portrait.Player.UpdateUI();
			//}
			//else if (minion != null)
			//{
			//	minion.Health += Context.HealedAmount;
			//	minion.UpdateUI();
			//}
		}
		yield return null;
	}
}

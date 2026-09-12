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
		if (Presentation.AffectedEntities != null &
			Presentation.AffectedEntities.Count() > 0)
		{
			targets = Presentation.AffectedEntities.ToList();
		}
		else
		{
			targets = new() { (Presentation.Target, Presentation.HealedAmount) };
		}

		foreach (var target in targets)
		{
			GameObject gameObject = GameManager.GetObjectFor(target.Item1);
			if (gameObject == null) { yield break; }
			Transform targetTransforms = gameObject.transform;

			Object.FindFirstObjectByType<UI>().ShowHeal(target.Item2, targetTransforms, Presentation);

			//var portrait = gameObject.GetComponent<HeroPortrait>();
			//var minion = gameObject.GetComponent<Minion>();
			//if (portrait != null)
			//{
			//	portrait.Player.Health += Presentation.HealedAmount;
			//	portrait.Player.UpdateUI();
			//}
			//else if (minion != null)
			//{
			//	minion.Health += Presentation.HealedAmount;
			//	minion.UpdateUI();
			//}
		}
		yield return new WaitForSecondsRealtime(0.75f);
	}
}

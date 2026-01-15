using CardBattleEngine;
using System.Collections;
using UnityEngine;

public class TakeHealAnimation : GameActionAnimation<HealAction>
{
	public override IEnumerator Play()
	{
		GameObject gameObject = GameManager.GetObjectFor(Context.Target);
		if (gameObject == null) { yield break; }
		Transform target = gameObject.transform;

		Object.FindFirstObjectByType<UI>().ShowHeal(Context.HealedAmount, target);

		var portrait = gameObject.GetComponent<HeroPortrait>();
		var minion = gameObject.GetComponent<Minion>();
		if (portrait != null)
		{
			portrait.Player.Health += Context.HealedAmount;
			portrait.Player.UpdateUI();
		}
		else if (minion != null)
		{
			minion.Health += Context.HealedAmount;
			minion.UpdateUI();
		}

		yield return null;
	}
}

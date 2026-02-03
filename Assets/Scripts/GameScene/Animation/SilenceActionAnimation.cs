using CardBattleEngine;
using System.Collections;

public class SilenceActionAnimation : GameActionAnimation<SilenceAction>
{
	public override IEnumerator Play()
	{
		var minionData = GameManager.GetObjectFor(Context.Target);
		var minion = minionData?.GetComponent<Minion>();

		if (minion != null)
		{
			minion.IsSilenced = true;
		}

		yield return null;
	}
}

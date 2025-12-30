using CardBattleEngine;
using System.Collections;

public class FreezeActionAnimation : GameActionAnimation<FreezeAction>
{
	public override IEnumerator Play()
	{
		yield return null;
	}
}

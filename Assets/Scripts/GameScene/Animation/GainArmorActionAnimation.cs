using CardBattleEngine;
using System.Collections;

public class GainArmorActionAnimation : GameActionAnimation<GainArmorAction>
{
	public override IEnumerator Play()
	{
		var player = GameManager.GetPlayerFor(Context.Target as CardBattleEngine.Player);
		player.RefreshData();

		yield return null;
	}
}
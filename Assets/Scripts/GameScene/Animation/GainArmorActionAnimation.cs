using CardBattleEngine;
using System.Collections;

public class GainArmorActionAnimation : GameActionAnimation<GainArmorAction>
{
	public override IEnumerator Play()
	{
		var player = GameManager.GetPlayerFor(Context.Target as CardBattleEngine.Player);
		if (player == null) { yield break; }
		player.Armor += Context.ArmorGained;
		player.UpdateUI();

		yield return null;
	}
}
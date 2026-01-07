using CardBattleEngine;
using System.Collections;

public class IncreaseMaxManaAnimation : GameActionAnimation<IncreaseMaxManaAction>
{
	public override IEnumerator Play()
	{
		var player = this.GameManager.GetPlayerFor(Context.SourcePlayer);
		player.MaxMana = Context.SourcePlayer.MaxMana;
		player.UpdateUI();
		yield return null;
	}
}

using CardBattleEngine;
using System.Collections;

public class RefillManaAnimation : GameActionAnimation<RefillManaAction>
{
	public override IEnumerator Play()
	{
		var player = this.GameManager.GetPlayerFor(Context.SourcePlayer);
		player.Mana = Context.SourcePlayer.MaxMana;
		player.UpdateUI();
		yield return null;
	}
}

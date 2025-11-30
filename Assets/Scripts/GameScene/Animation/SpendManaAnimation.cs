using CardBattleEngine;
using System.Collections;

public class SpendManaAnimation : GameActionAnimation<SpendManaAction>
{
	public override IEnumerator Play()
	{
		var spendManaAction = Action;
		var player = GameManager.GetPlayerFor(Context.SourcePlayer);
		player.Mana -= spendManaAction.Amount;
		player.UpdateUI();

		yield return null;
	}
}

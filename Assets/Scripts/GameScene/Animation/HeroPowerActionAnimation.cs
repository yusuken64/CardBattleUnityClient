using CardBattleEngine;
using System.Collections;
using UnityEngine;

public class HeroPowerActionAnimation : GameActionAnimation<HeroPowerAction>
{
	public override IEnumerator Play()
	{
		var ui = Object.FindFirstObjectByType<UI>();
		var triggerParticle = Object.Instantiate(ui.TriggeredEffectParticlePrefab); ;
		//entity could be card, hero or minion, or secret

		var owningPlayer = GameManager.GetPlayerFor(Context.SourcePlayer);
		triggerParticle.transform.position = owningPlayer.HeroPower.transform.position;
		owningPlayer.HeroPower.RefreshData();

		Object.Destroy(triggerParticle.gameObject, 1f);

		yield return new WaitForSecondsRealtime(0.3f);
	}
}
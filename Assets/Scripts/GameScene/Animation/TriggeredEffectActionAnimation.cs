using CardBattleEngine;
using System.Collections;
using UnityEngine;

public class TriggerEffectActionAnimation : GameActionAnimation<TriggerEffectAction>
{
	public TriggerEffectActionAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current) : base(gameManager, state, current)
	{
	}

	public override IEnumerator Play()
	{
		var ui = Object.FindFirstObjectByType<UI>();
		var triggerParticle = Object.Instantiate(ui.TriggeredEffectParticlePrefab);
		var entity = Action.TriggerSource.Entity;
		//entity could be card, hero or minion, or secret
		var gameObject = GameManager.GetObjectFor(entity);

		if (gameObject != null)
		{
			triggerParticle.transform.position = gameObject.transform.position;
		}
		else
		{
			var owningPlayer = GameManager.GetPlayerFor(entity.Owner);
			triggerParticle.transform.position = owningPlayer.transform.position;
		}

		Object.Destroy(triggerParticle.gameObject, 1f);

		yield return new WaitForSecondsRealtime(0.3f);
	}
}

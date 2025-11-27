using CardBattleEngine;
using System.Collections;
using UnityEngine;

public class TriggerEffectActionAnimation : GameActionAnimation<TriggerEffectAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public TriggerEffectActionAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		if (current.action is TriggerEffectAction triggerEffectAction)
		{
			var ui = Object.FindFirstObjectByType<UI>();
			var triggerParticle = Object.Instantiate(ui.TriggeredEffectParticlePrefab);
			var entity = triggerEffectAction.TriggerSource.Entity;
			//entity could be card, hero or minion, or secret
			var gameObject = gameManager.GetObjectFor(entity);

			if (gameObject != null)
			{
				triggerParticle.transform.position = gameObject.transform.position;
			}
			else
			{
				var owningPlayer = gameManager.GetPlayerFor(entity.Owner);
				triggerParticle.transform.position = owningPlayer.transform.position;
			}

			Object.Destroy(triggerParticle.gameObject, 1f);
		}

		yield return new WaitForSecondsRealtime(0.3f);
	}
}

public class HeroPowerActionAnimation : GameActionAnimation<HeroPowerAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public HeroPowerActionAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		if (current.action is HeroPowerAction heroPowerAction)
		{
			var ui = Object.FindFirstObjectByType<UI>();
			var triggerParticle = Object.Instantiate(ui.TriggeredEffectParticlePrefab);;
			//entity could be card, hero or minion, or secret
			
			var owningPlayer = gameManager.GetPlayerFor(current.context.SourcePlayer);
			triggerParticle.transform.position = owningPlayer.HeroPower.transform.position;
			owningPlayer.HeroPower.RefreshData();

			Object.Destroy(triggerParticle.gameObject, 1f);
		}

		yield return new WaitForSecondsRealtime(0.3f);
	}
}
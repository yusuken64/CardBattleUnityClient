using CardBattleEngine;
using System.Collections;
using UnityEngine;

public class TriggerEffectActionAnimation : GameActionAnimation<TriggerEffectAction>
{
	public AudioClip TriggerSound;

	public override IEnumerator Play()
	{
		Common.Instance.AudioManager.PlaySound(TriggerSound);
		var ui = Object.FindFirstObjectByType<UI>();
		var triggerParticle = Presentation.Own(Object.Instantiate(ui.TriggeredEffectParticlePrefab));
		var entity = Presentation.TriggerSource;
		//entity could be card, hero or minion, or secret
		var gameObject = GameManager.GetObjectFor(entity);

		if (gameObject != null)
		{
			triggerParticle.transform.position = gameObject.transform.position;
		}
		else
		{
			var owningPlayer = GameManager.GetPlayerFor(entity?.Owner ?? Presentation.SourcePlayer);
			triggerParticle.transform.position = owningPlayer.transform.position;
		}

		Object.Destroy(triggerParticle.gameObject, 1f);

		yield return new WaitForSecondsRealtime(0.3f);
	}
}

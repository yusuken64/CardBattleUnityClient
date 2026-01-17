using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TakeDamageAnimation : GameActionAnimation<DamageAction>
{
	public AudioClip DamageSound;
	public GameObject DamageParticles;
	public override IEnumerator Play()
	{
		List<(IGameEntity, int)> targets;
		if (Context.AffectedEntities != null &
			Context.AffectedEntities.Count() > 0)
		{
			targets = Context.AffectedEntities.ToList();
		}
		else
		{
			targets = new() { (Context.Target, Context.DamageDealt) };
		}

		foreach (var target in targets)
		{
			GameObject gameObject = GameManager.GetObjectFor(target.Item1);

			if (gameObject is null)
			{
				throw new System.Exception("No damage object");
			}

			Transform targetTransform = gameObject.transform;

			var particles = Instantiate(DamageParticles, targetTransform.transform.position, Quaternion.identity);

			// simple shake: short, small, no fancy stuff
			Tween shake = targetTransform.DOShakePosition(
				duration: 0.2f,
				strength: 0.15f,   // how far it shakes
				vibrato: 20,       // how fast it vibrates
				randomness: 90f,   // random direction
				fadeOut: true
			);

			Common.Instance.AudioManager.PlaySound(DamageSound);
			Object.FindFirstObjectByType<UI>().ShowDamage(target.Item2, targetTransform);

			yield return shake.WaitForCompletion();

			//var portrait = gameObject.GetComponent<HeroPortrait>();
			//var minion = gameObject.GetComponent<Minion>();
			//if (portrait != null)
			//{
			//	portrait.Player.Health -= Context.DamageDealt;
			//	portrait.Player.Armor -= Context.ArmorDamageDealt;
			//	portrait.Player.UpdateUI();
			//}
			//else if (minion != null)
			//{
			//	minion.Health -= Context.DamageDealt;
			//	minion.HasDivineShield = (Context.Target as CardBattleEngine.Minion).HasDivineShield;
			//	minion.UpdateUI();
			//}
		}
	}
}

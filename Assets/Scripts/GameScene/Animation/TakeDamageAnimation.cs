using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class TakeDamageAnimation : GameActionAnimation<DamageAction>
{
	public AudioClip DamageSound;
	public override IEnumerator Play()
	{
		Common.Instance.AudioManager.PlaySound(DamageSound);
		GameObject gameObject = GameManager.GetObjectFor(Context.Target);

		if (gameObject is null)
		{
			throw new System.Exception("No damage object");
		}

		Transform target = gameObject.transform;

		// simple shake: short, small, no fancy stuff
		Tween shake = target.DOShakePosition(
			duration: 0.2f,
			strength: 0.15f,   // how far it shakes
			vibrato: 20,       // how fast it vibrates
			randomness: 90f,   // random direction
			fadeOut: true
		);

		Object.FindFirstObjectByType<UI>().ShowDamage((Context.DamageDealt), target);

		yield return shake.WaitForCompletion();

		var portrait = gameObject.GetComponent<HeroPortrait>();
		var minion = gameObject.GetComponent<Minion>();
		if (portrait != null)
		{
			portrait.Player.Health -= Context.DamageDealt;
			portrait.Player.Armor -= Context.ArmorDamageDealt;
			portrait.Player.UpdateUI();
		}
		else if (minion != null)
		{
			minion.Health -= Context.DamageDealt;
			minion.HasDivineShield = (Context.Target as CardBattleEngine.Minion).HasDivineShield;
			minion.UpdateUI();
		}
	}
}

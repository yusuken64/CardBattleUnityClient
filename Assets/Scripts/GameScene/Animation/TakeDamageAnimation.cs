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

		var player = gameObject.GetComponentInParent<Player>();
		var minion = gameObject.GetComponent<Minion>();
		if (player != null)
		{
			player.RefreshData();
		}
		else if (minion != null)
		{
			minion.RefreshData();
		}

		yield return shake.WaitForCompletion();
	}
}

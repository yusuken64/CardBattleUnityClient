using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class EquipWeaponAnimation : GameActionAnimation<EquipWeaponAction>
{
	public AudioClip EquipWeaponClip;
	public override IEnumerator Play()
	{
		Common.Instance.AudioManager.PlaySound(EquipWeaponClip);


		var player = GameManager.GetPlayerFor(Presentation.Target as CardBattleEngine.Player);
		player.Weapon.gameObject.SetActive(true);
		player.Weapon.transform.localScale = Vector3.zero;
		player.Weapon.Setup((Presentation.Target as CardBattleEngine.Player)?.EquippedWeapon);

		player.Weapon.transform.DOScale(Vector3.one, 0.3f).SetId(Presentation);
		player.Weapon.transform.DOPunchScale(Vector3.one * 1.1f, 0.2f).SetId(Presentation);

		//player.Weapon.RefreshData();


		yield return new WaitForSecondsRealtime(0.3f);
	}
}

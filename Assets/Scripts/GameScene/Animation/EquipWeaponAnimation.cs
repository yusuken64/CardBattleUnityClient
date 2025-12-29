using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class EquipWeaponAnimation : GameActionAnimation<EquipWeaponAction>
{
	public AudioClip EquipWeaponClip;
	public override IEnumerator Play()
	{
		Common.Instance.AudioManager.PlayClip(EquipWeaponClip);

		var equipWeaponAction = Action;
		var player = GameManager.GetPlayerFor(Context.Target as CardBattleEngine.Player);
		player.Weapon.gameObject.SetActive(true);
		player.Weapon.transform.localScale = Vector3.zero;
		player.Weapon.Setup(equipWeaponAction.Weapon);

		player.Weapon.transform.DOScale(Vector3.one, 0.3f);
		player.Weapon.transform.DOPunchScale(Vector3.one * 1.1f, 0.2f);

		//player.Weapon.RefreshData();
		player.Attack += player.Data.Attack;
		player.UpdateUI();

		yield return new WaitForSecondsRealtime(0.3f);
	}
}

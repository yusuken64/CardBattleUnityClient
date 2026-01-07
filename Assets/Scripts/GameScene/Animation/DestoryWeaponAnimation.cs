using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class DestoryWeaponAnimation : GameActionAnimation<DestroyWeaponAction>
{
	public override IEnumerator Play()
	{
		var destroyWeaponAction = Action as DestroyWeaponAction;
		var player = GameManager.GetPlayerFor(Context.Target as CardBattleEngine.Player);
		player.Weapon.Setup(null);

		player.Weapon.transform.DOScale(0, 0.3f);
		player.Weapon.transform.DOShakePosition(0.3f);

		yield return new WaitForSecondsRealtime(0.3f);

		player.Weapon.gameObject.SetActive(false);
	}
}
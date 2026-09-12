using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class DestoryWeaponAnimation : GameActionAnimation<DestroyWeaponAction>
{
	public override IEnumerator Play()
	{
		var player = GameManager.GetPlayerFor(Presentation.Target as CardBattleEngine.Player);
		player.Weapon.Setup(null);

		player.Weapon.transform.DOScale(0, 0.3f).SetId(Presentation);
		player.Weapon.transform.DOShakePosition(0.3f).SetId(Presentation);

		yield return new WaitForSecondsRealtime(0.3f);

		player.Weapon.gameObject.SetActive(false);
	}
}

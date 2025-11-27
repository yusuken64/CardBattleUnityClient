using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class EquipWeaponAnimation : GameActionAnimation<EquipWeaponAction>
{
	public EquipWeaponAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current) : base(gameManager, state, current)
	{
	}

	public override IEnumerator Play()
	{
		var equipWeaponAction = Action;
		var player = GameManager.GetPlayerFor(Context.Target as CardBattleEngine.Player);
		player.Weapon.gameObject.SetActive(true);
		player.Weapon.transform.localScale = Vector3.zero;
		player.Weapon.Setup(equipWeaponAction.Weapon);

		player.Weapon.transform.DOScale(Vector3.one, 0.3f);
		player.Weapon.transform.DOPunchScale(Vector3.one * 1.1f, 0.2f);

		player.RefreshData();

		yield return new WaitForSecondsRealtime(0.3f);
	}
}

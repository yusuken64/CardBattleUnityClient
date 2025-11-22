using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class EquipWeaponAnimation : GameActionAnimation<EquipWeaponAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public EquipWeaponAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		var equipWeaponAction = current.action as EquipWeaponAction;
		var player = gameManager.GetPlayerFor(current.context.Target as CardBattleEngine.Player);
		player.Weapon.gameObject.SetActive(true);
		player.Weapon.transform.localScale = Vector3.zero;
		player.Weapon.Setup(equipWeaponAction.Weapon);

		player.Weapon.transform.DOScale(Vector3.one, 0.3f);
		player.Weapon.transform.DOPunchScale(Vector3.one * 1.1f, 0.2f);

		player.RefreshData(player.Data == state.CurrentPlayer);

		yield return new WaitForSecondsRealtime(0.3f);
	}
}

public class DestoryWeaponAnimation : GameActionAnimation<DestroyWeaponAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public DestoryWeaponAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		var destroyWeaponAction = current.action as DestroyWeaponAction;
		var player = gameManager.GetPlayerFor(current.context.Target as CardBattleEngine.Player);
		player.Weapon.Setup(null);

		player.Weapon.transform.DOScale(0, 0.3f);
		player.Weapon.transform.DOShakePosition(0.3f);

		yield return new WaitForSecondsRealtime(0.3f);

		player.Weapon.gameObject.SetActive(false);
	}
}
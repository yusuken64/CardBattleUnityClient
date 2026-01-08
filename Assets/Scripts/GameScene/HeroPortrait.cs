using CardBattleEngine;
using System;
using TMPro;
using UnityEngine;

public class HeroPortrait : MonoBehaviour, ITargetOrigin, ITargetable
{
	public Player Player;
	public GameObject HealthObject;
	public GameObject AttackObject;
	public GameObject ArmorObject;
	public TextMeshProUGUI HealthText;
	public TextMeshProUGUI AttackText;
	public TextMeshProUGUI ArmorText;

	public GameObject FreezeIndicator;
	private UI _ui;

	private void Start()
	{
		_ui = FindFirstObjectByType<UI>();
	}

	internal void UpdateUI()
	{
		HealthText.text = Player.Health.ToString();
		AttackText.text = Player.Attack.ToString();
		ArmorText.text = Player.Armor.ToString();

		HealthObject.gameObject.SetActive(true);
		AttackObject.gameObject.SetActive(Player.Attack != 0);
		ArmorObject.gameObject.SetActive(Player.Armor != 0);

		FreezeIndicator.gameObject.SetActive(Player.IsFrozen);

		if (_ui != null)
		{
			AttackText.color = _ui.GetColor(Player.Attack, Player.Attack, Player.Attack);
			HealthText.color = _ui.GetColor(Player.Health, 30, Player.MaxHealth);
		}
	}

	public AimIntent AimIntent { get; set; } = AimIntent.Attack;

	public GameObject DragObject => this.gameObject;

	public bool CanStartAiming()
	{
		return Player.Data.CanAttack();
	}

	public IGameEntity GetData()
	{
		return Player.Data;
	}

	public CardBattleEngine.Player GetPlayer()
	{
		return Player.Data;
	}

	public bool WillResolveSuccessfully(
		ITargetable target,
		GameObject pendingAimObject,
		out (IGameAction, ActionContext) current,
		Vector3 mousePos,
		out string reason)
	{
		var gameManager = FindFirstObjectByType<GameManager>();

		AttackAction attackAction = new();
		ActionContext context = new()
		{
			SourcePlayer = Player.Data,
			Source = Player.Data,
			Target = target.GetData()
		};

		current = (attackAction, context);
		return gameManager.CheckIsValid(attackAction, context, out reason);
	}


	public void ResolveAim((IGameAction action, ActionContext context) current, GameObject gameObject)
	{
		var gameManager = FindFirstObjectByType<GameManager>();
		gameManager.ResolveAction(current.action, current.context);
	}
}

using CardBattleEngine;
using UnityEngine;

public class HeroPortrait : MonoBehaviour, ITargetOrigin
{
	public Player Player;
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

	public bool WillResolveSuccessfully(ITargetable target, GameObject pendingAimObject, out (IGameAction, ActionContext) current)
	{
		var gameManager = FindFirstObjectByType<GameManager>();

		AttackAction attackAction = new();
		ActionContext context = new()
		{
			SourcePlayer = Player.Data,
			Target = target.GetData()
		};

		current = (attackAction, context);
		return gameManager.ChecksValid(attackAction, context);
	}


	public void ResolveAim((IGameAction action, ActionContext context) current)
	{
		var gameManager = FindFirstObjectByType<GameManager>();
		gameManager.ResolveAction(current.action, current.context);
	}
}

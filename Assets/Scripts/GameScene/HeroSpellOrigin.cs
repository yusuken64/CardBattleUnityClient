using CardBattleEngine;
using UnityEngine;

public class HeroSpellOrigin : MonoBehaviour, ITargetOrigin
{
	public Player Player;
	public AimIntent AimIntent { get; set; } = AimIntent.CastSpell;

	public GameObject DragObject => this.gameObject;

	public bool CanStartAiming()
	{
		return false;
	}

	public IGameEntity GetData()
	{
		return Player.Data;
	}

	public CardBattleEngine.Player GetPlayer()
	{
		return Player.Data;
	}

	public void ResolveAim((IGameAction action, ActionContext context) current)
	{
		var gameManager = FindFirstObjectByType<GameManager>();
		gameManager.ResolveAction(current.action, current.context);
	}

	public bool WillResolveSuccessfully(ITargetable target, GameObject pendingAimObject, out (IGameAction, ActionContext) current)
	{
		var pendingCard = pendingAimObject.GetComponent<Card>();
		if (pendingCard == null)
		{
			current = (null, null);
			return false;
		}

		var gameManager = FindFirstObjectByType<GameManager>();
		CastSpellAction action = new();
		ActionContext context = new()
		{
			SourceCard = pendingCard.Data,
			SourcePlayer = pendingCard.Data.Owner,
			Target = target.GetData()
		};

		current = (action, context);
		return gameManager.ChecksValid(action, context);
	}
}

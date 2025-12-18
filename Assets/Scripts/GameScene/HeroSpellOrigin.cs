using CardBattleEngine;
using System.Linq;
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

	public void ResolveAim((IGameAction action, ActionContext context) current, GameObject dragObject)
	{
		var gameManager = FindFirstObjectByType<GameManager>();
		gameManager.ResolveAction(current.action, current.context);

		if (dragObject != null)
		{
			var card = dragObject.GetComponent<Card>();
			if (card != null)
			{
				Player.Hand.Cards.Remove(card);
				Destroy(card.gameObject);
			}
		}
		Player.Hand.UpdateCardPositions();
	}

	public bool WillResolveSuccessfully(
		ITargetable target,
		GameObject pendingAimObject,
		out (IGameAction, ActionContext) current,
		Vector3 mousePos,
		out string reason)
	{
		var pendingCard = pendingAimObject.GetComponent<Card>();
		if (pendingCard == null)
		{
			current = (null, null);
			reason = null;
			return false;
		}

		var gameManager = FindFirstObjectByType<GameManager>();
		PlayCardAction action = new()
		{
			Card = pendingCard.Data
		};
		ActionContext context = new()
		{
			SourceCard = pendingCard.Data,
			SourcePlayer = pendingCard.Data.Owner,
			Target = target.GetData()
		};

		current = (action, context);
		return gameManager.CheckIsValid(action, context, out reason);
	}
}

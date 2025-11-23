using CardBattleEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroPower : MonoBehaviour, ITargetOrigin, IHoverable
{
	public Player Player;
	public Image HeroPowerImage;
	public GameObject HeroPowerExpiredObject;
	public GameObject HeroPowerCostObject;
	public TextMeshProUGUI HeroPowerCostText;

	public CardBattleEngine.HeroPower Data { get; set; }
	internal void RefreshData()
	{
		if (Data == null)
		{
			this.gameObject.SetActive(false);
			return;
		}

		//FindFirstObjectByType<CardManager>();
		HeroPowerExpiredObject.SetActive(Data.UsedThisTurn);
		HeroPowerCostObject.SetActive(Data.ManaCost >= 0);
		HeroPowerCostText.text = Data.ManaCost.ToString();

		//HeroPowerImage.sprite = FindFirstObjectByType<CardManager>().GetCardByName(Data.Name).Sprite;
	}

	public AimIntent AimIntent { get; set; } = AimIntent.HeroPower;

	public GameObject DragObject => this.gameObject;

	public CardBattleEngine.Card OriginalCard { get; internal set; }

	public bool CanStartAiming()
	{
		return !Data.UsedThisTurn;//TODO heropoweraction.isvalid
	}

	public IGameEntity GetData()
	{
		return Player.Data;
	}

	public CardBattleEngine.Player GetPlayer()
	{
		return Player.Data;
	}

	public void ResolveAim((IGameAction action, ActionContext context) current, GameObject gameObject)
	{
		var gameManager = FindFirstObjectByType<GameManager>();
		gameManager.ResolveAction(current.action, current.context);
	}

	public bool WillResolveSuccessfully(ITargetable target, GameObject pendingAimObject, out (IGameAction, ActionContext) current, Vector3 mousePos)
	{
		var gameManager = FindFirstObjectByType<GameManager>();
		CardBattleEngine.HeroPowerAction action = new CardBattleEngine.HeroPowerAction();
		CardBattleEngine.ActionContext context = new CardBattleEngine.ActionContext()
		{
			Source = Player.Data,
			Target = target.GetData(),
			SourcePlayer = Player.Data
		};

		current = (action, context);
		return gameManager.CheckIsValid(action, context);
	}

	public CardBattleEngine.Card GetDisplayCard()
	{
		return OriginalCard;
	}
}

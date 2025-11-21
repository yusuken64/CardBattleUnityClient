using CardBattleEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroPower : MonoBehaviour, ITargetOrigin
{
	public Player Player;
	public Image HeroPowerImage;
	public GameObject HeroPowerExpiredObject;
	public GameObject HeroPowerCostObject;
	public TextMeshProUGUI HeroPowerCostText;

	public CardBattleEngine.HeroPower Data { get; internal set; }
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

		HeroPowerImage.sprite = FindFirstObjectByType<CardManager>().GetCardByName(Data.Name).Sprite;
	}

	public AimIntent AimIntent { get; set; } = AimIntent.HeroPower;

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

}

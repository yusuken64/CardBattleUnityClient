using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour, IHoverable
{
	public GameObject AttackObject;
	public TextMeshProUGUI AttackText;

	public GameObject DurabilityObject;
	public TextMeshProUGUI DurabilityText;

	public Image WeaponImage;

	public CardBattleEngine.Weapon Data;

	public int Attack;
	public int Durability;
	public Vector3 ToolTipOffset;

	internal void Setup(CardBattleEngine.Weapon weapon)
	{
		Data = weapon;
		if (weapon == null)
		{
			return;
		}

		var cardManager = Common.Instance.CardManager;
		WeaponImage.sprite = cardManager.GetSpriteByCardID(Data.OriginalCard.SpriteID);

		RefreshData();
	}

	public void RefreshData()
	{
		if (Data == null) { return; }

		Attack = Data.Attack;
		Durability = Data.Durability;

		UpdateUI();
	}

	public void UpdateUI()
	{
		gameObject.SetActive(Data != null);
		AttackText.text = Attack.ToString();
		DurabilityText.text = Durability.ToString();
	}

	internal void SyncData(CardBattleEngine.Weapon equippedWeapon)
	{
		if (equippedWeapon != null)
		{
			Attack = equippedWeapon.Attack;
			Durability = equippedWeapon.Durability;
		}
		else
		{
			Attack = 0;
			Durability = 0;
		}

		UpdateUI();
	}

	public CardBattleEngine.Card DisplayCard => Data.OriginalCard;

	public void HoverStart()
	{
		var ui = FindFirstObjectByType<UI>();
		ui.HoverPreviewStart(this);
	}

	public void HoverEnd()
	{
		var ui = FindFirstObjectByType<UI>();
		ui.HoverPreviewEnd();
	}

	public Vector3 GetPosition()
	{
		return this.transform.position + ToolTipOffset;
	}
}

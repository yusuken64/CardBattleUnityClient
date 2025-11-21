using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
	public GameObject AttackObject;
	public TextMeshProUGUI AttackText;

	public GameObject DurabilityObject;
	public TextMeshProUGUI DurabilityText;

	public Image WeaponImage;

	public CardBattleEngine.Weapon Data;

	public int Attack;
	public int Durablity;

	internal void Setup(CardBattleEngine.Weapon weapon)
	{
		Data = weapon;
		if (weapon == null)
		{
			return;
		}

		var cardManager = FindFirstObjectByType<CardManager>();
		WeaponImage.sprite = cardManager.GetCardByName(weapon.Name).Sprite;

		RefreshData();
	}

	public void RefreshData()
	{
		if (Data == null) { return; }

		Attack = Data.Attack;
		Durablity = Data.Durability;

		UpdateUI();
	}

	public void UpdateUI()
	{
		AttackText.text = Attack.ToString();
		DurabilityText.text = Durablity.ToString();
	}
}

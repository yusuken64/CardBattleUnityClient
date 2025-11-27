using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour, ITargetable
{
	public HeroPortrait HeroPortrait;
	public HeroSpellOrigin HeroSpellOrigin;
	public Image HeroImage;
	public GameObject ExplodeParticlePrefab;
	public GameObject CanAttackIndicator;

	public Hand Hand;
	public Board Board;
	public Weapon Weapon;
	public GameObject DrawPile;
	public HeroPower HeroPower;
	public bool CanAttack;

	public CardBattleEngine.Player Data { get; internal set; }

	public int Health;
	public int Attack;
	public int Armor;
	public int Mana;
	public int MaxMana;

	public TextMeshProUGUI ManaText;

	internal void Clear()
	{
		foreach (var card in Hand.Cards)
		{
			Destroy(card.gameObject);
		}

		foreach (var minion in Board.Minions)
		{
			Destroy(minion.gameObject);
		}

		Hand.Cards.Clear();
		Board.Minions.Clear();
	}

	internal void RefreshData(bool activePlayerTurn)
	{
		Health = Data.Health;
		Attack = Data.Attack;
		//Armor = Data.Armor;
		Mana = Data.Mana;
		MaxMana = Data.MaxMana;
		CanAttack = Data.CanAttack();

		if (HeroPower != null)
		{
			//HeroPower.Data = Data.HeroPower;
			HeroPower.RefreshData();
		}

		if (Data.EquippedWeapon == null)
		{
			Weapon.gameObject.SetActive(false);
		}

		foreach(var minion in Board.Minions)
		{
			minion.RefreshData(activePlayerTurn);
		}

		foreach (var card in Hand.Cards)
		{
			card.RefreshData(activePlayerTurn);
		}

		UpdateUI();
	}

	public void UpdateUI()
	{
		HeroPortrait.UpdateUI();
		ManaText.text = $"{Mana}/{MaxMana}";
		CanAttackIndicator.SetActive(CanAttack);
	}

	public CardBattleEngine.IGameEntity GetData()
	{
		return this.Data;
	}

	public AimIntent AimIntent { get; set; } = AimIntent.Attack;

	[ContextMenu("DoDeathRoutine")]
	public void TestRoutine()
	{
		StartCoroutine(DoDeathRoutine());
	}

	internal IEnumerator DoDeathRoutine()
	{
		var shake = HeroPortrait.transform.DOShakePosition(1.7f, 1f, 30);

		var sequence = DOTween.Sequence();
		sequence.Append(HeroImage.DOColor(Color.yellow, 0.8f));
		sequence.Append(HeroImage.DOFade(0, 1.4f));
		//sequence.Append(cardMaterial.DOFloat(0.237f, "_OverlayBlend", 0.8f));
		//sequence.Append(cardMaterial.DOFloat(11.9f, "_OverlayGlow", 0.8f));
		//sequence.Append(cardMaterial.DOFloat(1f, "_HitEffectBlend", 0.8f));
		yield return shake.WaitForCompletion();
		HeroPortrait.gameObject.SetActive(false);

		var explode = Instantiate(ExplodeParticlePrefab, this.transform);
		explode.transform.position = HeroPortrait.transform.position;
		//Common.Instance.AudioManager.PlayClip(Common.Instance.AudioManager.Explosion);
		Destroy(explode.gameObject, 3f);
		yield return new WaitForSecondsRealtime(1f);
		HeroPortrait.gameObject.SetActive(false);
	}
}

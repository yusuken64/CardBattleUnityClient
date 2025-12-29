using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour, ITargetable, IUnityGameEntity
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
	public TextMeshProUGUI DrawPileCount;
	public HeroPower HeroPower;
	public bool CanAttack;

	public CardBattleEngine.Player Data { get; internal set; }

	public int Health;
	public int Attack;
	public int Armor;
	public int Mana;
	public int MaxMana;
	public int CardsLeftInDeck;

	public TextMeshProUGUI ManaText;

	public AudioClip HeroDieStart;
	public AudioClip HeroDieExplode;

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

	internal void RefreshData()
	{
		Health = Data.Health;
		Attack = Data.Attack;
		//Armor = Data.Armor;
		Mana = Data.Mana;
		MaxMana = Data.MaxMana;
		CanAttack = Data.CanAttack();
		CardsLeftInDeck = Data.Deck.Count();

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
			minion.RefreshData();
		}

		foreach (var card in Hand.Cards)
		{
			card.RefreshData();
		}

		UpdateUI();
	}

	public void SyncData(CardBattleEngine.IGameEntity entity)
	{
		var data = entity as CardBattleEngine.Player;
		Health = data.Health;
		Attack = data.Attack;
		//Armor = data.Armor;
		Mana = data.Mana;
		MaxMana = data.MaxMana;
		CanAttack = data.CanAttack();
		CardsLeftInDeck = data.Deck.Count();

		if (HeroPower != null)
		{
			//HeroPower.data = data.HeroPower;
			HeroPower.RefreshData();
		}

		if (data.EquippedWeapon == null)
		{
			Weapon.gameObject.SetActive(false);
		}

		UpdateUI();
	}

	public void UpdateUI()
	{
		if (!this) return;
		HeroPortrait.UpdateUI();
		ManaText.text = $"{Mana}/{MaxMana}";
		DrawPileCount.text = CardsLeftInDeck.ToString();
	}

	public CardBattleEngine.IGameEntity GetData()
	{
		return this.Data;
	}

	public AimIntent AimIntent { get; set; } = AimIntent.Attack;

	public CardBattleEngine.IGameEntity Entity => GetData();

	[ContextMenu("DoDeathRoutine")]
	public void TestRoutine()
	{
		StartCoroutine(DoDeathRoutine());
	}

	internal IEnumerator DoDeathRoutine()
	{
		CanAttackIndicator.gameObject.SetActive(false);
		Common.Instance.AudioManager.PlayClip(HeroDieStart);
		var shake = HeroPortrait.transform.DOShakePosition(1.7f, 1f, 30);

		var sequence = DOTween.Sequence();
		sequence.Append(HeroImage.DOColor(Color.yellow, 0.8f));
		sequence.Append(HeroImage.DOFade(0, 1.4f));
		//sequence.Append(cardMaterial.DOFloat(0.237f, "_OverlayBlend", 0.8f));
		//sequence.Append(cardMaterial.DOFloat(11.9f, "_OverlayGlow", 0.8f));
		//sequence.Append(cardMaterial.DOFloat(1f, "_HitEffectBlend", 0.8f));
		yield return shake.WaitForCompletion();
		HeroPortrait.gameObject.SetActive(false);

		Common.Instance.AudioManager.PlayClip(HeroDieExplode);
		var explode = Instantiate(ExplodeParticlePrefab, this.transform);
		explode.transform.position = HeroPortrait.transform.position;
		//Common.Instance.AudioManager.PlayClip(Common.Instance.AudioManager.Explosion);
		Destroy(explode.gameObject, 3f);
		yield return new WaitForSecondsRealtime(1f);
		HeroPortrait.gameObject.SetActive(false);
	}

	internal void UpdatePlayableActions(bool isActivePlayer)
	{
		CanAttack = isActivePlayer && Data.CanAttack();
		CanAttackIndicator.gameObject.SetActive(CanAttack);
		HeroPower.RefreshData();

		foreach(var card in Hand.Cards)
		{
			card.RefreshData();
			card.CanPlayIndicator.gameObject.SetActive(isActivePlayer && card.CanPlay);
		}

		foreach(var minion in Board.Minions)
		{
			minion.AttackReadyIndicator.gameObject.SetActive(isActivePlayer && minion.CanAttack);
		}
	}
}

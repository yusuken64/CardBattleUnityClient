using CardBattleEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour, IDraggable, IHoverable
{
    public string CardName => this.name;
    public Image CardImage;

    public CardBattleEngine.Card Data { get; private set; }

    #region Display
    public int Cost;
    public int Attack;
    public int Health;
    public bool CanPlay;
    public TextMeshProUGUI CostText;
    public TextMeshProUGUI AttackText;
    public GameObject AttackObject;
    public TextMeshProUGUI HealthText;
    public GameObject HealthObject;
    public GameObject CanPlayIndicator;
    public TextMeshProUGUI CardTypeText;

    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;
    public GameObject TribeObject;
    public TextMeshProUGUI TribeText;

    public GameObject CastIndicator;

    #endregion

    #region Animation
    public Vector2 TargetPosition { get; internal set; }
    public Quaternion TargetAngle { get; internal set; }
    public bool Moving { get; internal set; }

    public CardType CardType => this.Data.Type;

	public GameObject VisualParent;

	public float moveSpeed;
	public float rotateSpeed;
	private Minion _pendingMinion;
	public int _pendingIndex;
	#endregion

	public void ResetVisuals()
	{
        CastIndicator.gameObject.SetActive(false);
        VisualParent.transform.localScale = Vector3.one;
    }

	private void Update()
    {
        if (Moving && !Dragging)
        {
            // Move towards target in local space
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                TargetPosition,
                moveSpeed * Time.deltaTime
            );

            // Rotate in local space
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation,
                TargetAngle,
                rotateSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.localPosition, TargetPosition) < 0.05f &&
                Quaternion.Angle(transform.localRotation, TargetAngle) < 0.5f)
            {
                Moving = false;
            }
        }
        else if (Dragging)
		{
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation,
                Quaternion.Euler(0, 0, 0),
                rotateSpeed * Time.deltaTime
            );
        }
    }

	internal void Setup(CardBattleEngine.Card cardData)
	{
        var cardManager = FindFirstObjectByType<CardManager>();
        var cardDefinition = cardManager.GetCardByName(cardData.Name);
        this.CardImage.sprite = cardDefinition.Sprite;

        this.Data = cardData;
        RefreshData(false);
    }

    private void UpdateUI()
    {
        NameText.text = Data.Name;
        string description = "";
        if (this.Data is MinionCard minionCard)
        {
            TribeObject.gameObject.SetActive(false);
            //TribeObject.gameObject.SetActive(minionCard.MinionTribes.Any(x => x != MinionTribe.None));
            //TribeText.text = minionCard.MinionTribes[0].ToString();

            var keyWords = new List<string>();
            if (minionCard.HasCharge) { keyWords.Add("Charge"); }
            if (minionCard.HasRush) { keyWords.Add("Rush"); }
            if (minionCard.HasTaunt) { keyWords.Add("Taunt"); }
            if (minionCard.HasDivineShield) { keyWords.Add("Divine Shield"); }
            if (minionCard.HasPoisonous) { keyWords.Add("Poison"); }
            if (minionCard.IsStealth) { keyWords.Add("Stealth"); }
            if (minionCard.HasReborn) { keyWords.Add("Reborn"); }
            if (minionCard.HasLifeSteal) { keyWords.Add("LifeSteal"); }
            if (minionCard.HasWindfury) { keyWords.Add("Windfury"); }
            description = string.Join(",", keyWords);

            AttackObject.SetActive(true);
            HealthObject.SetActive(true);
            CardTypeText.text = "Minion";

            if (!string.IsNullOrWhiteSpace(minionCard.Description))
			{
                description += $"{Environment.NewLine}{minionCard.Description}";
			}
        }
		else if (this.Data is WeaponCard weaponCard)
        {
            AttackObject.SetActive(true);
            HealthObject.SetActive(true);
            TribeObject.gameObject.SetActive(false);
            CardTypeText.text = "Weapon";
            description += $"{Environment.NewLine}{weaponCard.Description}";
        }
        else if (this.Data is SpellCard spellCard)
        {
            AttackObject.SetActive(false);
            HealthObject.SetActive(false);
            TribeObject.gameObject.SetActive(false);
            CardTypeText.text = "Spell";
            description += $"{Environment.NewLine}{spellCard.Description}";
        }

        //description = Data.CardText;
        DescriptionText.text = description;

        CostText.text = Cost.ToString();
        CanPlayIndicator.gameObject.SetActive(CanPlay);
        if (AttackText != null) AttackText.text = Attack.ToString();
        if (HealthText != null) HealthText.text = Health.ToString();
    }

	internal void RefreshData(bool activePlayerTurn)
    {
        Cost = this.Data.ManaCost;
        CanPlay = this.Data.Owner.Mana >= Cost && activePlayerTurn;

        if (this.Data is MinionCard minionCard)
        {
            Attack = minionCard.Attack;
            Health = minionCard.Health;
        }
        else if (this.Data is WeaponCard weaponCard)
        {
            Attack = weaponCard.Attack;
            Health -= weaponCard.Durability;
		}
        else if (this.Data is SpellCard spellCard)
        {
            Attack = 0;
            Health -= 0;
        }

        UpdateUI();
    }

    public bool Dragging { get; set; }

    public GameObject DragObject => this.gameObject;

	public bool CanStartDrag() => true;
    public bool RequiresTarget()
    {
        if (this.Data is MinionCard minionCard)
        {
            if (minionCard.TriggeredEffects.Count() == 0)
			{
                return false;
			}

            return minionCard.TriggeredEffects[0].TargetType != TargetingType.None;
        }
        else if (this.Data is SpellCard spellCard)
        {
            if (spellCard.TargetingType == TargetingType.None)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    public GameObject TransitionToAim(Vector3 mousePos)
    {
        var animator = GetComponent<Animator>();
        animator.Play("CardCast", 0, 0f);

        var gameManager = FindFirstObjectByType<GameManager>();
        var player = gameManager.GetPlayerFor(this.Data.Owner);
        if (this.Data is MinionCard minionCard)
        {
            //summon pending minion
            var index = player.Board.Minions.Count(x => x.transform.position.x < mousePos.x);
            var minionPrefab = FindFirstObjectByType<GameInteractionHandler>().MinionPrefab;
            _pendingIndex = index;
            _pendingMinion = Instantiate(minionPrefab, player.Board.transform);
            _pendingMinion.SetupWithCard(Data as CardBattleEngine.MinionCard);
            player.Board.Minions.Insert(_pendingIndex, _pendingMinion);
            player.Board.UpdateMinionPositions();
            _pendingMinion.transform.position = _pendingMinion.TargetPosition;
            this.transform.position = _pendingMinion.TargetPosition;

            return _pendingMinion.gameObject;
        }
        else if (this.Data is SpellCard spellCard)
        {
            //pending cast from player
            this.transform.position = player.HeroSpellOrigin.transform.position;
            return player.HeroSpellOrigin.gameObject;
        }

        return null;
    }

    public void CancelAim()
	{
        StartCoroutine(CancelAimRoutine());
	}

    public void EndAim()
	{
        _pendingIndex = -1;
        _pendingMinion = null;
        CastIndicator.gameObject.SetActive(false);

        var gameManager = FindFirstObjectByType<GameManager>();
        var player = gameManager.GetPlayerFor(this.Data.Owner);
        player.Board.UpdateMinionPositions();
        player.Hand.UpdateCardPositions();

        GameInteractionHandler cardInteractionController = FindFirstObjectByType<GameInteractionHandler>();
        cardInteractionController.MinionPlayPreview.gameObject.SetActive(false);
    }

	private IEnumerator CancelAimRoutine()
	{
        GameInteractionHandler cardInteractionController = FindFirstObjectByType<GameInteractionHandler>();
        cardInteractionController.MinionPlayPreview.gameObject.SetActive(false);
        CastIndicator.gameObject.SetActive(false);

        var gameManager = FindFirstObjectByType<GameManager>();
		var player = gameManager.GetPlayerFor(this.Data.Owner);

		if (_pendingMinion != null)
		{
			//play minion clear
			//remove from board
			if (player.Board.Minions.Contains(_pendingMinion))
			{
				player.Board.Minions.Remove(_pendingMinion);
				var pendingAnimator = _pendingMinion.GetComponent<Animator>();
				pendingAnimator.Play("MinionReturn");
			}
			player.Board.UpdateMinionPositions();
			Destroy(_pendingMinion.gameObject, 2f); //the length of animation

            yield return new WaitForSecondsRealtime(2f);
		}

		Dragging = false;
		player.Board.UpdateMinionPositions();
		player.Hand.UpdateCardPositions();
		this.transform.localPosition = this.TargetPosition;

		var animator = GetComponent<Animator>();
		animator.Play("CardAppear", 0, 0f);
        yield return null;
    }

	public IGameEntity GetData()
	{
        return this.Data as IGameEntity;
	}

    public bool CanResolve(Vector3 mousePos, out (IGameAction action, ActionContext context) current)
    {
        var gameManager = FindFirstObjectByType<GameManager>();
        var player = gameManager.GetPlayerFor(Data.Owner);

        var index = player.Board.Minions.Count(x => x.transform.position.x < mousePos.x);

        PlayCardAction action = null;
        ActionContext context = null;
        var card = this;
        if (card.CardType == CardBattleEngine.CardType.Minion)
        {
            if (card.RequiresTarget())
            {
                //CardInteractionController.StartAiming(newMinion.transform);
            }
            else
            {
                action = new CardBattleEngine.PlayCardAction()
                {
                    Card = card.Data,
                };
                context = new CardBattleEngine.ActionContext()
                {
                    SourcePlayer = player.Data,
                    PlayIndex = index
                };
            }
        }
        else if (card.CardType == CardBattleEngine.CardType.Weapon)
        {
            action = new CardBattleEngine.PlayCardAction()
            {
                Card = card.Data,
            };
            context = new CardBattleEngine.ActionContext()
            {
                SourcePlayer = player.Data,
                PlayIndex = index,
                Target = player.Data
            };
        }
        else
        {
            action = new CardBattleEngine.PlayCardAction()
            {
                Card = card.Data,
            };
            context = new CardBattleEngine.ActionContext()
            {
                SourcePlayer = player.Data,
                PlayIndex = index
            };
        }
        current = (action, context);
        return gameManager.CheckIsValid(action, context);
    }

    public void Resolve(Vector3 mousePos, (IGameAction action, ActionContext context) current)
    {
        var gameManager = FindFirstObjectByType<GameManager>();
        var player = gameManager.GetPlayerFor(Data.Owner);
        GameInteractionHandler cardInteractionController = FindFirstObjectByType<GameInteractionHandler>();
        cardInteractionController.MinionPlayPreview.gameObject.SetActive(false);
        var minionPrefab = cardInteractionController.MinionPrefab;
        var index = player.Board.Minions.Count(x => x.transform.position.x < mousePos.x);

        //play the card
        var animator = GetComponent<Animator>();
        animator.Play("CardCast", 0, 0f);

        var card = this;
        var cardIndex = player.Hand.Cards.IndexOf(card);
        player.Hand.Cards.Remove(card);
        player.Hand.UpdateCardPositions();
        CastIndicator.gameObject.SetActive(false);

        if (card.CardType == CardBattleEngine.CardType.Minion)
        {
            var newMinion = Instantiate(minionPrefab, player.Board.transform);
            //var minionData = new CardBattleEngine.Minion(card.Data as CardBattleEngine.MinionCard, player.Data);
            newMinion.SetupWithCard(card.Data as CardBattleEngine.MinionCard);
            player.Board.Minions.Insert(index, newMinion);
            player.Board.UpdateMinionPositions();
            newMinion.transform.position = newMinion.TargetPosition;

            animator.Play("MinionAppear");
            card.transform.position = newMinion.transform.position;

            if (card.RequiresTarget())
            {
                //CardInteractionController.StartAiming(newMinion.transform);
            }
            else
            {
                gameManager.ResolveAction(current.action, current.context);
            }
            Destroy(card.gameObject, 2f);
        }
        else if (card.CardType == CardBattleEngine.CardType.Weapon)
        {
            gameManager.ResolveAction(current.action, current.context);
            Destroy(card.gameObject, 2f);
        }
        else
        {
            gameManager.ResolveAction(current.action, current.context);
            Destroy(card.gameObject, 2f);
        }
    }

	public void PreviewPlayOverBoard(Vector3 mousePos, bool mouseOverBoard)
    {
        var gameManager = FindFirstObjectByType<GameManager>();
        var player = gameManager.GetPlayerFor(Data.Owner);
        var minionPlayPreview = FindFirstObjectByType<GameInteractionHandler>().MinionPlayPreview;
        var index = player.Board.Minions.Count(x => x.transform.position.x < mousePos.x);

        CastIndicator.gameObject.SetActive(mouseOverBoard);

        if (!mouseOverBoard ||
            index == -1 ||
            Data.Type != CardType.Minion)
        {
            player.Board.UpdateMinionPositions();
            minionPlayPreview.gameObject.SetActive(false);
        }
        else
        {
            var originalMinions = player.Board.Minions.ToList();
            originalMinions.Insert(index, minionPlayPreview);
            player.Board.UpdateMinionPositions(originalMinions);
            minionPlayPreview.gameObject.SetActive(true);
        }
    }

	public void CancelDrag()
    {
        CastIndicator.gameObject.SetActive(false);

        var gameManager = FindFirstObjectByType<GameManager>();
        var player = gameManager.GetPlayerFor(Data.Owner);
        player.Board.UpdateMinionPositions();
        player.Hand.UpdateCardPositions();
    }

	public CardBattleEngine.Card GetDisplayCard()
	{
        return this.Data;
	}
}

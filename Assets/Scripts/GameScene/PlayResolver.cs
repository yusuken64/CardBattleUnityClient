using System.Linq;
using UnityEngine;

public class PlayResolver : MonoBehaviour
{
    public Card CardPrefab;
    public Minion MinionPrefab;

    public CardInteractionController CardInteractionController;
	public Minion MinionPlayPreview;

	private Player pendingSpellPlayer;
	private Card pendingSpellCard;
	private int pendingCardIndex;
	private Minion pendingTargetingMinion;

	private void Start()
	{
		MinionPlayPreview.gameObject.SetActive(false);
	}

	private void OnEnable()
    {
		CardInteractionController.CardPlayed += CardInteractionController_CardPlayed;
		CardInteractionController.SpellPlayedPreview += CardInteractionController_SpellPlay;
		CardInteractionController.MinionPlayedPreview += CardInteractionController_MinionPlayedPreview;
		CardInteractionController.TargetSelected += CardInteractionController_TargetSelected;
		CardInteractionController.TargetingCanceled += CardInteractionController_TargetingCanceled;
	}

	private void OnDisable()
	{
		CardInteractionController.CardPlayed -= CardInteractionController_CardPlayed;
		CardInteractionController.SpellPlayedPreview -= CardInteractionController_SpellPlay;
		CardInteractionController.MinionPlayedPreview -= CardInteractionController_MinionPlayedPreview;
		CardInteractionController.TargetSelected -= CardInteractionController_TargetSelected;
		CardInteractionController.TargetingCanceled -= CardInteractionController_TargetingCanceled;
	}

	private void CardInteractionController_TargetSelected(ITargetOrigin source, ITargetable target)
	{
		if (source.AimIntent == AimIntent.CastSpell)
		{
			var gameManager = FindFirstObjectByType<GameManager>();
			CardBattleEngine.PlayCardAction action = new CardBattleEngine.PlayCardAction()
			{
				Card = pendingSpellCard.Data
			};
			CardBattleEngine.ActionContext context = new CardBattleEngine.ActionContext()
			{
				Source = source.GetData(),
				Target = target.GetData(),
				SourcePlayer = source.GetPlayer()
			};
			var isValid = gameManager.ChecksValid(action, context);
			if (isValid)
			{
				gameManager.ResolveAction(
					action,
					context);
			}
			else
			{
				CardInteractionController_TargetingCanceled();
				return;
			}
		}
		else if (source.AimIntent ==  AimIntent.Attack)
		{
			var gameManager = FindFirstObjectByType<GameManager>();
			gameManager.ResolveAction(
				new CardBattleEngine.AttackAction(),
				new CardBattleEngine.ActionContext()
				{
					Source = source.GetData(),
					Target = target.GetData(),
					SourcePlayer = source.GetPlayer()
				});
		}

		if (pendingSpellCard != null)
		{
			Destroy(pendingSpellCard.gameObject, 2f);
			pendingSpellCard = null;
		}

		MinionPlayPreview.gameObject.SetActive(false);
	}

	private void CardInteractionController_CardPlayed(Player player, Card card, int index)
	{
		MinionPlayPreview.gameObject.SetActive(false);

		//Should this happen before this method is even called?
		var gameManager = FindFirstObjectByType<GameManager>();
		var valid = gameManager.ChecksValid(
			new CardBattleEngine.PlayCardAction()
			{
				Card = card.Data
			},
			new CardBattleEngine.ActionContext()
			{
				SourcePlayer = player.Data
			});

		if (!valid)
		{
			pendingSpellPlayer = player;
			pendingSpellCard = card;
			FindFirstObjectByType<UI>().ShowMessage("Invalid Play");
			CardInteractionController_TargetingCanceled();
			return;
		}

		var animator = card.GetComponent<Animator>();
		animator.Play("CardCast", 0, 0f);

		var cardIndex = player.Hand.Cards.IndexOf(card);
		player.Hand.Cards.Remove(card);
		player.Hand.UpdateCardPositions();

		if (card.CardType == CardBattleEngine.CardType.Minion)
		{
			var newMinion = Instantiate(MinionPrefab, player.Board.transform);
			//var minionData = new CardBattleEngine.Minion(card.Data as CardBattleEngine.MinionCard, player.Data);
			newMinion.SetupWithCard(card.Data as CardBattleEngine.MinionCard);
			player.Board.Minions.Insert(index, newMinion);
			player.Board.UpdateMinionPositions();

			animator.Play("MinionAppear");
			card.transform.position = newMinion.transform.position;

			if (card.RequiresTarget())
			{
				pendingSpellPlayer = player;
				pendingSpellCard = card;
				pendingCardIndex = cardIndex;
				pendingTargetingMinion = newMinion;
				CardInteractionController.StartAiming(newMinion.transform);
			}
			else
			{
				gameManager.ResolveAction(
					new CardBattleEngine.PlayCardAction()
					{
						Card = card.Data,
					},
					new CardBattleEngine.ActionContext()
					{
						SourcePlayer = player.Data,
						PlayIndex = index
					});
			}
		}
		else if (card.CardType == CardBattleEngine.CardType.Weapon)
		{
			gameManager.ResolveAction(
				new CardBattleEngine.PlayCardAction()
				{
					Card = card.Data,
				},
				new CardBattleEngine.ActionContext()
				{
					SourcePlayer = player.Data,
					PlayIndex = index,
					Target = player.Data
				});
			Destroy(card.gameObject, 2f);
		}
		else
		{
			gameManager.ResolveAction(
				new CardBattleEngine.PlayCardAction()
				{
					Card = card.Data,
				},
				new CardBattleEngine.ActionContext()
				{
					SourcePlayer = player.Data,
					PlayIndex = index
				});
			Destroy(card.gameObject, 2f);
		}
	}

	private void CardInteractionController_MinionPlayedPreview(Player player, Card card, int index)
	{
		if (index == -1)
		{
			player.Board.UpdateMinionPositions();
			MinionPlayPreview.gameObject.SetActive(false);
		}
		else
		{
			var originalMinions = player.Board.Minions.ToList();
			originalMinions.Insert(index, MinionPlayPreview);
			player.Board.UpdateMinionPositions(originalMinions);

			MinionPlayPreview.gameObject.SetActive(true);
		}
	}

	private void CardInteractionController_SpellPlay(Player player, Card card)
	{
		pendingSpellCard = card;
		pendingSpellPlayer = player;
		pendingCardIndex = player.Hand.Cards.IndexOf(card);

		var animator = card.GetComponent<Animator>();
		animator.Play("CardCast", 0, 0f);
		player.Hand.Cards.Remove(card);

		//Destroy(card.gameObject, 2f);
		player.Hand.UpdateCardPositions();

		if (card.RequiresTarget())
		{
			CardInteractionController.StartAiming(player.HeroSpellOrigin.transform);
		}
	}

	private void CardInteractionController_TargetingCanceled()
	{
		var player = pendingSpellPlayer;
		var card = pendingSpellCard;

		if (player != null && card != null)
		{
			if (!player.Hand.Cards.Contains(card))
			{
				// Return card to hand list
				player.Hand.Cards.Insert(pendingCardIndex, card);

				// Move card back near hand before layout snap
				card.transform.position = player.Hand.transform.position;

				// Reactivate if disabled
				card.gameObject.SetActive(true);

				var director = card.GetComponent<Animator>();
				director.Play("CardAppear");
			}

			// Restore card's visuals and sorting
			player.Hand.UpdateCardPositions();
		}

		// Reverse pending minion if there is one
		if (pendingTargetingMinion != null)
		{
			var animator = pendingTargetingMinion.GetComponent<Animator>();
			animator.Play("MinionReturn");

			// Remove it from the board list so layout updates properly
			player.Board.Minions.Remove(pendingTargetingMinion);

			// Destroy after reversed animation finishes
			Destroy(pendingTargetingMinion.gameObject, 2f);

			pendingTargetingMinion = null;
		}
		player?.Board.UpdateMinionPositions();

		// Cleanup
		pendingSpellCard = null;
		pendingSpellPlayer = null;

		// Also restore minion preview etc.
		MinionPlayPreview.gameObject.SetActive(false);
	}
}

public interface ITargetOrigin
{
	public AimIntent AimIntent { get; set; }
	bool CanStartAiming();
	CardBattleEngine.IGameEntity GetData();
	CardBattleEngine.Player GetPlayer();
}

public enum AimIntent
{
	Attack,
	CastSpell
}

public interface ITargetable
{
	CardBattleEngine.IGameEntity GetData();
}

public interface IDraggable
{
	bool Dragging { get; set; }

	bool CanStartDrag();
}
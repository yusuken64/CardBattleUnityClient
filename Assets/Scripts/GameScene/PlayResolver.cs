using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

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

	private void CardInteractionController_TargetSelected(ITargetSource source, ITargetable target)
	{
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
		var animator = card.GetComponent<Animator>();
		animator.Play("CardCast", 0, 0f);

		var cardIndex = player.Hand.Cards.IndexOf(card.gameObject);
		player.Hand.Cards.Remove(card.gameObject);
		player.Hand.UpdateCardPositions();

		if (card.CardType == CardBattleEngine.CardType.Minion)
		{
			var newMinion = Instantiate(MinionPrefab, player.Board.transform);
			player.Board.Minions.Insert(index, newMinion);
			player.Board.UpdateMinionPositions();

			animator.Play("MinionAppear");
			card.transform.position = newMinion.transform.position;

			if (card.RequiresTarget)
			{
				pendingSpellPlayer = player;
				pendingSpellCard = card;
				pendingCardIndex = cardIndex;
				pendingTargetingMinion = newMinion;
				CardInteractionController.StartAiming(newMinion.transform);
			}
		}
		else
		{
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
		pendingCardIndex = player.Hand.Cards.IndexOf(card.gameObject);

		var animator = card.GetComponent<Animator>();
		animator.Play("CardCast", 0, 0f);
		player.Hand.Cards.Remove(card.gameObject);

		//Destroy(card.gameObject, 2f);
		player.Hand.UpdateCardPositions();

		if (card.RequiresTarget)
		{
			CardInteractionController.StartAiming(player.HeroPortrait.transform);
		}
	}

	private void CardInteractionController_TargetingCanceled()
	{
		var player = pendingSpellPlayer;
		var card = pendingSpellCard;

		if (player != null && card != null)
		{
			// Return card to hand list
			player.Hand.Cards.Insert(pendingCardIndex, card.gameObject);

			// Move card back near hand before layout snap
			card.transform.position = player.Hand.transform.position;

			// Reactivate if disabled
			card.gameObject.SetActive(true);

			// Remove any aiming graphics
			//CardInteractionController.CancelAimingImmediate();

			// Restore card's visuals and sorting
			player.Hand.UpdateCardPositions();
			
			var director = card.GetComponent<Animator>();
			director.Play("CardAppear");
		}

		// Reverse pending minion if there is one
		if (pendingTargetingMinion != null)
		{
			var animator = pendingTargetingMinion.GetComponent<Animator>();
			animator.Play("MinionReturn");

			// Remove it from the board list so layout updates properly
			player.Board.Minions.Remove(pendingTargetingMinion);
			player.Board.UpdateMinionPositions();

			// Destroy after reversed animation finishes
			Destroy(pendingTargetingMinion.gameObject, 2f);

			pendingTargetingMinion = null;
		}

		// Cleanup
		pendingSpellCard = null;
		pendingSpellPlayer = null;

		// Also restore minion preview etc.
		MinionPlayPreview.gameObject.SetActive(false);
	}
}

public interface ITargetSource
{
}

public interface ITargetable
{
}
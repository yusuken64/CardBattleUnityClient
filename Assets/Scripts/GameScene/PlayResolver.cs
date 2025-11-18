using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

public class PlayResolver : MonoBehaviour
{
    public Card CardPrefab;
    public Minion MinionPrefab;

    public CardInteractionController CardInteractionController;
	public Minion MinionPlayPreview;

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
		MinionPlayPreview.gameObject.SetActive(false);
	}

	private void CardInteractionController_CardPlayed(Player player, Card card, int index)
	{
		MinionPlayPreview.gameObject.SetActive(false);
		card.GetComponent<PlayableDirector>().Play();
		player.Hand.Cards.Remove(card.gameObject);
		Destroy(card.gameObject, 2f);
		player.Hand.UpdateCardPositions();

		if (card.CardType == CardBattleEngine.CardType.Minion)
		{
			var newMinion = Instantiate(MinionPrefab, player.Board.transform);
			player.Board.Minions.Insert(index, newMinion);
			player.Board.UpdateMinionPositions();
			newMinion.GetComponent<PlayableDirector>().Play();
			card.transform.position = newMinion.transform.position;

			if (card.RequiresTarget)
			{
				CardInteractionController.StartAiming(newMinion.transform);
			}
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
		card.GetComponent<PlayableDirector>().Play();
		player.Hand.Cards.Remove(card.gameObject);
		Destroy(card.gameObject, 2f);
		player.Hand.UpdateCardPositions();

		if (card.RequiresTarget)
		{
			CardInteractionController.StartAiming(player.HeroPortrait.transform);
		}
	}

	private void CardInteractionController_TargetingCanceled()
	{
		var player = FindFirstObjectByType<Player>();
		player.Board.UpdateMinionPositions();
		player.Hand.UpdateCardPositions();
	}
}

public interface ITargetSource
{
}

public interface ITargetable
{
}
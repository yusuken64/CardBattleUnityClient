using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using System.Linq;
using UnityEngine;

public class PlayCardAnimation : GameActionAnimation<PlayCardAction>
{
	public AudioClip PlayCardClip;
	public override IEnumerator Play()
	{
		Common.Instance.AudioManager.PlaySound(PlayCardClip);

		var playCardAction = Action;
		var player = this.GameManager.GetPlayerFor(playCardAction.Card.Owner);

		var playedCard = player.Hand.Cards.FirstOrDefault(x => x.Data == Context.SourceCard);

		if (playedCard != null)
		{
			player.Hand.Cards.Remove(playedCard);
			player.Hand.UpdateCardPositions();

			if (playCardAction.Card.Owner == this.GameManager.Opponent.Data)
			{
				UI ui = FindFirstObjectByType<UI>();
				yield return ui.StartCoroutine(PreviewRoutine(playedCard, ui));
			}
			else
			{
				Object.Destroy(playedCard.gameObject, 2.0f);
			}
		}

		yield return null;
	}

	private IEnumerator PreviewRoutine(Card card, UI ui)
	{
		yield return card.transform
			.DOMove(ui.CardPreview.transform.position, 0.5f)
			.SetEase(Ease.OutQuad)
			.WaitForCompletion();

		// Playing a card is a public action - it's no longer secret once it lands here, so reveal it
		// regardless of who played it (otherwise DisplayCard stays null for the opponent's card and
		// the preview never shows anything).
		card.ForceReveal = true;
		ui.PreviewStart(card);

		Object.Destroy(card.gameObject);
		yield return new WaitForSecondsRealtime(2f);

		ui.PreviewEnd();
	}
}

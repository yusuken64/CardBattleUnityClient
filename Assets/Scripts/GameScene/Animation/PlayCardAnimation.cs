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
		Common.Instance.AudioManager.PlayClip(PlayCardClip);

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

		ui.PreviewStart(card);

		yield return new WaitForSecondsRealtime(2f);

		ui.PreviewEnd();

		Object.Destroy(card.gameObject);
	}
}

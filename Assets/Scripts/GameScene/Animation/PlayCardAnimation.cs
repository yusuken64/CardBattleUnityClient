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
				FindFirstObjectByType<UI>().PreviewStart(playedCard);
			}
		}

		if (playCardAction.Card.Owner == this.GameManager.Opponent.Data)
		{
			yield return new WaitForSecondsRealtime(1.0f);
			FindFirstObjectByType<UI>().PreviewEnd();
		}

		yield return null;
	}
}

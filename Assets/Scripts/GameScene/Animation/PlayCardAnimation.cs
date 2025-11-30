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

		var playedCard = player.Hand.Cards.FirstOrDefault(x => x.Data == playCardAction.Card);
		if (playedCard != null)
		{
			//This is always the opponent playing a card
			var animator = playedCard.GetComponent<Animator>();
			animator.Play("CardCast", 0, 0f);
			player.Hand.Cards.Remove(playedCard);
			player.Hand.UpdateCardPositions();

			yield return playedCard.transform.DOMove(Vector3.zero, 0.4f).WaitForCompletion();
		}

		yield return null;
	}
}

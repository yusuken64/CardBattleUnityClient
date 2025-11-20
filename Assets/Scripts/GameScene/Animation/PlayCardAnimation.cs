using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using System.Linq;
using UnityEngine;

public class PlayCardAnimation : GameActionAnimation<PlayCardAction>
{
	private GameManager gameManager;
	private GameState state;
	private (IGameAction action, ActionContext context) current;

	public PlayCardAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
	{
		this.gameManager = gameManager;
		this.state = state;
		this.current = current;
	}

	public override IEnumerator Play()
	{
		var playCardAction = (current.action as PlayCardAction);
		var player = this.gameManager.GetPlayerFor(playCardAction.Card.Owner);

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

		player.ManaText.text = $"{player.Data.Mana}/{player.Data.MaxMana}";
		yield return null;
	}
}

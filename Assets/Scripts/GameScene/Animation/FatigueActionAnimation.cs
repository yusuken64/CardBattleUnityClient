using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class FatigueActionAnimation : GameActionAnimation<FatigueAction>
{
	public Card FatigueCardPrefab;
	public GameObject FatigueParticlesPrefab;

	public override IEnumerator Play()
	{
		var fatigueCard = Presentation.Own(Instantiate(FatigueCardPrefab));
		var playerData = Presentation.SourcePlayer;

		if (playerData == null)
		{
			yield break;
		}

		// No real CardBattleEngine.Card behind this - Setup() is never called - so nothing else
		// ever reveals it past its default (closed) FlippableCard state.
		fatigueCard.FlippableCard?.SetToFront();

		fatigueCard.DescriptionText.text = @$"No Cards Left in Deck.
Take {Presentation.Fatigue} Damage";

		var player = GameManager.GetPlayerFor(Presentation.SourcePlayer);
		var startPosition = player.DrawPile.transform.position;

		fatigueCard.transform.position = startPosition;
		fatigueCard.Dragging = true;

		Sequence seq = DOTween.Sequence().SetId(Presentation);

		// Move to center
		seq.Append(
			fatigueCard.transform.DOMove(Vector3.zero, 1f).SetId(Presentation)
				.SetEase(Ease.OutCubic)
		);
		seq.Join(
			fatigueCard.transform.DOScale(Vector3.one * 2, 1f).SetId(Presentation)
				.SetEase(Ease.OutCubic)
		);

		// Small dramatic pause
		seq.AppendInterval(0.4f);

		// Spawn particles
		seq.AppendCallback(() =>
		{
			var particles = Presentation.Own(Instantiate(FatigueParticlesPrefab, fatigueCard.transform.position, Quaternion.identity));
			particles.transform.localScale = Vector3.one * 2;
		});

		seq.AppendInterval(0.3f);

		// Destroy card
		seq.AppendCallback(() =>
		{
			Destroy(fatigueCard.gameObject);
		});
		yield return seq.WaitForCompletion();
	}
}

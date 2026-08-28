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
		var fatigueCard = Instantiate(FatigueCardPrefab);
		var playerData = ClonedState.GetEntityById(Context.SourcePlayer.Id) as CardBattleEngine.Player;

		if (playerData == null)
		{
			yield break;
		}

		fatigueCard.DescriptionText.text = @$"No Cards Left in Deck.
Take {playerData.Fatigue} Damage";

		var player = GameManager.GetPlayerFor(Context.SourcePlayer);
		var startPosition = player.DrawPile.transform.position;

		fatigueCard.transform.position = startPosition;
		fatigueCard.Dragging = true;

		Sequence seq = DOTween.Sequence();

		// Move to center
		seq.Append(
			fatigueCard.transform.DOMove(Vector3.zero, 1f)
				.SetEase(Ease.OutCubic)
		);
		seq.Join(
			fatigueCard.transform.DOScale(Vector3.one * 2, 1f)
				.SetEase(Ease.OutCubic)
		);

		// Small dramatic pause
		seq.AppendInterval(0.4f);

		// Spawn particles
		seq.AppendCallback(() =>
		{
			var particles = Instantiate(FatigueParticlesPrefab, fatigueCard.transform.position, Quaternion.identity);
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

using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CollectionEffect : MonoBehaviour
{
	public CanvasGroup FullCard;
	public CanvasGroup SmallCard;

	public Collection Collection;
	public VerticalDeckViewer VerticalDeckViewer;

	public void Start()
	{
		FullCard.gameObject.SetActive(false);
		SmallCard.gameObject.SetActive(false);
	}

	public void OnEnable()
	{
		VerticalDeckViewer.CardAdded += VerticalDeckViewer_CardAdded;
		VerticalDeckViewer.CardRemoved += VerticalDeckViewer_CardRemoved;
	}

	private void OnDisable()
	{
		VerticalDeckViewer.CardAdded -= VerticalDeckViewer_CardAdded;
		VerticalDeckViewer.CardRemoved -= VerticalDeckViewer_CardRemoved;
	}

	private void VerticalDeckViewer_CardAdded(DeckCard obj)
	{
		var collectionItem = Collection.showingItems.FirstOrDefault(x => x.CardDefinition == obj.CardDefinition);
		if (collectionItem == null)
		{
			return;
		}

		FullCard.gameObject.SetActive(true);

		var startPos = collectionItem.transform.position;
		var endPos = startPos + new Vector3(500f, 0f, 0f);

		FullCard.DOKill();

		FullCard.transform.position = startPos;
		FullCard.alpha = 1f;

		DOTween.Sequence()
			.Join(FullCard.transform.DOMove(endPos, 0.2f).SetEase(Ease.OutQuad))
			.Join(FullCard.DOFade(0f, 0.2f))
			.OnComplete(() =>
			{
				FullCard.gameObject.SetActive(false);
			});
	}

	private void VerticalDeckViewer_CardRemoved(DeckCard obj)
	{
		if (obj == null) { return; }

		SmallCard.gameObject.SetActive(true);

		var startPos = obj.transform.position;
		var endPos = startPos + new Vector3(-500f, 0f, 0f);

		SmallCard.DOKill();

		SmallCard.transform.position = startPos;
		SmallCard.alpha = 1f;

		DOTween.Sequence()
			.Join(SmallCard.transform.DOMove(endPos, 0.2f).SetEase(Ease.OutQuad))
			.Join(SmallCard.DOFade(0f, 0.2f))
			.OnComplete(() =>
			{
				SmallCard.gameObject.SetActive(false);
			});
	}
}

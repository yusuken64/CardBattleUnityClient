using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlippingDeck : MonoBehaviour
{
    public FlippableCard FlippableCardPrefab;
    public int Count;
    public float Delay;
    public float MoveTime;
    public float FlipDuration;
    public List<FlippableCard> FlippableCards;

    public Vector3 StartPosition;
    public Vector3 EndPosition;

    public Transform In;
    public Transform Out;

    void Start()
    {
        FlippableCards = new();
        for (int i = 0; i < Count; i++)
		{
            var newCard = Instantiate(FlippableCardPrefab, this.transform, false);
            newCard.transform.localPosition = StartPosition;
            newCard.transform.SetAsFirstSibling();
            //TODO newCard.Setup
            FlippableCards.Add(newCard);

        }

        StartCoroutine(FlipCardRoutine());
    }

	private IEnumerator FlipCardRoutine()
	{
        foreach(var card in FlippableCards)
		{
            card.CanFlip = true;
            card.FlipCard = null;
            card.FlipDuration = FlipDuration;
            card.FlipMidPoint = () =>
            {
                //card.transform.SetParent(Out);
                card.transform.SetAsLastSibling();
            };
            card.Flip();
            card.transform.DOLocalMove(EndPosition, MoveTime);

            yield return new WaitForSeconds(Delay);
		}
	}
}

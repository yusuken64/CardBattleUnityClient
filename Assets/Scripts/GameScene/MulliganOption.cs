using System;
using UnityEngine;

public class MulliganOption : MonoBehaviour
{
	public Transform CardParent;
	public Card Card;

	public GameObject MullIndicator;
	public bool Keep;

	internal void Setup(Card card)
	{
		Card = card;
		Card.Dragging = true;
		Card.transform.parent = CardParent;
		Card.transform.localPosition = Vector3.zero;

		Card.GetComponent<BoxCollider2D>().enabled = false;
		Keep = true;
		UpdateUI();
	}

	public void Toggle_Click()
	{
		Keep = !Keep;
		UpdateUI();
	}

	private void UpdateUI()
	{
		MullIndicator.gameObject.SetActive(!Keep);
	}
}

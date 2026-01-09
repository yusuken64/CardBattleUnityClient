using System;
using UnityEngine;

public class TutorialPopup : MonoBehaviour
{
	public string TutorialName;

	public Action<string> Action { get; internal set; }

	internal void Show()
	{
		this.gameObject.SetActive(true);
	}

	public void OKButton_Click()
	{
		Action?.Invoke(TutorialName);
	}
}
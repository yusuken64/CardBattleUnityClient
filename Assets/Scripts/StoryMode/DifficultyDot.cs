using UnityEngine;

public class DifficultyDot : MonoBehaviour
{
	public GameObject OnObject;
	public GameObject OffObject;

	public bool IsOn;

	public void Start()
	{
		UpdateUI();
	}

	private void UpdateUI()
	{
		OnObject.gameObject.SetActive(IsOn);
		OffObject.gameObject.SetActive(!IsOn);
	}

	public void TurnOn()
	{
		IsOn = true;
		UpdateUI();
	}

	public void TurnOff()
	{
		IsOn = false;
		UpdateUI();
	}
}

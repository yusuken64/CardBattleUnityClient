using TMPro;
using UnityEngine;

public class PackRedDot : MonoBehaviour
{
	public TextMeshProUGUI CountText;

	private void Start()
	{
		RefreshData();
	}

	public void RefreshData()
	{
		var packs = Common.Instance.SaveManager.SaveData.GameSaveData.PackCount;
		CountText.text = packs.ToString();

		this.gameObject.SetActive(packs > 0);
	}
}

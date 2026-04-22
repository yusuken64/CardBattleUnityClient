using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModItem : MonoBehaviour
{
    public TextMeshProUGUI ModText;
	public Toggle Toggle;
	public ModData ModData;

	public Button PreviewButton;

	public Action<ModItem> PreviewCallBack { get; internal set; }

	internal void Setup(ModData modData)
	{
		ModData = modData;
		ModText.text = $"{ModData.modName} ({modData.cards.Count()} cards)";
		Toggle.isOn = ModData.enabled;
		UpdateUI();
	}

	private void UpdateUI()
	{
		var enabledMods = Common.Instance.SaveManager.SaveData.ModSaveData.EnabledMods;
		var enabled = enabledMods.Contains(ModData.modName);

		PreviewButton.interactable = enabled;
	}

	public bool IsModEnabled()
	{
		return Toggle.isOn;
	}

    public void Toggle_Change(bool change)
	{

	}

	public void PreviewCards_Clicked()
	{
		PreviewCallBack?.Invoke(this);
	}
}
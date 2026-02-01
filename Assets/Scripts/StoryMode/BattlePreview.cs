using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattlePreview : MonoBehaviour
{
	public Image BattleImage;
	public TextMeshProUGUI DescriptionText;
	private StoryModeDungeonDefinition _data;

	public Rondel Rondel;

	internal void Setup(StoryModeDungeonDefinition data)
	{
		this._data = data;

		BattleImage.sprite = data.BattleImage;
		DescriptionText.text = data.Description;
	}

	internal StoryModeDungeonDefinition GetData()
	{
		return this._data;
	}
}

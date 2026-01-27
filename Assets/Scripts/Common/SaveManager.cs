using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class SaveManager : MonoBehaviour
{
	public bool LoadDataAtStart;
	public string _savePath;

	public SaveData SaveData;

	public void Initialize()
	{
		_savePath = Path.Combine(Application.persistentDataPath, "save.json");
		Debug.Log($"SaveManager path: {_savePath}");
	}

	public void Save()
	{
		try
		{
			Debug.Log($"Saving to {_savePath}");
			string json = JsonUtility.ToJson(SaveData, prettyPrint: true);
			File.WriteAllText(_savePath, json);
		}
		catch (System.Exception ex)
		{
			Debug.LogError($"Save failed: {ex}");
		}
	}

	public void Load()
	{
		SaveData saveData;
		try
		{
			if (!LoadDataAtStart ||
				!File.Exists(_savePath))
			{
				Debug.Log($"Save file not found. Creating new save. {_savePath}");
				saveData = new SaveData();
			}
			else
			{
				Debug.Log($"Loading data. {_savePath}");
				string json = File.ReadAllText(_savePath);
				saveData = JsonUtility.FromJson<SaveData>(json);
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogError($"Load failed: {ex}");
			saveData = new SaveData();
		}

		SaveData = saveData;
	}

	public void EnsureData()
	{
		if (SaveData.GameSaveData.DeckSaveDatas.Count() == 0)
		{
			var startingDeck = Common.Instance.StartingDeck;
			SaveData.GameSaveData.DeckSaveDatas.Add(startingDeck.ToDeckData());

			foreach (var card in startingDeck.Cards)
			{
				SaveData.GameSaveData.CardCollection.Add(card.ID, 1);
			}
			Save();
		}
	}

	[ContextMenu("Reset Data")]
	public void ResetData()
	{
		//SaveData = new SaveData();
		SaveData.GameSaveData = new();
	}

	internal void ResetTutorialData()
	{
		SaveData.TutorialSaveData = new();
	}
}

using UnityEngine;
using System.IO;

public class SaveManager
{
	private readonly string _savePath;

	public SaveManager()
	{
		// Save file name, stored under persistentDataPath
		_savePath = Path.Combine(Application.persistentDataPath, "save.json");
	}

	public void Save(GameSaveData gameSaveData)
	{
		try
		{
			Debug.Log($"Saving to {_savePath}");
			string json = JsonUtility.ToJson(gameSaveData, prettyPrint: true);
			File.WriteAllText(_savePath, json);
		}
		catch (System.Exception ex)
		{
			Debug.LogError($"Save failed: {ex}");
		}
	}

	public GameSaveData Load()
	{
		try
		{
			if (!File.Exists(_savePath))
			{
				Debug.Log($"Save file not found. Creating new save. {_savePath}");
				return new GameSaveData();
			}

			string json = File.ReadAllText(_savePath);
			return JsonUtility.FromJson<GameSaveData>(json);
		}
		catch (System.Exception ex)
		{
			Debug.LogError($"Load failed: {ex}");
			return new GameSaveData();
		}
	}
}

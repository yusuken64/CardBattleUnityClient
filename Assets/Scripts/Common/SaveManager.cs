using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
	public bool LoadDataAtStart;
	public string _savePath;

	public SaveData SaveData;

	public void Initialize()
	{
		_savePath = Path.Combine(Application.persistentDataPath, "save.json");
	}

	public void Save(SaveData saveData)
	{
		try
		{
			Debug.Log($"Saving to {_savePath}");
			string json = JsonUtility.ToJson(saveData, prettyPrint: true);
			File.WriteAllText(_savePath, json);
		}
		catch (System.Exception ex)
		{
			Debug.LogError($"Save failed: {ex}");
		}
	}

	public SaveData Load()
	{
		try
		{
			if (!LoadDataAtStart ||
				!File.Exists(_savePath))
			{
				Debug.Log($"Save file not found. Creating new save. {_savePath}");
				SaveData = new SaveData();
			}
			else
			{
				string json = File.ReadAllText(_savePath);
				SaveData = JsonUtility.FromJson<SaveData>(json);
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogError($"Load failed: {ex}");
			SaveData = new SaveData();
		}

		return SaveData;
	}
}

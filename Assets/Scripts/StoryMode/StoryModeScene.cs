using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryModeScene : MonoBehaviour
{
    public Map Map;

    void Start()
	{
        //InitializeGridButtons();

        //Reload Dungeon State
        ReloadDungeonState();
	}

	public void ReloadDungeonState()
	{
        var currentDungeon = Common.Instance.SaveManager.SaveData.GameSaveData.StorySaveData.CurrentDungeon;
        if (currentDungeon == null ||
            currentDungeon.Exited)
		{
            //normal
            Map.gameObject.SetActive(false);
            Map.Dungeon.gameObject.SetActive(false);
        }
		else
		{
            //restore dungeon
            Map.Dungeon.gameObject.SetActive(true);
            Map.Dungeon.Setup();
		}
	}

    public void Deck_Click()
    {
        DeckBuilderPage.ReturnScreenName = "StoryMode";
        Common.Instance.SceneTransition.DoTransition(() =>
        {
            SceneManager.LoadScene("DeckBuilder");
        });
    }

    public void OpenPacks_Click()
    {
        OpenPackScene.ReturnScreenName = "StoryMode";
        Common.Instance.SceneTransition.DoTransition(() =>
        {
            SceneManager.LoadScene("OpenPacks");
        });
    }

    public void Map_Click()
    {
        Common.Instance.SceneTransition.DoTransition(() =>
        {
            Map.gameObject.SetActive(true);
            Map.ShowRegionPicker();
        });
    }

    public void Back_Clicked()
    {
        Common.Instance.SceneTransition.DoTransition(() =>
        {
            SceneManager.LoadScene("Main");
        });
    }
}

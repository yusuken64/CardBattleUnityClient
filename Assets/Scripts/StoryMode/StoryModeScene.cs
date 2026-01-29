using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryModeScene : MonoBehaviour
{
    public Transform Container;
    public BattleGridButton BattleGridButtonPrefab;

    public BattlePreview BattlePreview;

    public List<StoryModeBattleDefinition> Datas;

    public Map Map;

    void Start()
	{
        //InitializeGridButtons();

        //Reload Dungeon State
        ReloadDungeonState();
	}

	private void ReloadDungeonState()
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

	private void InitializeGridButtons()
	{
		foreach (Transform child in Container)
		{
			Destroy(child.gameObject);
		}

		foreach (var data in Datas)
		{
			var newButton = Instantiate(BattleGridButtonPrefab, Container);
			newButton.Setup(data);
			newButton.ClickAction = BattleGridButton_Clicked;
		}

		BattleGridButton_Clicked(null);
	}

	public void BattleGridButton_Clicked(StoryModeBattleDefinition data)
    {
        if (data == null)
        {
            BattlePreview.gameObject.SetActive(false);
        }
        else
        {
            BattlePreview.gameObject.SetActive(true);
            BattlePreview.Setup(data);
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
            Map.Setup();
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

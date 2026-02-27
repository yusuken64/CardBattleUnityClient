using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryModeScene : MonoBehaviour
{
    public GameObject Main;
    public Map Map;
    public MissionList MissionList;

    public MissionDot MissionDot;
    public GameObject Settings;
    public GameObject DataObject;

    void Start()
    {
        MissionList.gameObject.SetActive(false);
        Settings.gameObject.SetActive(false);
        DataObject.gameObject.SetActive(false);

        ReloadDungeonState();
    }

	public void ReloadDungeonState()
    {
        Main.gameObject.SetActive(true);
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

    public void Missions_Click()
    {
        Common.Instance.SceneTransition.DoTransition(() =>
        {
            MissionList.gameObject.SetActive(true);
            MissionList.Setup();
        });
    }


    public void Back_Clicked()
    {
        Settings.gameObject.SetActive(true);
    }

    public void RefreshRedDots()
	{
        MissionDot.RefreshData();
    }

    public void AudioSettings_Click()
    {
        Common.Instance.GlobalSettings.gameObject.SetActive(true);
        Common.Instance.GlobalSettings.SetToAudioSettings();
    }

    public void DisplaySettings_Click()
    {
        Common.Instance.GlobalSettings.gameObject.SetActive(true);
        Common.Instance.GlobalSettings.SetToDisplaySettings();
    }

    public void Close_Click()
    {
        Common.Instance.SaveManager.Save();

        Common.Instance.SceneTransition.DoTransition(() =>
        {
            SceneManager.LoadScene("Main");
        });
    }

    public void Data_Click()
    {
        DataObject.gameObject.SetActive(true);
    }

    public void AllCards_Click()
    {
        Common.Instance.CardManager.GiveAllCards();
        SceneManager.LoadScene("StoryMode");
    }

    public void PacksPlus10_Click()
    {
        Common.Instance.SaveManager.SaveData.GameSaveData.PackCount += 10;
        SceneManager.LoadScene("StoryMode");
    }
}

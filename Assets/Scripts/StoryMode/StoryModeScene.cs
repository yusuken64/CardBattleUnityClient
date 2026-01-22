using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryModeScene : MonoBehaviour
{
    public Transform Container;
    public BattleGridButton BattleGridButtonPrefab;

    public BattlePreview BattlePreview;

    public List<StoryModeBattleDefinition> Datas;

    void Start()
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

    public void Back_Clicked()
    {
        Common.Instance.SceneTransition.DoTransition(() =>
        {
            SceneManager.LoadScene("Main");
        });
    }
}

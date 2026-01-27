using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ArenaPage : MonoBehaviour
{
	public GameObject NewAdventurePrompt;
	public VerticalDeckViewer DeckViewer;
	public DeckToggle DeckToggle;
	public GameObject Encouter;
	public AdventureCardPicker CardPicker;
	public GameObject BattlePrompt;

	public TextMeshProUGUI ArenaText;
	public TextMeshProUGUI BattleText;
	public GameObject BattleButton;

	public List<DeckDefinition> EnemyDecks;

	private void Start()
	{
		//TODO make a better indicator for "start new game"
		var newAdventure = Common.Instance.SaveManager.SaveData.GameSaveData.AdventureSaveData.CurrentDeck == null;

		if (newAdventure)
		{
			NewAdventurePrompt.gameObject.SetActive(true);
			Encouter.gameObject.SetActive(false);
		}
		else
		{
			var deck = Common.Instance.SaveManager.SaveData.GameSaveData.AdventureSaveData.CurrentDeck.ToDeck();

			DeckViewer.Setup(deck);
			SetToEncounter();
		}
	}

	public void StartNewAdventure_Clicked()
	{
		Deck deck = new();
		AdventureSaveData adventureSaveData = Common.Instance.SaveManager.SaveData.GameSaveData.AdventureSaveData;
		adventureSaveData.CurrentDeck = DeckSaveData.FromDeck(deck);
		adventureSaveData.PicksLeft = 30;
		adventureSaveData.Wins = 0;
		adventureSaveData.Lives = 1;
		DeckViewer.Setup(deck);
		SetToEncounter();
	}

	private void SetToEncounter()
	{
		DeckViewer.RemoveCardOnClick = false;
		NewAdventurePrompt.gameObject.SetActive(false);
		Encouter.gameObject.SetActive(true);

		AdventureSaveData adventureSaveData = Common.Instance.SaveManager.SaveData.GameSaveData.AdventureSaveData;
		if (adventureSaveData.PicksLeft > 0)
		{
			ArenaText.text = $"Pick A Card ({adventureSaveData.PicksLeft}/{30})";
			PromptToPickCard();
		}
		else
		{
			PromptToBattle();
		}
		DeckToggle.OpenDeck();
	}

	private void PromptToBattle()
	{
		Encouter.gameObject.SetActive(true);
		CardPicker.gameObject.SetActive(false);
		BattlePrompt.gameObject.SetActive(true);

		AdventureSaveData adventureSaveData = Common.Instance.SaveManager.SaveData.GameSaveData.AdventureSaveData;

		BattleText.text = $@"Battles: {adventureSaveData.Wins}/3
Lives: {adventureSaveData.Lives}";

		if (adventureSaveData.Wins == 3 ||
			adventureSaveData.Lives <= 0)
		{
			BattleButton.gameObject.SetActive(false);
		}
		else
		{
			BattleButton.gameObject.SetActive(true);
		}
	}

	public void EndArena_Clicked()
	{
		//reset arena
		Common.Instance.SaveManager.SaveData.GameSaveData.AdventureSaveData.CurrentDeck = null;
		Common.Instance.SaveManager.Save();

		Common.Instance.SceneTransition.DoTransition(() =>
		{
			SceneManager.LoadScene("Main");
		});
	}

	public void Battle_Clicked()
	{
		GameManager.GameResultRoutine = GameResult;

		IEnumerator GameResult(bool isWin)
		{
			AdventureSaveData adventureSaveData = Common.Instance.SaveManager.SaveData.GameSaveData.AdventureSaveData;

			if (!isWin)
			{
				adventureSaveData.Lives--;
				Common.Instance.SaveManager.Save();
				yield break;
			}
			else
			{
				adventureSaveData.Wins++;
				Common.Instance.SaveManager.Save();
				yield break;
			}
		}

		GameSaveData gameSaveData = Common.Instance.SaveManager.SaveData.GameSaveData;
		gameSaveData.AdventureSaveData.CurrentDeck = DeckSaveData.FromDeck(DeckViewer.GetDeck());
		GameManager.ReturnScreenName = "Arena";
		Common.Instance.SaveManager.Save();

		GameStartParams gameStartParams = new();
		gameStartParams.CombatDeck = gameSaveData.AdventureSaveData.CurrentDeck.ToDeck();
		gameStartParams.CombatDeckEnemy = GenerateEnemyDeck().ToDeck();
		GameManager.GameStartParams = gameStartParams;
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			SceneManager.LoadScene("GameScene");
		});

	}

	private DeckSaveData GenerateEnemyDeck()
	{
		var deck = EnemyDecks[UnityEngine.Random.Range(0, EnemyDecks.Count)];
		return deck.ToDeckData();
	}

	public void PromptToPickCard()
	{
		Encouter.gameObject.SetActive(true);
		BattlePrompt.gameObject.SetActive(false);
		CardPicker.gameObject.SetActive(true);

		var isHeroSet = DeckViewer.HeroCard == null;
		CardPicker.Setup(!isHeroSet);
		CardPicker.CardPickedCallback = (cardDefinition) =>
		{
			AdventureSaveData adventureSaveData = Common.Instance.SaveManager.SaveData.GameSaveData.AdventureSaveData;
			adventureSaveData.PicksLeft--;
			DeckViewer.AddCardToDeck(cardDefinition, false);
			CardPicker.gameObject.SetActive(false);
			SetToEncounter();
		};
	}

	public void GoBackToMain_Clicked()
	{
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			SceneManager.LoadScene("Main");
		});
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class StoryModeGenerator
{
    public static string StoryDefinitionFolder = "Assets/Prefab/StoryMode/Battles";

    [MenuItem("Data/Story/GenerateBattles")]
    public static void GenerateBattles()
    {
        bool proceed = EditorUtility.DisplayDialog(
            "Generate Battles",
            "This will generate Battles. Continue?",
            "Yes",
            "No"
        );

        if (proceed)
        {
            GenerateRandomBattles();
        }
    }

	private static void GenerateRandomBattles()
    {
        if (!AssetDatabase.IsValidFolder(StoryDefinitionFolder))
        {
            Directory.CreateDirectory(StoryDefinitionFolder);
            AssetDatabase.Refresh();
        }

        var cards = LoadAllCards()
            .Where(x => x.Collectable)
            .ToList();

        var heroPool = cards
            .OfType<MinionCardDefinition>()
            .ToList();

        for (int i = 0; i < 10; i++)
        {
            string battleFilePath = Path.Combine(StoryDefinitionFolder, $"Battle{i}.asset");
            string battleDeckFilePath = Path.Combine(StoryDefinitionFolder, $"Battle{i}_Deck.asset");
            var newBattleDefinition = EnsureObjectExists<StoryModeBattleDefinition>(battleFilePath);
            var newBattleDeckDefinition = EnsureObjectExists<DeckDefinition>(battleDeckFilePath);
            var hero = PickRandomWithReplacement(heroPool)[0];

            newBattleDefinition.LevelID = $"Level{i}";
            newBattleDefinition.BattleImage = hero.Sprite; //sprite of the deckleader
            newBattleDefinition.Description = ""; // this will be filled out manually
            newBattleDefinition.Deck = newBattleDeckDefinition;

            newBattleDefinition.Deck.Title = $"Level{i} Deck";
            newBattleDefinition.Deck.HeroCard = hero;
            newBattleDefinition.Deck.Cards = PickRandomWithReplacement(cards, 30);
            EditorUtility.SetDirty(newBattleDefinition);
            EditorUtility.SetDirty(newBattleDeckDefinition);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static List<CardDefinition> LoadAllCards()
    {
        var guids = AssetDatabase.FindAssets("t:CardDefinition");
        var cards = new List<CardDefinition>(guids.Length);

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var card = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
            if (card != null)
                cards.Add(card);
        }

        return cards;
    }

    private static List<T> PickRandomWithReplacement<T>(IList<T> list, int count = 1)
    {
        if (list == null || list.Count == 0)
            throw new ArgumentException("PickRandom called with null or empty list");

        var result = new List<T>(count);
        for (int i = 0; i < count; i++)
            result.Add(list[UnityEngine.Random.Range(0, list.Count)]);

        return result;
    }

    private static T EnsureObjectExists<T>(string filePath) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(filePath);
        if (existing != null)
            return existing;

        var newObject = ScriptableObject.CreateInstance<T>();

        AssetDatabase.CreateAsset(newObject, filePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return newObject;
    }
}
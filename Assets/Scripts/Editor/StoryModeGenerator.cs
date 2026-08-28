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

        var heroPool = PickRandomWithoutReplacement(cards.OfType<MinionCardDefinition>().ToList(), 12);

        for (int i = 0; i < 12; i++)
        {
            string battleFilePath = Path.Combine(StoryDefinitionFolder, $"Battle{i}.asset");
            string battleDeckFilePath = Path.Combine(StoryDefinitionFolder, $"Battle{i}_Deck.asset");
            var newBattleDefinition = EnsureObjectExists<StoryModeDungeonEncounterDefinition>(battleFilePath);
            var newBattleDeckDefinition = EnsureObjectExists<DeckDefinition>(battleDeckFilePath);
            var hero = heroPool[i];

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

    public static List<CardDefinition> LoadAllCards()
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

    public static List<T> PickRandomWithoutReplacement<T>(IList<T> list, int count)
    {
        if (list == null || list.Count == 0)
            throw new ArgumentException("PickRandom called with null or empty list");

        if (count < 0 || count > list.Count)
            throw new ArgumentOutOfRangeException(nameof(count));

        var copy = new List<T>(list);

        // Fisher-Yates shuffle
        for (int i = copy.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }

        return copy.GetRange(0, count);
    }

    public static List<T> PickRandomWithReplacement<T>(IList<T> list, int count = 1)
    {
        if (list == null || list.Count == 0)
            throw new ArgumentException("PickRandom called with null or empty list");

        var result = new List<T>(count);
        for (int i = 0; i < count; i++)
            result.Add(list[UnityEngine.Random.Range(0, list.Count)]);

        return result;
    }

    public static T EnsureObjectExists<T>(string filePath) where T : ScriptableObject
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
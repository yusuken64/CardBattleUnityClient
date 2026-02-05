using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GenerateRegionWindow : EditorWindow
{
    private const string KeyRegionName = "GenerateRegion_RegionName";
    private const string KeyOutputFolder = "GenerateRegion_OutputFolder";
    private const string KeyRegionSize = "GenerateRegion_RegionSize";
    private const string KeyDungeonSize = "GenerateRegion_DungeonSize";
    private const string KeyDeckCount = "GenerateRegion_DeckCount";

    private void OnEnable()
    {
        // Load values from EditorPrefs
        regionName = EditorPrefs.GetString(KeyRegionName, "NewRegion");
        outputFolder = EditorPrefs.GetString(KeyOutputFolder, "Assets/Regions");
        regionSize = EditorPrefs.GetInt(KeyRegionSize, 5);
        dungeonSize = EditorPrefs.GetInt(KeyDungeonSize, 5);
    }

    private void OnDisable()
    {
        // Save values to EditorPrefs
        EditorPrefs.SetString(KeyRegionName, regionName);
        EditorPrefs.SetString(KeyOutputFolder, outputFolder);
        EditorPrefs.SetInt(KeyRegionSize, regionSize);
        EditorPrefs.SetInt(KeyDungeonSize, dungeonSize);
    }

    private string regionName = "NewRegion";
    private string outputFolder = "Assets/Regions";

    private int regionSize = 5;
    private int dungeonSize = 5;

    [MenuItem("Data/Story/Generate Region")]
    public static void Open()
    {
        GetWindow<GenerateRegionWindow>("Generate Region");
    }

    private void OnGUI()
    {
        GUILayout.Label("Region Settings", EditorStyles.boldLabel);

        regionName = EditorGUILayout.TextField("Region Name", regionName);

        DrawFolderField();

        GUILayout.Space(10);

        GUILayout.Label("Region Generation Parameters", EditorStyles.boldLabel);

        regionSize = EditorGUILayout.IntSlider("Region Size", regionSize, 1, 10);
        dungeonSize = EditorGUILayout.IntSlider("Dungeon Size", dungeonSize, 1, 10);

        GUILayout.Space(20);

        using (new EditorGUI.DisabledScope(!IsValid()))
        {
            if (GUILayout.Button("Generate Region", GUILayout.Height(30)))
            {
                Generate();
            }

            if (GUILayout.Button("Generate Region Decks", GUILayout.Height(30)))
            {
                GenerateDecks();
            }
        }
    }

    private void DrawFolderField()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Output Folder", GUILayout.Width(EditorGUIUtility.labelWidth - 4));

        outputFolder = EditorGUILayout.TextField(outputFolder);

        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string selected = EditorUtility.OpenFolderPanel(
                "Select Output Folder",
                "Assets",
                ""
            );

            if (!string.IsNullOrEmpty(selected))
            {
                if (!selected.StartsWith(Application.dataPath))
                {
                    EditorUtility.DisplayDialog(
                        "Invalid Folder",
                        "Folder must be inside the Assets directory.",
                        "OK"
                    );
                }
                else
                {
                    outputFolder = "Assets" + selected.Substring(Application.dataPath.Length);
                }
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(regionName))
            return false;

        if (!AssetDatabase.IsValidFolder(outputFolder))
            return false;

        return true;
    }

    private void Generate()
    {
        Debug.Log($"Generating region '{regionName}'");
        Debug.Log($"Output: {outputFolder}");

        string regionFolderPath = $"{outputFolder}/{regionName}";
        string regionDecksFolderPath = $"{outputFolder}/{regionName}/Deck";

        // Create folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(regionFolderPath))
        {
            AssetDatabase.CreateFolder(outputFolder, regionName);
        }

        if (!AssetDatabase.IsValidFolder(regionDecksFolderPath))
        {
            AssetDatabase.CreateFolder(regionFolderPath, "Deck");
        }

        string safeRegionName = string.Concat(regionName.Split(System.IO.Path.GetInvalidFileNameChars()));
        var mapRegion = EnsureScriptableObjectExists<MapRegionDefinition>($"{regionFolderPath}/{safeRegionName}_MapRegionDefinition.asset");
        mapRegion.RegionID = safeRegionName;
        mapRegion.Name = regionName;

        mapRegion.Dungeons = new System.Collections.Generic.List<StoryModeDungeonDefinition>();
        for (int i = 0; i < regionSize; i++)
        {
            var dungeon = EnsureScriptableObjectExists<StoryModeDungeonDefinition>($"{regionFolderPath}/{safeRegionName}_StoryModeDungeonDefinition_{i}.asset");
            dungeon.DungeonID = $"{mapRegion.RegionID}_Dungeon{i}";
            dungeon.DungeonName = $"{mapRegion.RegionID} Dungeon lv.{i + 1}";
            dungeon.MaxWins = 3 + (i * 2);
            dungeon.StoryModeDungeonEncounterDefinitions = new System.Collections.Generic.List<StoryModeDungeonEncounterDefinition>();

            for (int j = 0; j < dungeonSize; j++)
            {
                StoryModeDungeonEncounterDefinition storyModeDungeonEncounterDefinition =
                    EnsureScriptableObjectExists<StoryModeDungeonEncounterDefinition>($"{regionFolderPath}/{safeRegionName}_StoryModeDungeonEncounterDefinition_{i}_{j}.asset");
                dungeon.StoryModeDungeonEncounterDefinitions.Add(storyModeDungeonEncounterDefinition);

                storyModeDungeonEncounterDefinition.LevelID = $"{regionName}_StoryModeDungeonEncounterDefinition_{i}";
                DeckDefinition encounterDeck =
                    EnsureScriptableObjectExists<DeckDefinition>($"{regionDecksFolderPath}/{safeRegionName}_StoryModeDungeonEncounterDeckDefinition_{j}.asset");
                storyModeDungeonEncounterDefinition.Deck = encounterDeck;
                storyModeDungeonEncounterDefinition.Health = 30 + (i * 15);
            }

            mapRegion.Dungeons.Add(dungeon);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void GenerateDecks()
    {
        Debug.Log($"Generating region '{regionName}'");
        Debug.Log($"Output: {outputFolder}");

        string regionFolderPath = $"{outputFolder}/{regionName}";
        string regionDecksFolderPath = $"{outputFolder}/{regionName}/Deck";

        // Create folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder(regionFolderPath))
        {
            AssetDatabase.CreateFolder(outputFolder, regionName);
        }

        if (!AssetDatabase.IsValidFolder(regionDecksFolderPath))
        {
            AssetDatabase.CreateFolder(regionFolderPath, "Deck");
        }

        string safeRegionName = string.Concat(regionName.Split(System.IO.Path.GetInvalidFileNameChars()));

        var cards = StoryModeGenerator.LoadAllCards()
            .Where(x => x.Collectable)
            .ToList();

        var heroPool = StoryModeGenerator.
            PickRandomWithoutReplacement(cards.OfType<MinionCardDefinition>().ToList(), dungeonSize);

        for (int i = 0; i < dungeonSize; i++)
        {
            DeckDefinition encounterDeck =
                EnsureScriptableObjectExists<DeckDefinition>($"{regionDecksFolderPath}/{safeRegionName}_StoryModeDungeonEncounterDeckDefinition_{i}.asset");

            encounterDeck.Title = $"Deck{i}";
            encounterDeck.HeroCard = heroPool[i];
            encounterDeck.Cards = StoryModeGenerator.PickRandomWithReplacement(cards, 50);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static T EnsureScriptableObjectExists<T>(string filePath) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(filePath);
        if (existing != null)
            return existing;

        var newScriptableObject = ScriptableObject.CreateInstance<T>();

        AssetDatabase.CreateAsset(newScriptableObject, filePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return newScriptableObject;
    }
}

using System.IO;
using UnityEditor;
using UnityEngine;

public static class MinionGenerator
{
    public static string MinionArtPath = "Assets/Images/Characters";
    public static string MinionDefinitionFolder = "Assets/Prefab/CardDefinitions";

    [MenuItem("Data/Cards/GenerateMinions")]
    public static void GenerateMinions()
    {
        bool proceed = EditorUtility.DisplayDialog(
            "Generate Minions",
            "This will generate Minions for every Character Image. Continue?",
            "Yes",
            "No"
        );

        if (proceed)
        {
            GenerateMinionsFromArt();
        }
    }

    private static void GenerateMinionsFromArt()
    {
        if (!AssetDatabase.IsValidFolder(MinionDefinitionFolder))
        {
            Directory.CreateDirectory(MinionDefinitionFolder);
            AssetDatabase.Refresh();
        }

        // Find all jpg files in the folder recursively
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { MinionArtPath });

        foreach (string guid in guids)
        {
            string imageAssetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetExtension(imageAssetPath).ToLower() != ".jpg")
                continue; // skip non-jpg textures

            string fileName = Path.GetFileNameWithoutExtension(imageAssetPath);
            string fileID = fileName
                .Replace(" ", "_")
                .ToLower();
            string filePath = Path.Combine(MinionDefinitionFolder, fileID + ".asset");

            var minion = EnsureMinionCardExists(filePath);

            minion.name = fileID;
            minion.CardName = fileName;
            minion.ID = fileID;
            minion.Sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imageAssetPath);
        }

        EditorUtility.DisplayDialog("Done", "Minion generation completed!", "OK");
    }

    private static MinionCardDefinition EnsureMinionCardExists(string filePath)
    {
        var existing = AssetDatabase.LoadAssetAtPath<MinionCardDefinition>(filePath);
        if (existing != null)
            return existing;

        var newCard = ScriptableObject.CreateInstance<MinionCardDefinition>();

        AssetDatabase.CreateAsset(newCard, filePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return newCard;
    }
}

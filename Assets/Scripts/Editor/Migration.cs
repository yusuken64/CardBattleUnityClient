using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class MinionMigration
{
    public static string MinionDefinitionFolder = "Assets/Prefab/CardDefinitions";
    public static string TestMinionDefinitionFolder = "Assets/Prefab/TestCards";

    [MenuItem("Data/Cards/MigrateTriggeredEffectsToMinionEffects")]
    public static void MigrateTriggeredEffectsToMinionEffects()
    {
        // Find all assets of type MinionCardDefinition in the target folder
        string[] guids = AssetDatabase.FindAssets("t:MinionCardDefinition", new[] { TestMinionDefinitionFolder });

        int modifiedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var minion = AssetDatabase.LoadAssetAtPath<MinionCardDefinition>(path);

            if (minion == null)
                continue;

            // Skip if already migrated
            if (minion.TriggeredEffects == null || minion.TriggeredEffects.Count == 0)
                continue;

            // Copy effects over
            minion.MinionTriggeredEffects = new List<TriggeredEffectWrapper>(minion.TriggeredEffects);

            minion.TriggeredEffects.Clear();

            EditorUtility.SetDirty(minion);
            modifiedCount++;

            Debug.Log($"Migrated effects on: {path}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Completed migration. Modified {modifiedCount} MinionCardDefinition assets.");
    }

    [MenuItem("Data/Cards/EnsureMinionTriggeredEffects")]
    public static void EnsureMinionTriggeredEffects()
    {
        // Find all assets of type MinionCardDefinition in the target folder
        string[] guids = AssetDatabase.FindAssets("t:MinionCardDefinition", new[] 
        {
            MinionDefinitionFolder,
            TestMinionDefinitionFolder,
        });

        int modifiedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var minion = AssetDatabase.LoadAssetAtPath<MinionCardDefinition>(path);

            if (minion == null)
                continue;

            // Copy effects over
            if (minion.MinionTriggeredEffects == null)
            {
                minion.MinionTriggeredEffects = new List<TriggeredEffectWrapper>();
            }

            EditorUtility.SetDirty(minion);
            modifiedCount++;

            Debug.Log($"Migrated effects on: {path}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Completed migration. Modified {modifiedCount} MinionCardDefinition assets.");

    }
}

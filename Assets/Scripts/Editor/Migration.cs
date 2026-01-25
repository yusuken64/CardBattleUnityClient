using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MinionMigration
{
    public static string MinionDefinitionFolder = "Assets/Prefab/CardDefinitions";
    public static string TestMinionDefinitionFolder = "Assets/Prefab/TestCards";

    [MenuItem("Data/Cards/MigrateTriggeredEffectsToMinionEffects")]
    public static void MigrateTriggeredEffectsToMinionEffects()
    {
        //// Find all assets of type MinionCardDefinition in the target folder
        //string[] guids = AssetDatabase.FindAssets("t:MinionCardDefinition", new[] { TestMinionDefinitionFolder });

        //int modifiedCount = 0;

        //foreach (string guid in guids)
        //{
        //    string path = AssetDatabase.GUIDToAssetPath(guid);
        //    var minion = AssetDatabase.LoadAssetAtPath<MinionCardDefinition>(path);

        //    if (minion == null)
        //        continue;

        //    // Skip if already migrated
        //    if (minion.TriggeredEffects == null || minion.TriggeredEffects.Count == 0)
        //        continue;

        //    // Copy effects over
        //    minion.MinionTriggeredEffects = new List<TriggeredEffectWrapper>(minion.TriggeredEffects);

        //    minion.TriggeredEffects.Clear();

        //    EditorUtility.SetDirty(minion);
        //    modifiedCount++;

        //    Debug.Log($"Migrated effects on: {path}");
        //}

        //AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();

        //Debug.Log($"Completed migration. Modified {modifiedCount} MinionCardDefinition assets.");
    }

    [MenuItem("Data/Cards/EnsureMinionTriggeredEffects")]
    public static void EnsureMinionTriggeredEffects()
    {
        //// Find all assets of type MinionCardDefinition in the target folder
        //string[] guids = AssetDatabase.FindAssets("t:MinionCardDefinition", new[]
        //{
        //    MinionDefinitionFolder,
        //    TestMinionDefinitionFolder,
        //});

        //int modifiedCount = 0;

        //foreach (string guid in guids)
        //{
        //    string path = AssetDatabase.GUIDToAssetPath(guid);
        //    var minion = AssetDatabase.LoadAssetAtPath<MinionCardDefinition>(path);

        //    if (minion == null)
        //        continue;

        //    // Copy effects over
        //    if (minion.MinionTriggeredEffects == null)
        //    {
        //        minion.MinionTriggeredEffects = new List<TriggeredEffectWrapper>();
        //    }

        //    EditorUtility.SetDirty(minion);
        //    modifiedCount++;

        //    Debug.Log($"Migrated effects on: {path}");
        //}

        //AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();

        //Debug.Log($"Completed migration. Modified {modifiedCount} MinionCardDefinition assets.");

    }

    [MenuItem("Data/Cards/MigrateTargetSelector")]
    public static void MigrateTargetSelector()
    {
        // Find all assets of type CardDefinition in the project
        string[] guids = AssetDatabase.FindAssets("t:CardDefinition");

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var card = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);

            CardBattleEngine.TargetingType? originalTargetType = null;
            if (card is MinionCardDefinition minionCardDefinition)
			{
                var effect = minionCardDefinition.MinionTriggeredEffects.FirstOrDefault();
				originalTargetType = effect?.TargetType;
			}
            else if(card is SpellCardDefinition spellCardDefinition)
			{
                originalTargetType = spellCardDefinition.TargetingType;
			}
            else if (card is WeaponCardDefinition weaponCardDefinition)
			{
                var effect = weaponCardDefinition.TriggeredEffects.FirstOrDefault();
                originalTargetType = effect?.TargetType;
			}

            IValidTargetSelectorWrapperBase targetSelector = null;
            switch (originalTargetType)
			{
				case CardBattleEngine.TargetingType.Any:
                    targetSelector = new EntityTypeSelectorWrapper()
                    {
                        EntityTypes = CardBattleEngine.EntityType.Player |
                                      CardBattleEngine.EntityType.Minion,
                        TeamRelationship = CardBattleEngine.TeamRelationship.Any
                    };
                    break;
				case CardBattleEngine.TargetingType.FriendlyMinion:
                    targetSelector = new EntityTypeSelectorWrapper()
                    {
                        EntityTypes = CardBattleEngine.EntityType.Minion,
                        TeamRelationship = CardBattleEngine.TeamRelationship.Friendly
                    };
                    break;
				case CardBattleEngine.TargetingType.FriendlyHero:
                    targetSelector = new EntityTypeSelectorWrapper()
                    {
                        EntityTypes = CardBattleEngine.EntityType.Player,
                        TeamRelationship = CardBattleEngine.TeamRelationship.Friendly
                    };
                    break;
				case CardBattleEngine.TargetingType.EnemyMinion:
                    targetSelector = new EntityTypeSelectorWrapper()
                    {
                        EntityTypes = CardBattleEngine.EntityType.Minion,
                        TeamRelationship = CardBattleEngine.TeamRelationship.Enemy
                    };
                    break;
				case CardBattleEngine.TargetingType.EnemyHero:
                    targetSelector = new EntityTypeSelectorWrapper()
                    {
                        EntityTypes = CardBattleEngine.EntityType.Player,
                        TeamRelationship = CardBattleEngine.TeamRelationship.Enemy
                    };
                    break;
				case CardBattleEngine.TargetingType.AnyEnemy:
                    targetSelector = new EntityTypeSelectorWrapper()
                    {
                        EntityTypes = CardBattleEngine.EntityType.Player |
                                      CardBattleEngine.EntityType.Minion,
                        TeamRelationship = CardBattleEngine.TeamRelationship.Enemy
                    };
                    break;
				case CardBattleEngine.TargetingType.Self:
                    targetSelector = new EntityTypeSelectorWrapper()
                    {
                        EntityTypes = CardBattleEngine.EntityType.Player,
                        TeamRelationship = CardBattleEngine.TeamRelationship.Friendly
                    };
                    break;
				case CardBattleEngine.TargetingType.None:
                    targetSelector = null;
                    break;
				case CardBattleEngine.TargetingType.AnyMinion:
                    targetSelector = new EntityTypeSelectorWrapper()
                    {
                        EntityTypes = CardBattleEngine.EntityType.Minion,
                        TeamRelationship = CardBattleEngine.TeamRelationship.Any
                    };
                    break;
			}

			card.ValidTargetSelector = targetSelector;
            EditorUtility.SetDirty(card);
        }
        AssetDatabase.SaveAssets();
    }
}

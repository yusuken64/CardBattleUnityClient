using System;
using CardBattleEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public static class NetworkCardExportValidation
{
    [MenuItem("Game/Network/Validate Card Export")]
    public static void Validate()
    {
        var definition = ScriptableObject.CreateInstance<SpellCardDefinition>();
        var sfx = ScriptableObject.CreateInstance<ProjectileSFX>();
        var muzzle = new GameObject("Network export validation");
        try
        {
            definition.ID = "network-export-test";
            definition.CardName = "Network Export Test";
            sfx.MuzzleObject = muzzle;
            var damage = new DamageAction { Damage = (Value)3, CustomSFX = sfx };
            var card = new SpellCard(definition.CardName, 2) { CustomSFX = sfx };
            var effect = new SpellCastEffect();
            effect.GameActions.Add(damage);
            card.SpellCastEffects.Add(effect);
            var json = definition.ToWireDefinitionJson(card);
            var loaded = CardDatabase.LoadCardFromJson(json) as CardBattleEngine.SpellCardDefinition;
            if (loaded == null || loaded.Id != definition.ID || loaded.Name != definition.CardName ||
                loaded.Cost != 2 || loaded.SpellCastEffects.Count != 1 ||
                loaded.SpellCastEffects[0].GameActions[0] is not DamageAction restored ||
                restored.Damage is not ConstantValue amount || amount.Number != 3 ||
                json.Contains("CustomSFX") || json.Contains("UnityEngine") ||
                !ReferenceEquals(damage.CustomSFX, sfx) || !ReferenceEquals(card.CustomSFX, sfx))
                throw new Exception("Network card export regression failed.");

            int count = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:CardDefinition"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset is not MinionCardDefinition && asset is not SpellCardDefinition && asset is not WeaponCardDefinition)
                    continue;
                var runtime = asset.CreateCard();
                var exported = asset.ToWireDefinitionJson(runtime);
                var roundTrip = CardDatabase.LoadCardFromJson(exported);
                if (roundTrip == null || roundTrip.Id != asset.ID || exported.Contains("CustomSFX"))
                    throw new Exception($"Network export failed for {AssetDatabase.GetAssetPath(asset)}");
                if (runtime is MinionCard minion)
                {
                    var effects = JObject.Parse(exported)["MinionTriggeredEffects"]
                        .ToObject<System.Collections.Generic.List<TriggeredEffect>>(
                            JsonSerializer.Create(new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }));
                    if (effects.Count != minion.MinionTriggeredEffects.Count)
                        throw new Exception($"Minion effects lost in export: {asset.ID}");
                    for (int i = 0; i < effects.Count; i++)
                    {
                        if (effects[i].EffectTrigger != minion.MinionTriggeredEffects[i].EffectTrigger ||
                            effects[i].GameActions.Count != minion.MinionTriggeredEffects[i].GameActions.Count)
                            throw new Exception($"Minion effect changed in export: {asset.ID}");
                    }
                }
                count++;
            }
            Debug.Log($"Network card export validation passed: projectile spell regression and {count} card assets.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(muzzle);
            UnityEngine.Object.DestroyImmediate(sfx);
            UnityEngine.Object.DestroyImmediate(definition);
        }
    }
}

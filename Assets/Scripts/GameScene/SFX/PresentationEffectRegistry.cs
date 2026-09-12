using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PresentationEffectRegistry : ScriptableObject
{
    [Serializable] public struct Entry { public string Id; public CustomSFX Effect; }
    public List<Entry> Entries = new();
    private static Dictionary<string, CustomSFX> _effects;
    public static string IdFor(CustomSFX effect)
    {
        var registry = Resources.Load<PresentationEffectRegistry>("PresentationEffectRegistry");
        if (registry != null)
            foreach (var entry in registry.Entries) if (entry.Effect == effect) return entry.Id;
        return null;
    }
    public static CustomSFX Resolve(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (_effects == null)
        {
            _effects = new();
            var registry = Resources.Load<PresentationEffectRegistry>("PresentationEffectRegistry");
            if (registry != null)
                foreach (var entry in registry.Entries) _effects[entry.Id] = entry.Effect;
        }
        if (_effects.TryGetValue(id, out var effect)) return effect;
        Debug.LogWarning($"Unknown presentation effect {id}; using the action's default animation.");
        return null;
    }
}

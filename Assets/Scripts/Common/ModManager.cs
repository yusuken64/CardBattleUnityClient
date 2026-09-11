using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ModManager : MonoBehaviour
{
    internal static readonly HashSet<string> KnownCardTypes =
        new(StringComparer.OrdinalIgnoreCase) { "minion", "weapon", "spell" };

    private static readonly string[] SupportedImageExtensions = { ".jpg", ".jpeg", ".png" };

    private HashSet<ModData> mods = new HashSet<ModData>();

    public void DiscoverMods()
    {
        mods.Clear();

        string exeDir = Directory.GetParent(Application.dataPath).FullName;
        string modsDir = Path.Combine(exeDir, "Mods");

        if (!Directory.Exists(modsDir))
            Directory.CreateDirectory(modsDir);

        ModSaveData modSaveData = Common.Instance?.SaveManager?.SaveData?.ModSaveData ?? new ModSaveData();
        foreach (var dir in Directory.GetDirectories(modsDir))
        {
            var fileName = Path.GetFileName(dir);
            ModData mod = new ModData
            {
                modName = fileName,
                folderPath = dir,
                enabled = modSaveData.EnabledMods.Contains(fileName),
            };
            mods.Add(mod);

            if (mod.enabled)
            {
                LoadMod(mod);
            }
            else
            {
                ScanMod(mod);
            }
        }
    }

	internal List<ModData> GetAllMods() =>
		mods.OrderBy(m => m.modName, StringComparer.OrdinalIgnoreCase).ToList();

	private void LoadMod(ModData mod) => LoadModCardData(mod, loadImages: true);

	private void ScanMod(ModData mod) => LoadModCardData(mod, loadImages: false);

	private void LoadModCardData(ModData mod, bool loadImages)
    {
        if (mod.loaded) { return; }
        mod.cards.Clear();
        mod.cachedDefinitions = null;

        var jsonFiles = Directory.GetFiles(mod.folderPath, "*.json", SearchOption.TopDirectoryOnly);

        foreach (var file in jsonFiles)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                string json = File.ReadAllText(file);
                var def = JsonUtility.FromJson<CardData>(json);

                if (!IsValidCardData(def))
                {
                    Debug.LogWarning($"ModManager: Skipping '{file}' — missing name or unrecognized cardType '{def?.cardType}'.");
                    continue;
                }

                def.id = $"{mod.modName}::{fileName}";

                if (loadImages)
                {
                    Texture2D tex = LoadCardTexture(Path.GetDirectoryName(file), fileName);
                    def.loadedSprite = ToSprite(tex);
                }

                mod.cards.Add(def);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to load {file}: {e.Message}");
            }
        }
        mod.loaded = true;
    }

    private static bool IsValidCardData(CardData data)
    {
        return data != null
            && !string.IsNullOrWhiteSpace(data.name)
            && !string.IsNullOrWhiteSpace(data.cardType)
            && KnownCardTypes.Contains(data.cardType.Trim());
    }

    Texture2D LoadCardTexture(string directory, string fileNameNoExt)
    {
        foreach (var ext in SupportedImageExtensions)
        {
            string candidate = Path.Combine(directory, fileNameNoExt + ext);
            if (File.Exists(candidate))
                return LoadTexture(candidate);
        }
        return null;
    }

    Texture2D LoadTexture(string path)
    {
        if (!File.Exists(path))
            return null;

        byte[] data = File.ReadAllBytes(path);

        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(data); // auto-detects JPG/PNG
        return tex;
    }

    Sprite ToSprite(Texture2D tex)
    {
        if (tex == null) return null;

        return Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    public List<CardDefinition> GetAllEnabledCardDefinitions()
    {
        List<CardDefinition> cards = new List<CardDefinition>();

        foreach (var mod in mods.Where(x => x.enabled))
        {
            if (mod.cachedDefinitions == null)
            {
                LoadMod(mod);
                mod.cachedDefinitions = mod.cards
                    .Select(AsCardDefinition)
                    .Where(c => c != null)
                    .ToList();
            }

            cards.AddRange(mod.cachedDefinitions);
        }

        return cards;
    }

    public static CardDefinition AsCardDefinition(CardData card)
    {
        switch (card.cardType?.Trim().ToLowerInvariant())
        {
            case "weapon":
                {
                    var weapon = ScriptableObject.CreateInstance<WeaponCardDefinition>();
                    weapon.ID = card.id;
                    weapon.CardName = card.name;
                    weapon.WeaponName = card.name;
                    weapon.DescriptionOverride = card.description;
                    weapon.Cost = card.cost;
                    weapon.Attack = card.attack;
                    weapon.Durability = card.health;
                    weapon.Sprite = card.loadedSprite;
                    weapon.Collectable = true;
                    return weapon;
                }

            case "spell":
                {
                    var spell = ScriptableObject.CreateInstance<SpellCardDefinition>();
                    spell.ID = card.id;
                    spell.CardName = card.name;
                    spell.DescriptionOverride = card.description;
                    spell.Cost = card.cost;
                    spell.Sprite = card.loadedSprite;
                    spell.Collectable = true;
                    return spell;
                }

            case "minion":
                {
                    var minion = ScriptableObject.CreateInstance<MinionCardDefinition>();
                    minion.ID = card.id;
                    minion.CardName = card.name;
                    minion.DescriptionOverride = card.description;
                    minion.Cost = card.cost;
                    minion.Attack = card.attack;
                    minion.Health = card.health;
                    minion.Sprite = card.loadedSprite;
                    minion.Collectable = true;
                    return minion;
                }

            default:
                Debug.LogWarning($"ModManager: Unrecognized cardType '{card.cardType}' for card id '{card.id}'. Skipping.");
                return null;
        }
    }
}

public class ModData
{
	public string modName;
    public string folderPath;
    public bool enabled;

    public List<CardData> cards = new List<CardData>();
	public List<CardDefinition> cachedDefinitions;
	public bool loaded;
}

public class CardData
{
    public string id;
    public string cardType;
	public string name;
	public string description;
	public int cost;
	public int attack;
	public int health;
	public Sprite loadedSprite;
}
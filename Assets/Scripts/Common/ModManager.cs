using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ModManager : MonoBehaviour
{
    private HashSet<ModData> mods = new HashSet<ModData>();

	private void Awake()
	{
        DiscoverMods();
	}

    public void DiscoverMods()
    {
        mods.Clear();

        string exeDir = Directory.GetParent(Application.dataPath).FullName;
        string modsDir = Path.Combine(exeDir, "Mods");

        if (!Directory.Exists(modsDir))
            Directory.CreateDirectory(modsDir);

        ModSaveData modSaveData = Common.Instance.SaveManager.SaveData.ModSaveData;
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
        }
    }

	internal HashSet<ModData> GetAllMods() => mods;

	private void LoadMod(ModData mod)
    {
        if (mod.loaded) { return; }
        mod.cards.Clear();

        var jsonFiles = Directory.GetFiles(mod.folderPath, "*.json", SearchOption.AllDirectories);

        foreach (var file in jsonFiles)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                string json = File.ReadAllText(file);
                var def = JsonUtility.FromJson<CardData>(json);
                def.id = fileName;

                string imagePath = Path.Combine(
                    Path.GetDirectoryName(file),
                    fileName + ".jpg");
                Texture2D tex = LoadTexture(imagePath);
                Sprite loadedSprite = ToSprite(tex);
                def.loadedSprite = loadedSprite;

                mod.cards.Add(def);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to load {file}: {e.Message}");
            }
        }
        mod.loaded = true;
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
            LoadMod(mod);

            foreach (var card in mod.cards)
			{
				CardDefinition newCardDefinition = AsCardDefinition(card);
				cards.Add(newCardDefinition);
			}
		}

        return cards;
    }

    public static CardDefinition AsCardDefinition(CardData card)
    {
        if (card.cardType == "weapon")
        {
            return new WeaponCardDefinition()
            {
                ID = card.id,
                CardName = card.name,
                WeaponName = card.name,
                //Description = card.description;
                Cost = card.cost,
                Attack = card.attack,
                Durability = card.health,
                Sprite = card.loadedSprite,
                Collectable = true,
            };
        }

        //fallback to minion
        return new MinionCardDefinition()
        {
            ID = card.id,
            CardName = card.name,
            //Description = card.description;
            Cost = card.cost,
            Attack = card.attack,
            Health = card.health,
            Sprite = card.loadedSprite,
            Collectable = true,
        };
    }
}

public class ModData
{
	public string modName;
    public string folderPath;
    public bool enabled;

    public List<CardData> cards = new List<CardData>();
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
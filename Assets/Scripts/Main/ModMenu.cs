using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class ModMenu : MonoBehaviour
{
    public GameObject ModItemPrefab;
    public Transform ModContainer;

    public Texture2D DefaultImage;
    public Texture2D WeaponImage;
    private List<ModItem> modItems = new();

    public ModCardPreviews ModCardPreviews;

    public void Setup()
    {
        ModCardPreviews.gameObject.SetActive(false);
        ModManager modManager = Common.Instance.ModManager;
        modManager.DiscoverMods();
        var modsData = modManager.GetAllMods();

        foreach (Transform child in ModContainer)
        {
            Destroy(child.gameObject);
        }
        modItems.Clear();


        foreach (var modData in modsData)
        {
            GameObject item = Instantiate(ModItemPrefab, ModContainer);

            // Pass data into the prefab
            ModItem modItem = item.GetComponent<ModItem>();
            if (modItem != null)
            {
                modItem.Setup(modData);
                modItem.PreviewCallBack = (x) =>
                {
                    ModCardPreviews.gameObject.SetActive(true);
                    ModCardPreviews.Setup(x.ModData.cards.Select(ModManager.AsCardDefinition).ToList());
                };
                modItems.Add(modItem);
            }
            else
            {
                Debug.LogWarning("ModItem prefab is missing ModItem script.");
            }
        }
    }

    public void CreateDefault()
    {
        // Path to exe directory
        string exeDir = Directory.GetParent(Application.dataPath).FullName;

        // Mods/default folder
        string modsDir = Path.Combine(exeDir, "Mods");
        string defaultDir = Path.Combine(modsDir, "default");

        // Ensure directories exist
        Directory.CreateDirectory(defaultDir);

        // File paths
        string readmePath = Path.Combine(defaultDir, "README.txt");

        if (!File.Exists(readmePath))
        {
            string readmeContent = @"SlayQueen Gatekeeper mod format
Each mod is contained in Mod/{ModName}
The folder is used as the modname
Each card is defined in {cardName}.json
and the Image should be {cardName}.jpg
";
            File.WriteAllText(readmePath, readmeContent);
        }

        string fileNameMinion = "customminion";
        CardData data = new CardData()
        {
            cardType = "minion",
            name = "Custom Minion",
            description = "test description",
            cost = 6,
            attack = 6,
            health = 7,
        };
        CreateFiles(defaultDir, fileNameMinion, data, DefaultImage);

        string fileNameWeapon = "customweapon";
        CardData weaponData = new CardData()
        {
            cardType = "weapon",
            name = "Custom Weapon",
            description = "test description",
            cost = 4,
            attack = 2,
            health = 2,
        };
        CreateFiles(defaultDir, fileNameWeapon, weaponData, WeaponImage);

        if (Common.Instance.SaveManager.SaveData.ModSaveData.EnabledMods.Remove(defaultDir))
        {
            Common.Instance.SaveManager.Save();
        }
        Debug.Log("Default mod created at: " + defaultDir);
        Setup();
    }

    private void CreateFiles(
        string defaultDir,
        string fileNameMinion,
        CardData data,
        Texture2D texture)
    {
        string jsonPath = Path.Combine(defaultDir, fileNameMinion + ".json");
        string jpgPath = Path.Combine(defaultDir, fileNameMinion + ".jpg");

        // Write JSON file (only if it doesn't exist)
        if (!File.Exists(jsonPath))
        {
            string jsonContent = JsonUtility.ToJson(data);
            File.WriteAllText(jsonPath, jsonContent);
        }

        if (!File.Exists(jpgPath))
        {
            byte[] jpgBytes = texture.EncodeToJPG();
            File.WriteAllBytes(jpgPath, jpgBytes);
        }
    }

    public void OpenFolder_Clicked()
    {
        string exeDir = Directory.GetParent(Application.dataPath).FullName;
        string modsDir = Path.Combine(exeDir, "Mods");

        if (!System.IO.Directory.Exists(modsDir))
        {
            return;
        }

        Process.Start(new ProcessStartInfo()
        {
            FileName = "explorer.exe",
            Arguments = $"\"{modsDir}\"",
            UseShellExecute = true
        });
    }

    public void Cancel_Clicked()
    {
        this.gameObject.SetActive(false);
    }

    public void Apply_Clicked()
    {
        Common.Instance.YesNoConfirmation.Setup("Apply Mod Changes?",
            "This could break in progress Campaigns",
            "Apply",
            () =>
            {
                //Common.Instance.SaveManager.ResetData();
                //Common.Instance.SaveManager.EnsureData();
                var activeMods = modItems.Where(x => x.IsModEnabled())
                    .Select(x => x.ModData.modName).ToList();
                Common.Instance.SaveManager.SaveData.ModSaveData.EnabledMods = activeMods;
                Common.Instance.SaveManager.Save();
                Common.Instance.CardManager.ReloadCards();

                SceneManager.LoadScene("Main");
            },
            "Cancel",
            () => { this.gameObject.SetActive(false); });
    }
}

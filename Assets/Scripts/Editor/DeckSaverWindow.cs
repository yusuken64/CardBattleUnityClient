using System.Linq;
using UnityEditor;
using UnityEngine;

public class DeckSaver : EditorWindow
{
    private const string LastDeckSavePathKey = "DeckSaver_LastSavePath";
    private const string LastDeckLoadPathKey = "DeckSaver_LastLoadPath";

    [MenuItem("Data/DeckSaver/Deck Dialog")]
    public static void ShowWindow()
    {
        GetWindow<DeckSaver>("DeckSaver");
    }

    [MenuItem("Data/DeckSaver/Give All Cards")]
    public static void GiveAllCards()
    {
        Common.Instance.CardManager.GiveAllCards();

        FindFirstObjectByType<Collection>().SetToPage(0);
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        if (GUILayout.Button("Save Current Deck", GUILayout.Height(40)))
        {
            SaveDeckToFile();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Load Deck From Asset", GUILayout.Height(40)))
        {
            LoadDeckFromFile();
        }
    }


    public static void SaveDeckToFile()
    {
        var deckViewer = FindFirstObjectByType<VerticalDeckViewer>();
        if (deckViewer == null)
        {
            Debug.LogError("No VerticalDeckViewer found in scene.");
            return;
        }

        var cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager == null)
        {
            Debug.LogError("No CardManager found in scene.");
            return;
        }

        var deck = deckViewer.GetDeck();

        // Load last used path (default to Assets/)
        string lastPath = EditorPrefs.GetString(LastDeckSavePathKey, "Assets");

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Deck Definition",
            deck.Title,
            "asset",
            "Choose where to save the deck asset",
            lastPath
        );

        if (string.IsNullOrEmpty(path))
            return;

        // Remember folder for next time
        string folder = System.IO.Path.GetDirectoryName(path);
        EditorPrefs.SetString(LastDeckSavePathKey, folder);

        // Create ScriptableObject
        DeckDefinition deckDefinition = AssetDatabase.LoadAssetAtPath<DeckDefinition>(path);

        if (deckDefinition == null)
        {
            // Create new asset
            deckDefinition = ScriptableObject.CreateInstance<DeckDefinition>();
            AssetDatabase.CreateAsset(deckDefinition, path);
        }

        deckDefinition.Title = deck.Title;
        deckDefinition.HeroCard = cardManager.GetCardByID(deck.HeroCard.ID);
        deckDefinition.Cards = deck.Cards
            .Select(x => cardManager.GetCardByID(x.ID))
            .ToList();

        AssetDatabase.CreateAsset(deckDefinition, path);
        EditorUtility.SetDirty(deckDefinition);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Deck saved to {path}");
    }

    public static void LoadDeckFromFile()
    {
        var deckViewer = FindFirstObjectByType<VerticalDeckViewer>();
        if (deckViewer == null)
        {
            Debug.LogError("No VerticalDeckViewer found in scene.");
            return;
        }

        var cardManager = FindFirstObjectByType<CardManager>();
        if (cardManager == null)
        {
            Debug.LogError("No CardManager found in scene.");
            return;
        }

        // Load last used folder
        string lastPath = EditorPrefs.GetString(LastDeckLoadPathKey, "Assets");

        string path = EditorUtility.OpenFilePanel(
            "Load Deck Definition",
            lastPath,
            "asset"
        );

        if (string.IsNullOrEmpty(path))
            return;

        // Convert absolute path -> Unity relative path
        if (path.StartsWith(Application.dataPath))
            path = "Assets" + path.Substring(Application.dataPath.Length);
        else
        {
            Debug.LogError("Deck must be inside the project Assets folder.");
            return;
        }

        // Remember folder
        string folder = System.IO.Path.GetDirectoryName(path);
        EditorPrefs.SetString(LastDeckLoadPathKey, folder);

        // Load asset
        DeckDefinition deckDefinition = AssetDatabase.LoadAssetAtPath<DeckDefinition>(path);
        if (deckDefinition == null)
        {
            Debug.LogError("Failed to load DeckDefinition.");
            return;
        }

        // Convert DeckDefinition -> runtime deck
        var runtimeDeck = new Deck();
        runtimeDeck.Title = deckDefinition.Title;
        runtimeDeck.HeroCard = cardManager.GetCardByID(deckDefinition.HeroCard.ID);
        runtimeDeck.Cards = deckDefinition.Cards
            .Select(c => cardManager.GetCardByID(c.ID))
            .ToList();

        // Push into viewer
        deckViewer.Setup(runtimeDeck);

        Debug.Log($"Deck loaded from {path}");
    }
}

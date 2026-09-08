using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Debug/dev window for GameManager.ExportBoardStateToFile / ImportBoardStateFromFile - lets a
// developer capture the current hand/board/hero state to a JSON file and later restore it
// exactly, without needing a live network match or scripted repro steps.
public class BoardStateExportWindow : EditorWindow
{
    private const string LastPathKey = "BoardStateExportWindow_LastPath";
    private const string DefaultPath = "Assets/BoardStateSnapshots/boardstate.json";

    private string _filePath;

    [MenuItem("Data/Board State/Export-Import Window")]
    public static void ShowWindow()
    {
        GetWindow<BoardStateExportWindow>("Board State");
    }

    private void OnEnable()
    {
        _filePath = EditorPrefs.GetString(LastPathKey, DefaultPath);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Board State Export / Import", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Enter Play mode with an active match to export or import board state.",
                MessageType.Info);
        }

        EditorGUILayout.BeginHorizontal();
        _filePath = EditorGUILayout.TextField("File Path", _filePath);
        if (GUILayout.Button("Browse...", GUILayout.Width(70)))
        {
            BrowseForPath();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        var gameManager = FindFirstObjectByType<GameManager>();
        bool canOperate = Application.isPlaying && gameManager != null && !string.IsNullOrEmpty(_filePath);

        EditorGUI.BeginDisabledGroup(!canOperate);

        if (GUILayout.Button("Export Current Board State", GUILayout.Height(30)))
        {
            Export(gameManager);
        }

        if (GUILayout.Button("Import Board State", GUILayout.Height(30)))
        {
            Import(gameManager);
        }

        EditorGUI.EndDisabledGroup();

        if (Application.isPlaying && gameManager == null)
        {
            EditorGUILayout.HelpBox("No GameManager found in the active scene.", MessageType.Warning);
        }
    }

    private void BrowseForPath()
    {
        string directory = string.IsNullOrEmpty(_filePath) ? "Assets" : Path.GetDirectoryName(_filePath);
        string fileName = string.IsNullOrEmpty(_filePath) ? "boardstate" : Path.GetFileNameWithoutExtension(_filePath);

        string path = EditorUtility.SaveFilePanelInProject(
            "Board State File",
            fileName,
            "json",
            "Choose the board state JSON file",
            directory);

        if (!string.IsNullOrEmpty(path))
        {
            _filePath = path;
            EditorPrefs.SetString(LastPathKey, _filePath);
        }
    }

    private void Export(GameManager gameManager)
    {
        try
        {
            gameManager.ExportBoardStateToFile(_filePath);
            AssetDatabase.Refresh();
            EditorPrefs.SetString(LastPathKey, _filePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Board state export failed: {e}");
        }
    }

    private void Import(GameManager gameManager)
    {
        if (!File.Exists(_filePath))
        {
            Debug.LogError($"Board state file not found: {_filePath}");
            return;
        }

        try
        {
            gameManager.ImportBoardStateFromFile(_filePath);
            EditorPrefs.SetString(LastPathKey, _filePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Board state import failed: {e}");
        }
    }
}

using UnityEngine;
using UnityEditor;
using System.IO;

public class DllImporterWindow : EditorWindow
{
    private const string PREF_SOURCE = "DllImporter_SourcePath";
    private const string PREF_TARGET = "DllImporter_TargetPath";

    private string sourceDllPath;
    private string targetDllPath;

    [MenuItem("Data/Import/DLL Importer")]
    public static void ShowWindow()
    {
        var window = GetWindow<DllImporterWindow>("DLL Importer");
        window.minSize = new Vector2(400, 150);
    }

    private void OnEnable()
    {
        // Load saved paths
        sourceDllPath = EditorPrefs.GetString(PREF_SOURCE, "");
        targetDllPath = EditorPrefs.GetString(PREF_TARGET, "");
    }

    private void OnGUI()
    {
        GUILayout.Label("DLL Import Settings", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // Source
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Source DLL:", GUILayout.Width(80));
        sourceDllPath = EditorGUILayout.TextField(sourceDllPath);

        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            string selected = EditorUtility.OpenFilePanel("Select source DLL", "", "dll");
            if (!string.IsNullOrEmpty(selected))
            {
                sourceDllPath = selected;
                EditorPrefs.SetString(PREF_SOURCE, sourceDllPath);
            }
        }
        EditorGUILayout.EndHorizontal();

        // Target
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Target DLL:", GUILayout.Width(80));
        targetDllPath = EditorGUILayout.TextField(targetDllPath);

        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            string selected = EditorUtility.OpenFilePanel("Select target DLL in project", Application.dataPath, "dll");
            if (!string.IsNullOrEmpty(selected))
            {
                targetDllPath = selected;
                EditorPrefs.SetString(PREF_TARGET, targetDllPath);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(15);

        GUI.enabled = File.Exists(sourceDllPath) && File.Exists(targetDllPath);

        if (GUILayout.Button("Import (Overwrite Target)", GUILayout.Height(30)))
        {
            ImportDll();
        }

        GUI.enabled = true;
    }

    private void ImportDll()
    {
        try
        {
            File.Copy(sourceDllPath, targetDllPath, overwrite: true);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", "DLL successfully imported and replaced.", "OK");
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("Error", "Failed to import DLL:\n" + ex.Message, "OK");
        }
    }
}

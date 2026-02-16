using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using UnityEditor.Build.Profile;


public class ItchBuildUploader : EditorWindow
{
    private const string KEY_SKIP_BUILD = "ItchBuildUploader_SkipBuild";
    private const string KEY_BUILD_FOLDER = "ItchBuildUploader_BuildFolder";
    private const string KEY_BUILD_EXE = "ItchBuildUploader_BuildExe";
    private const string KEY_SKIP_UPLOAD = "ItchBuildUploader_SkipUpload";
    private const string KEY_ITCH_TARGET = "ItchBuildUploader_ItchTarget";
    private const string KEY_BUTLER_PATH = "ItchBuildUploader_ButlerPath";
    private const string KEY_BUILD_PROFILE = "ItchBuildUploader_BuildProfile";

    private bool skipBuild = false;
    private string buildFolder = "SlayQueen-Win-Build";
    private string buildExe = "SlayQueenTheGateKeeper.exe";

    private bool skipUpload = false;
    private string itchTarget = "juicychicken/slayqueen-the-gatekeeper:windows";
    private string butlerPath = "C:/Users/yusuk/Documents/Unity/butler-windows-amd64/butler.exe";
    private string buildProfileName = "Windows Release";

    [MenuItem("Tools/Build and Upload to Itch")]
    public static void ShowWindow()
    {
        GetWindow<ItchBuildUploader>("Itch Uploader");
    }

    private void OnEnable()
    {
        skipBuild = EditorPrefs.GetBool(KEY_SKIP_BUILD, false);
        buildFolder = EditorPrefs.GetString(KEY_BUILD_FOLDER, "SlayQueen-Win-Build");
        buildExe = EditorPrefs.GetString(KEY_BUILD_EXE, "SlayQueenTheGateKeeper.exe");

        skipUpload = EditorPrefs.GetBool(KEY_SKIP_UPLOAD, false);
        itchTarget = EditorPrefs.GetString(KEY_ITCH_TARGET);
        butlerPath = EditorPrefs.GetString(KEY_BUTLER_PATH);
        buildProfileName = EditorPrefs.GetString(KEY_BUILD_PROFILE, "Windows Release");
    }

    private void OnGUI()
    {
        GUILayout.Label("Build Settings", EditorStyles.boldLabel);
        skipBuild = EditorGUILayout.Toggle("Skip Build", skipBuild);
        if (!skipBuild)
        {
            buildFolder = EditorGUILayout.TextField("Build Folder", buildFolder);
            buildExe = EditorGUILayout.TextField("Build FileName", buildExe);
            buildProfileName = EditorGUILayout.TextField("Build Profile", buildProfileName);
        }

        GUILayout.Space(10);

        GUILayout.Label("Itch Upload", EditorStyles.boldLabel);
        skipUpload = EditorGUILayout.Toggle("Skip Upload", skipUpload);
        if (!skipUpload)
        {
            itchTarget = EditorGUILayout.TextField("Itch Target", itchTarget);
            butlerPath = EditorGUILayout.TextField("Butler Path", butlerPath);
        }

        GUILayout.Space(20);

        if (GUILayout.Button("BUILD ZIP UPLOAD"))
        {
            BuildZipUpload();
        }

        EditorPrefs.SetBool(KEY_SKIP_BUILD, skipBuild);
        EditorPrefs.SetString(KEY_BUILD_FOLDER, buildFolder);
        EditorPrefs.SetString(KEY_BUILD_EXE, buildExe);

        EditorPrefs.SetBool(KEY_SKIP_UPLOAD, skipUpload);
        EditorPrefs.SetString(KEY_ITCH_TARGET, itchTarget);
        EditorPrefs.SetString(KEY_BUTLER_PATH, butlerPath);
        EditorPrefs.SetString(KEY_BUILD_PROFILE, buildProfileName);
    }

    private void BuildZipUpload()
    {
        if (!skipBuild)
            BuildUsingProfile(buildProfileName, Path.Combine(buildFolder, buildExe));

        string zipPath = ZipBuild();

        if (!skipUpload)
            Upload(zipPath);

        EditorUtility.DisplayDialog("Done", "Process complete!", "OK");
    }

    private BuildProfile FindBuildProfile(string profileName)
    {
        string[] guids = AssetDatabase.FindAssets("t:BuildProfile");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(path);

            if (profile != null && profile.name == profileName)
                return profile;
        }

        return null;
    }

    private void BuildUsingProfile(string profileName, string buildFolder)
    {
        var profile = FindBuildProfile(profileName);

        if (profile == null)
            throw new System.Exception($"Build profile not found: {profileName}");

        BuildPlayerWithProfileOptions options = new BuildPlayerWithProfileOptions();
        options.buildProfile = profile;
        options.locationPathName = $"{buildFolder}/{buildExe}";
        options.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("Build failed!");
    }

    private string ZipBuild()
    {
        string zipPath = buildFolder + ".zip";

        if (File.Exists(zipPath))
            File.Delete(zipPath);

        ZipFile.CreateFromDirectory(buildFolder, zipPath);
        return zipPath;
    }

    private void Upload(string zipPath)
    {
        Process process = new Process();
        process.StartInfo.FileName = butlerPath;
        process.StartInfo.Arguments = $"push \"{zipPath}\" {itchTarget}";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();

        while (!process.StandardOutput.EndOfStream)
            UnityEngine.Debug.Log(process.StandardOutput.ReadLine());

        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new System.Exception("Butler upload failed!");
    }
}


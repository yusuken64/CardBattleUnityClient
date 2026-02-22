using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using UnityEditor.Build.Profile;
using System;

public class BuildUploader : EditorWindow
{
    private const string KEY_SKIP_BUILD = "BuildUploader_SkipBuild";
    private const string KEY_BUILD_FOLDER = "BuildUploader_BuildFolder";
    private const string KEY_BUILD_EXE = "BuildUploader_BuildExe";

    private const string KEY_SKIP_UPLOAD_TO_ITCH = "BuildUploader_SkipUploadItch";
    private const string KEY_ITCH_TARGET = "BuildUploader_ItchTarget";
    private const string KEY_BUTLER_PATH = "BuildUploader_ButlerPath";
    private const string KEY_BUILD_PROFILE = "BuildUploader_BuildProfile";

    private const string KEY_SKIP_UPLOAD_TO_Steam = "BuildUploader_SkipUploadSteam";
    private const string KEY_STEAMVDF_PATH = "BuildUploader_STEAMVDFPath";
    private const string KEY_STEAMCMD_PATH = "BuildUploader_STEAMCMDPath";
    private const string KEY_STEAM_USERNAME = "BuildUploader_STEAMUserName";
    private const string KEY_STEAM_PASSWORD = "BuildUploader_STEAMPassWord";

    private bool skipBuild = false;
    private string buildFolder = "SlayQueen-Win-Build";
    private string buildExe = "SlayQueenTheGateKeeper.exe";

    private bool skipUploadToItch = false;
    private string itchTarget = "juicychicken/slayqueen-the-gatekeeper:windows";
    private string butlerPath = "C:/Path/butler.exe";
    private string buildProfileName = "Windows Release";

    private bool skipUploadToSteam = false;
    private string steamVdfPath = "C:/PATH/steam_build.vdf";
    private string steamCmdPath = "C:/PATH/steamcmd.exe";
    private string steamUserName = "UserName";
    private string steamPassWord = "Password";

    [MenuItem("Tools/Build and Upload")]
    public static void ShowWindow()
    {
        GetWindow<BuildUploader>("Uploader");
    }

    private void OnEnable()
    {
        skipBuild = EditorPrefs.GetBool(KEY_SKIP_BUILD, false);
        buildFolder = EditorPrefs.GetString(KEY_BUILD_FOLDER, "SlayQueen-Win-Build");
        buildExe = EditorPrefs.GetString(KEY_BUILD_EXE, "SlayQueenTheGateKeeper.exe");

        skipUploadToItch = EditorPrefs.GetBool(KEY_SKIP_UPLOAD_TO_ITCH, false);
        itchTarget = EditorPrefs.GetString(KEY_ITCH_TARGET);
        butlerPath = EditorPrefs.GetString(KEY_BUTLER_PATH);
        buildProfileName = EditorPrefs.GetString(KEY_BUILD_PROFILE, "Windows Release");

        skipUploadToSteam = EditorPrefs.GetBool(KEY_SKIP_UPLOAD_TO_Steam, false);

        steamVdfPath = EditorPrefs.GetString(KEY_STEAMVDF_PATH, "STEAMVDF");
        steamCmdPath = EditorPrefs.GetString(KEY_STEAMCMD_PATH, "SteamCMD");
        steamUserName = EditorPrefs.GetString(KEY_STEAM_USERNAME, "SteamUserName");
        steamPassWord = EditorPrefs.GetString(KEY_STEAM_PASSWORD, "SteamPasswords");
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
        skipUploadToItch = EditorGUILayout.Toggle("Skip Upload Itch", skipUploadToItch);
        if (!skipUploadToItch)
        {
            itchTarget = EditorGUILayout.TextField("Itch Target", itchTarget);
            butlerPath = EditorGUILayout.TextField("Butler Path", butlerPath);
        }

        GUILayout.Space(10);

        GUILayout.Label("Steam Upload", EditorStyles.boldLabel);
        skipUploadToSteam = EditorGUILayout.Toggle("Skip Upload To Steam", skipUploadToSteam);
        if (!skipUploadToSteam)
        {
            steamVdfPath = EditorGUILayout.TextField("Vdf", steamVdfPath);
            steamUserName = EditorGUILayout.TextField("Steam UserName", steamUserName);
            steamPassWord = EditorGUILayout.TextField("Steam PassWord", steamPassWord);
            steamCmdPath = EditorGUILayout.TextField("Steam CMD Path", steamCmdPath);
        }

        GUILayout.Space(20);

        if (GUILayout.Button("BUILD ZIP UPLOAD"))
        {
            BuildZipUpload();
        }

        EditorPrefs.SetBool(KEY_SKIP_BUILD, skipBuild);
        EditorPrefs.SetString(KEY_BUILD_FOLDER, buildFolder);
        EditorPrefs.SetString(KEY_BUILD_EXE, buildExe);

        EditorPrefs.SetBool(KEY_SKIP_UPLOAD_TO_ITCH, skipUploadToItch);
        EditorPrefs.SetString(KEY_ITCH_TARGET, itchTarget);
        EditorPrefs.SetString(KEY_BUTLER_PATH, butlerPath);
        EditorPrefs.SetString(KEY_BUILD_PROFILE, buildProfileName);

        EditorPrefs.SetBool(KEY_SKIP_UPLOAD_TO_Steam, skipUploadToSteam);
        EditorPrefs.SetString(KEY_STEAMVDF_PATH, steamVdfPath);
        EditorPrefs.SetString(KEY_STEAMCMD_PATH, steamCmdPath);
        EditorPrefs.SetString(KEY_STEAM_USERNAME, steamUserName);
        EditorPrefs.SetString(KEY_STEAM_PASSWORD, steamPassWord);
    }

    private void BuildZipUpload()
    {
        if (!skipBuild)
        {
            BuildUsingProfile(buildProfileName, buildFolder);
        }

        string zipPath = ZipBuild();

        if (!skipUploadToItch)
        {
            UploadToItch(zipPath);
        }

        if (!skipUploadToSteam)
        {
            //string vdf = GenerateSteamVdf(buildFolder, steamVdfPath);
            UploadToSteam(steamVdfPath, steamCmdPath);
        }

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

        if (Directory.Exists(buildFolder))
            Directory.Delete(buildFolder, true);

        Directory.CreateDirectory(buildFolder);

        BuildPlayerWithProfileOptions options = new BuildPlayerWithProfileOptions();
        options.buildProfile = profile;
        options.locationPathName = Path.Combine(buildFolder, buildExe);
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

    private void UploadToItch(string zipPath)
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

    private void UploadToSteam(string steamVdfPath, string steamCmdPath)
    {
        if (!System.IO.File.Exists(steamVdfPath))
            throw new System.Exception("Steam build manifest (.vdf) not found!");

        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.FileName = steamCmdPath;
        process.StartInfo.Arguments = $"+login {steamUserName} {steamPassWord} +run_app_build \"{steamVdfPath}\" +quit";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;

        process.Start();

        while (!process.StandardOutput.EndOfStream)
            UnityEngine.Debug.Log(process.StandardOutput.ReadLine());

        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new System.Exception("Steam upload failed!");
    }

    private string GenerateSteamVdf(string buildFolder, string vdfPath)
    {
        string dateStr = DateTime.Now.ToString("yyyy-MM-dd");
        string vdfContent = $@"
""appbuild""
{{
    ""appid"" ""4345100""
    ""desc"" ""Windows build {dateStr}""
    ""buildoutput"" ""{buildFolder}""
    ""contentroot"" ""{buildFolder}""
    ""setlive"" ""beta""
    ""depots""
    {{
        ""4345100"" ""{buildFolder}""
    }}
}}
";
        File.WriteAllText(vdfPath, vdfContent);
        return vdfPath;
    }
}


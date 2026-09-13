using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class SteamBuildTask
{
    [MenuItem("Tools/Steam/Build Current Client")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play mode before building.");
        var folder = Path.GetFullPath("Builds/Steam/CurrentClient");
        Directory.CreateDirectory(folder);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            target = BuildTarget.StandaloneWindows64,
            subtarget = (int)StandaloneBuildSubtarget.Player,
            locationPathName = Path.Combine(folder, "SlayQueenTheGateKeeper.exe"),
            options = BuildOptions.None
        });
        File.WriteAllText("Temp/SteamBuildResult.json", Newtonsoft.Json.JsonConvert.SerializeObject(new
        {
            result = report.summary.result.ToString(),
            errors = report.summary.totalErrors,
            warnings = report.summary.totalWarnings,
            output = report.summary.outputPath,
            duration = report.summary.totalTime.ToString()
        }, Newtonsoft.Json.Formatting.Indented));
        if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Steam client build failed.");
    }
}

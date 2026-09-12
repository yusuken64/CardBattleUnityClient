using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public sealed class CopyClientConfigToBuild : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.StandaloneWindows &&
            report.summary.platform != BuildTarget.StandaloneWindows64 &&
            report.summary.platform != BuildTarget.StandaloneLinux64 &&
            report.summary.platform != BuildTarget.StandaloneOSX)
            return;

        var source = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
            NetworkClientConfig.FileName);
        var destination = Path.Combine(Path.GetDirectoryName(report.summary.outputPath),
            NetworkClientConfig.FileName);
        // Ship the project template with local builds and the CI/Itch.io artifacts.
        File.Copy(source, destination, overwrite: true);
    }
}

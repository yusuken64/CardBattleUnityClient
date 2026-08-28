using System.IO;
using UnityEngine;

public static class FileUtils
{
    public static string[] GetFilesInRelativeFolder(string relativeFolder)
    {
        // Get path to the executable's folder
        string exeDir = Directory.GetParent(Application.dataPath).FullName;

        // Combine with your relative folder
        string targetDir = Path.Combine(exeDir, relativeFolder);

        if (!Directory.Exists(targetDir))
        {
            Debug.LogWarning($"Directory does not exist: {targetDir}");
            return new string[0];
        }

        // Get all files (non-recursive)
        return Directory.GetFiles(targetDir);
    }
}
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class NetworkManagerSceneSetup
{
    [MenuItem("Tools/Multiplayer/Wire NetworkManager")]
    public static void WireNetworkManager()
    {
        var root = GameObject.Find("Common");
        var common = root != null ? root.GetComponent<Common>() : null;
        var child = root != null ? root.transform.Find("NetworkManager") : null;
        var manager = child != null ? child.GetComponent<NetworkManager>() : null;
        if (common == null || manager == null)
            throw new InvalidOperationException("Open Common.unity with a Common/NetworkManager component first.");

        Undo.RecordObject(common, "Wire NetworkManager");
        common.NetworkManager = manager;
        EditorUtility.SetDirty(common);
        EditorSceneManager.MarkSceneDirty(root.scene);
        Debug.Log("Common.NetworkManager is wired to Common/NetworkManager.");
    }
}

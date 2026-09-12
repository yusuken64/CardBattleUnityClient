using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class PresentationEffectRegistryBuilder : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;
    public void OnPreprocessBuild(BuildReport report) => Rebuild();

    [MenuItem("Tools/Network/Rebuild Presentation Effects")]
    public static void Rebuild()
    {
        const string path = "Assets/Resources/PresentationEffectRegistry.asset";
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        var registry = AssetDatabase.LoadAssetAtPath<PresentationEffectRegistry>(path);
        if (registry == null) { registry = ScriptableObject.CreateInstance<PresentationEffectRegistry>(); AssetDatabase.CreateAsset(registry, path); }
        registry.Entries = AssetDatabase.FindAssets("t:CustomSFX").OrderBy(x => x).Select(id =>
        {
            var effect = AssetDatabase.LoadAssetAtPath<CustomSFX>(AssetDatabase.GUIDToAssetPath(id));
            return new PresentationEffectRegistry.Entry { Id = id, Effect = effect };
        }).ToList();
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
    }
}

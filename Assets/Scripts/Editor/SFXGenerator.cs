using MagicArsenal;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SFXGenerator
{
	public static string SFXArtPath = "Assets/MagicArsenal/Demo/Effect Prefabs";
	public static string SFXDefinitionFolder = "Assets/Prefab/SFX/Missiles";
	
	public static string SFXAOEArtPath = "Assets/MagicArsenal/Effects/Prefabs/AreaDamage";
	public static string SFXAOEDefinitionFolder = "Assets/Prefab/SFX/AOE";

	public static string SFXAuraArtPath = "Assets/MagicArsenal/Effects/Prefabs/Aura";
	public static string SFXAuraDefinitionFolder = "Assets/Prefab/SFX/Aura";

	[MenuItem("Data/SFX/GenerateSFX")]
	public static void GenerateSFX()
	{
		bool proceed = EditorUtility.DisplayDialog(
			"Generate SFX",
			"This will generate SFX. Continue?",
			"Yes",
			"No"
		);

		if (proceed)
		{
			GenerateSFXMissilesFromArt();
			GenerateSFXAOEFromArt();
			GenerateSFXAuraFromArt();
		}
	}

	private static void GenerateSFXMissilesFromArt()
	{
		// Ensure output folder exists
		if (!AssetDatabase.IsValidFolder(SFXDefinitionFolder))
		{
			Directory.CreateDirectory(SFXDefinitionFolder);
			AssetDatabase.Refresh();
		}

		// Find all prefabs recursively in the art path
		string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { SFXArtPath });

		foreach (string guid in guids)
		{
			string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

			if (prefab == null)
				continue;

			string assetName = prefab.name;
			string outputPath = Path.Combine(
				SFXDefinitionFolder,
				$"{assetName}.asset"
			);

			var sfx = EnsureScriptableObjectExists<ProjectileSFX>(outputPath);
			var script = prefab.GetComponent<MagicProjectileScript>();

			sfx.MuzzleObject = script.muzzleParticle;
			EnsureEffectSortingLayer(sfx.MuzzleObject);

			sfx.ProjectileObject = script.projectileParticle;
			EnsureEffectSortingLayer(sfx.ProjectileObject);

			sfx.ImpactObject = script.impactParticle;
			EnsureEffectSortingLayer(sfx.ImpactObject);

			EditorUtility.SetDirty(sfx);
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	private static void GenerateSFXAOEFromArt()
	{
		// Ensure output folder exists
		if (!AssetDatabase.IsValidFolder(SFXAOEDefinitionFolder))
		{
			Directory.CreateDirectory(SFXAOEDefinitionFolder);
			AssetDatabase.Refresh();
		}

		// Find all prefabs recursively in the art path
		string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { SFXAOEArtPath });

		foreach (string guid in guids)
		{
			string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

			if (prefab == null)
				continue;

			string assetName = prefab.name;
			string outputPath = Path.Combine(
				SFXAOEDefinitionFolder,
				$"{assetName}.asset"
			);

			var sfx = EnsureScriptableObjectExists<FullScreenSFX>(outputPath);

			sfx.FullScreenObject = prefab;
			EnsureEffectSortingLayer(sfx.FullScreenObject);

			EditorUtility.SetDirty(sfx);
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	private static void GenerateSFXAuraFromArt()
	{
		// Ensure output folder exists
		if (!AssetDatabase.IsValidFolder(SFXAuraDefinitionFolder))
		{
			Directory.CreateDirectory(SFXAuraDefinitionFolder);
			AssetDatabase.Refresh();
		}

		// Find all prefabs recursively in the art path
		string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { SFXAuraArtPath });

		foreach (string guid in guids)
		{
			string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

			if (prefab == null)
				continue;

			string assetName = prefab.name;
			string outputPath = Path.Combine(
				SFXAuraDefinitionFolder,
				$"{assetName}.asset"
			);

			var sfx = EnsureScriptableObjectExists<ProjectileSFX>(outputPath);

			sfx.MuzzleObject = prefab;
			//TODO find impact particle
			//EnsureEffectSortingLayer(sfx.FullScreenObject);

			EditorUtility.SetDirty(sfx);
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	private static T EnsureScriptableObjectExists<T>(string filePath) where T : ScriptableObject
	{
		var existing = AssetDatabase.LoadAssetAtPath<T>(filePath);
		if (existing != null)
			return existing;

		var newScriptableObject = ScriptableObject.CreateInstance<T>();

		AssetDatabase.CreateAsset(newScriptableObject, filePath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		return newScriptableObject;
	}

	private static void EnsureEffectSortingLayer(GameObject particleObject)
	{
		if (particleObject == null)
			return;

		var renderers = particleObject.GetComponentsInChildren<ParticleSystemRenderer>(true);

		foreach (var renderer in renderers)
		{
			renderer.sortingLayerName = "Effects";
			EditorUtility.SetDirty(renderer);
		}
	}
}

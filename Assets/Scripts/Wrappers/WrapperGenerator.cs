using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class WrapperGenerator
{
    // Interfaces to generate wrappers for
    private static readonly Type[] TargetInterfaces = new Type[]
    {
        typeof(CardBattleEngine.ITriggerCondition),
        typeof(CardBattleEngine.IGameAction),
        typeof(CardBattleEngine.IAffectedEntitySelector)
    };

    [MenuItem("Data/GenerateWrappers")]
    public static void GenerateWrappers()
    {
        foreach (var interfaceType in TargetInterfaces)
        {
            GenerateWrappersForInterface(interfaceType);
        }

        AssetDatabase.Refresh();
        Debug.Log("All wrappers generated!");
    }

    private static void GenerateWrappersForInterface(Type interfaceType)
    {
        // Find all concrete implementations
        var concreteTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToArray();

        if (concreteTypes.Length == 0)
        {
            Debug.LogWarning("No concrete implementations found for " + interfaceType.FullName);
            return;
        }

        // Find the interface script path
        string interfacePath = AssetDatabase.FindAssets(interfaceType.Name + " t:Script")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .FirstOrDefault();

        if (string.IsNullOrEmpty(interfacePath))
        {
            Debug.LogWarning($"Could not find .cs file for {interfaceType.Name}, using Assets folder.");
            interfacePath = "Assets";
        }
        else
        {
            interfacePath = Path.GetDirectoryName(interfacePath);
        }

        foreach (var type in concreteTypes)
        {
            // Ensure interface subfolder exists
            string interfaceFolder = Path.Combine(interfacePath, interfaceType.Name);
            if (!Directory.Exists(interfaceFolder))
            {
                Directory.CreateDirectory(interfaceFolder);
                AssetDatabase.Refresh();
            }

            // Generate wrapper file path
            string wrapperName = type.Name + "Wrapper";
            string filePath = Path.Combine(interfaceFolder, wrapperName + ".cs");

            // Overwrite if exists
            string code = GenerateWrapperCode(type, wrapperName, interfaceType);
            File.WriteAllText(filePath, code);
            Debug.Log("Generated wrapper: " + filePath);
        }
    }

    private static string GenerateWrapperCode(Type concreteType, string wrapperName, Type interfaceType)
    {
        // Grab all public read/write properties
        var props = concreteType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .ToArray();

        string propertyLines = string.Join(Environment.NewLine,
            props.Select(p => $"    public {GetCSharpTypeName(p.PropertyType)} {p.Name};"));


        string assignments = string.Join(Environment.NewLine,
            props.Select(p => $"        instance.{p.Name} = this.{p.Name};"));

        // Generate base class depending on the interface type
        string baseClass = interfaceType.Name + "WrapperBase"; // e.g., ITriggerConditionWrapperBase

        return $@"using System;
using UnityEngine;
using {interfaceType.Namespace};

[Serializable]
public class {wrapperName} : {baseClass}
{{
{propertyLines}

    public override {interfaceType.FullName} Create()
    {{
        var instance = new {concreteType.FullName}();
{assignments}
        return instance;
    }}
}}";
    }
    private static string GetCSharpTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            string genericType = type.GetGenericTypeDefinition().FullName;
            genericType = genericType.Substring(0, genericType.IndexOf('`')); // Remove `1
            string genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetCSharpTypeName));
            return $"{genericType}<{genericArgs}>";
        }
        else
        {
            return type.FullName;
        }
    }
}

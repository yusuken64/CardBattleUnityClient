using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class WrapperGenerator
{
    private static readonly Dictionary<Type, string> InterfaceToBaseName = new()
    {
        { typeof(CardBattleEngine.ITriggerCondition), "ITriggerConditionWrapperBase" },
        { typeof(CardBattleEngine.IGameAction), "IGameActionWrapperBase" },
        { typeof(CardBattleEngine.IAffectedEntitySelector), "IAffectedEntitySelectorWrapperBase" },
        { typeof(CardBattleEngine.ITargetOperation), "ITargetOperationWrapperBase" }
    };

    [MenuItem("Data/GenerateWrappers")]
    public static void GenerateWrappers()
    {
        foreach (var pair in InterfaceToBaseName.Keys)
            GenerateWrappersForInterface(pair);

        AssetDatabase.Refresh();
        Debug.Log("All wrappers generated!");
    }

    private static void GenerateWrappersForInterface(Type interfaceType)
    {
        var concreteTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => interfaceType.IsAssignableFrom(t) &&
                        !t.IsInterface &&
                        !t.IsAbstract)
            .ToArray();

        if (concreteTypes.Length == 0)
        {
            Debug.LogWarning("No implementations for " + interfaceType.FullName);
            return;
        }

        // Look up the interface .cs file
        string interfacePath = AssetDatabase.FindAssets(interfaceType.Name + " t:Script")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(Path.GetDirectoryName)
            .FirstOrDefault();

        if (interfacePath == null)
            interfacePath = "Assets";

        // Each interface gets its own folder
        string targetFolder = Path.Combine(interfacePath, interfaceType.Name);
        if (!Directory.Exists(targetFolder))
            Directory.CreateDirectory(targetFolder);

        foreach (var type in concreteTypes)
        {
            string wrapperName = type.Name + "Wrapper";
            string filePath = Path.Combine(targetFolder, wrapperName + ".cs");

            string code = GenerateWrapperCode(type, wrapperName, interfaceType);
            File.WriteAllText(filePath, code);

            Debug.Log("Generated wrapper: " + filePath);
        }
    }

    private static string GenerateWrapperCode(Type concreteType, string wrapperName, Type interfaceType)
    {
        string baseClass = InterfaceToBaseName[interfaceType];

        var props = concreteType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .ToArray();

        string propertyLines = string.Join("\n",
            props.Select(p => $"    public {GetCSharpTypeName(p.PropertyType)} {p.Name};"));

        string assignments = string.Join("\n",
            props.Select(p => $"        instance.{p.Name} = this.{p.Name};"));

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
        // Nullable<T> → T?
        if (Nullable.GetUnderlyingType(type) is Type underlying)
            return GetCSharpTypeName(underlying) + "?";

        // Generic Types
        if (type.IsGenericType)
        {
            string fullName = type.GetGenericTypeDefinition().FullName;

            // Strip the `1, `2, etc.
            int tickIndex = fullName.IndexOf('`');
            if (tickIndex >= 0)
                fullName = fullName.Substring(0, tickIndex);

            string args = string.Join(", ", type.GetGenericArguments().Select(GetCSharpTypeName));

            return $"{fullName}<{args}>";
        }

        // Array types
        if (type.IsArray)
        {
            return GetCSharpTypeName(type.GetElementType()) + "[]";
        }

        // Fallback: full name (includes namespace)
        return type.FullName;
    }
}

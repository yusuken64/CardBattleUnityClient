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
        typeof(CardBattleEngine.IAffectedEntitySelector),
        typeof(CardBattleEngine.ITargetOperation) // recursion target
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
        // ignore generic interfaces like IEnumerable<T>
        if (interfaceType.IsGenericType || interfaceType.IsGenericTypeDefinition)
            return;

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
            string interfaceFolder = Path.Combine(interfacePath, interfaceType.Name);
            if (!Directory.Exists(interfaceFolder))
            {
                Directory.CreateDirectory(interfaceFolder);
                AssetDatabase.Refresh();
            }

            string wrapperName = type.Name + "Wrapper";
            string filePath = Path.Combine(interfaceFolder, wrapperName + ".cs");

            string code = GenerateWrapperCode(type, wrapperName, interfaceType);
            File.WriteAllText(filePath, code);
            Debug.Log("Generated wrapper: " + filePath);
        }
    }

    private static string GenerateWrapperCode(Type concreteType, string wrapperName, Type interfaceType)
    {
        var props = concreteType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .ToArray();

        // Convert interface properties into wrapper properties
        string propertyLines = string.Join(Environment.NewLine,
            props.Select(p => GeneratePropertyLine(p)));

        string assignments = string.Join(Environment.NewLine,
            props.Select(p => GenerateAssignmentLine(p)));

        string baseClass = interfaceType.Name + "WrapperBase";

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

    private static string GeneratePropertyLine(PropertyInfo p)
    {
        var type = p.PropertyType;

        // If property is an interface, use wrapper instead
        if (type.IsInterface)
        {
            return $"    public {type.Name}WrapperBase {p.Name};";
        }

        // If property is List<T> of interface -> List<WrapperBase>
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) &&
            type.IsGenericType)
        {
            var arg = type.GetGenericArguments()[0];
            if (arg.IsInterface)
            {
                return $"    public System.Collections.Generic.List<{arg.Name}WrapperBase> {p.Name};";
            }
        }

        return $"    public {GetCSharpTypeName(type)} {p.Name};";
    }

    private static string GenerateAssignmentLine(PropertyInfo p)
    {
        var type = p.PropertyType;

        if (type.IsInterface)
        {
            return $"        instance.{p.Name} = this.{p.Name}?.Create();";
        }

        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) &&
            type.IsGenericType)
        {
            var arg = type.GetGenericArguments()[0];
            if (arg.IsInterface)
            {
                return
$@"        instance.{p.Name} = this.{p.Name}?
            .Select(w => w?.Create())
            .ToList();";
            }
        }

        return $"        instance.{p.Name} = this.{p.Name};";
    }

    private static string GetCSharpTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            string genericType = type.GetGenericTypeDefinition().FullName;
            genericType = genericType.Substring(0, genericType.IndexOf('`'));
            string genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetCSharpTypeName));
            return $"{genericType}<{genericArgs}>";
        }
        else
        {
            return type.FullName;
        }
    }
}

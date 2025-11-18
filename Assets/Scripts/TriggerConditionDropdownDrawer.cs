using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

[CustomPropertyDrawer(typeof(ITriggerConditionWrapperBase), true)]
public class TriggerConditionDropdownDrawer : PropertyDrawer
{
    private Type[] _types;
    private string[] _typeNames;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Initialize concrete types
        if (_types == null)
        {
            _types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(ITriggerConditionWrapperBase).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToArray();

            _typeNames = _types.Select(t => t.Name).ToArray();
        }

        // Current type
        Type currentType = property.managedReferenceValue?.GetType();
        int currentIndex = Array.FindIndex(_types, t => t == currentType);

        // Dropdown
        int newIndex = EditorGUI.Popup(position, label.text, currentIndex + 1, new[] { "None" }.Concat(_typeNames).ToArray()) - 1;
        property.managedReferenceValue = newIndex >= 0 ? Activator.CreateInstance(_types[newIndex]) : null;

        // If a trigger is selected, draw its fields inline
        if (property.managedReferenceValue != null)
        {
            // Move to next line(s)
            EditorGUI.indentLevel++;
            var propertyRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2,
                                        position.width, EditorGUIUtility.singleLineHeight);

            SerializedProperty triggerProp = property.Copy();
            triggerProp.NextVisible(true); // skip the reference itself
            SerializedProperty endProp = triggerProp.GetEndProperty(true);

            while (!SerializedProperty.EqualContents(triggerProp, endProp))
            {
                propertyRect.height = EditorGUI.GetPropertyHeight(triggerProp, true);
                EditorGUI.PropertyField(propertyRect, triggerProp, true);
                propertyRect.y += propertyRect.height + 2;
                triggerProp.NextVisible(false);
            }

            EditorGUI.indentLevel--;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight; // dropdown line

        if (property.managedReferenceValue != null)
        {
            SerializedProperty triggerProp = property.Copy();
            triggerProp.NextVisible(true);
            SerializedProperty endProp = triggerProp.GetEndProperty(true);

            while (!SerializedProperty.EqualContents(triggerProp, endProp))
            {
                height += EditorGUI.GetPropertyHeight(triggerProp, true) + 2;
                triggerProp.NextVisible(false);
            }
        }

        return height;
    }
}

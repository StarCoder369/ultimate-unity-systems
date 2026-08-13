#if UNITY_EDITOR

using System;
using System.Reflection;
using ModularInspector;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
public class ModularInspectorEditor : Editor
{
    private IncludeModularInspectorAttribute settings;

    private void OnEnable()
    {
        settings = target.GetType().GetCustomAttribute<IncludeModularInspectorAttribute>(true);
    }

    public override void OnInspectorGUI()
    {
        if (settings == null)
        {
            DrawDefaultInspector();
            return;
        }

        serializedObject.Update();

        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;
            DrawProperty(property);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawProperty(SerializedProperty property)
    {
        if (property.propertyPath == "m_Script")
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(property);
            EditorGUI.EndDisabledGroup();
            return;
        }

        FieldInfo field = FindField(target.GetType(), property.name);

        if (field == null)
        {
            EditorGUILayout.PropertyField(property, true);
            return;
        }

        if (field.GetCustomAttribute<InspectorHideAttribute>() != null)
        {
            return;
        }

        InspectorShowIfAttribute showIf = field.GetCustomAttribute<InspectorShowIfAttribute>();

        if (showIf != null && !EvaluateCondition(showIf.PropertyName, showIf.CompareValue))
        {
            return;
        }

        InspectorHideIfAttribute hideIf = field.GetCustomAttribute<InspectorHideIfAttribute>();

        if (hideIf != null && EvaluateCondition(hideIf.PropertyName, hideIf.CompareValue))
        {
            return;
        }

        InspectorDisableIfAttribute disableIf = field.GetCustomAttribute<InspectorDisableIfAttribute>();
        bool disabled = disableIf != null && EvaluateCondition(disableIf.PropertyName, disableIf.CompareValue);

        DrawHeader(field);
        DrawSeparator(field);
        DrawSpace(field);

        GUIContent label = new GUIContent(property.displayName);

        InspectorLabelAttribute labelAttribute = field.GetCustomAttribute<InspectorLabelAttribute>();

        if (labelAttribute != null)
        {
            label.text = labelAttribute.Label;
        }

        InspectorTooltipAttribute tooltip = field.GetCustomAttribute<InspectorTooltipAttribute>();

        if (tooltip != null)
        {
            label.tooltip = tooltip.Tooltip;
        }

        bool previousEnabled = GUI.enabled;

        if (disabled || field.GetCustomAttribute<InspectorReadOnlyAttribute>() != null)
        {
            GUI.enabled = false;
        }

        InspectorRangeAttribute range = field.GetCustomAttribute<InspectorRangeAttribute>();

        if (range != null && property.propertyType == SerializedPropertyType.Float)
        {
            property.floatValue = EditorGUILayout.Slider(label, property.floatValue, range.Min, range.Max);
        }
        else if (range != null && property.propertyType == SerializedPropertyType.Integer)
        {
            property.intValue = EditorGUILayout.IntSlider(label, property.intValue, Mathf.RoundToInt(range.Min), Mathf.RoundToInt(range.Max));
        }
        else
        {
            EditorGUILayout.PropertyField(property, label, true);
        }

        GUI.enabled = previousEnabled;
    }

    private void DrawHeader(FieldInfo field)
    {
        InspectorHeaderAttribute header = field.GetCustomAttribute<InspectorHeaderAttribute>();

        if (header == null)
        {
            return;
        }

        GUILayout.Space(5);

        GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
        style.fontSize = Mathf.RoundToInt(header.Size);
        style.fontStyle = header.Bold ? FontStyle.Bold : FontStyle.Normal;

        EditorGUILayout.LabelField(header.Title, style);

        GUILayout.Space(3);
    }

    private void DrawSeparator(FieldInfo field)
    {
        InspectorSeparatorAttribute separator = field.GetCustomAttribute<InspectorSeparatorAttribute>();

        if (separator == null)
        {
            return;
        }

        GUILayout.Space(separator.SpaceBefore);

        Rect rect = EditorGUILayout.GetControlRect(false, 1f);
        EditorGUI.DrawRect(rect, new Color(0.35f, 0.35f, 0.35f, 1f));

        GUILayout.Space(separator.SpaceAfter);
    }

    private void DrawSpace(FieldInfo field)
    {
        InspectorSpaceAttribute space = field.GetCustomAttribute<InspectorSpaceAttribute>();

        if (space != null)
        {
            GUILayout.Space(space.Amount);
        }
    }

    private bool EvaluateCondition(string propertyName, object compareValue)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            return false;
        }

        if (compareValue == null)
        {
            return property.propertyType == SerializedPropertyType.Boolean && property.boolValue;
        }

        if (property.propertyType == SerializedPropertyType.Boolean)
        {
            return property.boolValue == Convert.ToBoolean(compareValue);
        }

        if (property.propertyType == SerializedPropertyType.Integer)
        {
            return property.longValue == Convert.ToInt64(compareValue);
        }

        if (property.propertyType == SerializedPropertyType.Float)
        {
            return Mathf.Approximately((float)property.doubleValue, Convert.ToSingle(compareValue));
        }

        if (property.propertyType == SerializedPropertyType.String)
        {
            return property.stringValue == compareValue.ToString();
        }

        return false;
    }

    private FieldInfo FindField(Type type, string fieldName)
    {
        while (type != null)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            if (field != null)
            {
                return field;
            }

            type = type.BaseType;
        }

        return null;
    }
}

#endif
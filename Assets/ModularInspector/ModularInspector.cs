using System;
using UnityEngine;

namespace ModularInspector
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class IncludeModularInspectorAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorHeaderAttribute : PropertyAttribute
    {
        public string Title;
        public float Size;
        public bool Bold;

        public InspectorHeaderAttribute(string title, float size = 13f, bool bold = true)
        {
            Title = title;
            Size = size;
            Bold = bold;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorTooltipAttribute : PropertyAttribute
    {
        public string Tooltip;

        public InspectorTooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorSpaceAttribute : PropertyAttribute
    {
        public float Amount;

        public InspectorSpaceAttribute(float amount = 8f)
        {
            Amount = amount;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorRangeAttribute : PropertyAttribute
    {
        public float Min;
        public float Max;

        public InspectorRangeAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorHideAttribute : PropertyAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorReadOnlyAttribute : PropertyAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorShowIfAttribute : PropertyAttribute
    {
        public string PropertyName;
        public object CompareValue;

        public InspectorShowIfAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public InspectorShowIfAttribute(string propertyName, object compareValue)
        {
            PropertyName = propertyName;
            CompareValue = compareValue;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorHideIfAttribute : PropertyAttribute
    {
        public string PropertyName;
        public object CompareValue;

        public InspectorHideIfAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public InspectorHideIfAttribute(string propertyName, object compareValue)
        {
            PropertyName = propertyName;
            CompareValue = compareValue;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorDisableIfAttribute : PropertyAttribute
    {
        public string PropertyName;
        public object CompareValue;

        public InspectorDisableIfAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public InspectorDisableIfAttribute(string propertyName, object compareValue)
        {
            PropertyName = propertyName;
            CompareValue = compareValue;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorLabelAttribute : PropertyAttribute
    {
        public string Label;

        public InspectorLabelAttribute(string label)
        {
            Label = label;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorSeparatorAttribute : PropertyAttribute
    {
        public float SpaceBefore;
        public float SpaceAfter;

        public InspectorSeparatorAttribute(float spaceBefore = 5f, float spaceAfter = 5f)
        {
            SpaceBefore = spaceBefore;
            SpaceAfter = spaceAfter;
        }
    }
}
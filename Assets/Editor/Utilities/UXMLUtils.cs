using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ADOFAIModdingHelper.Utilities
{
    public static class UXMLUtils
    {

        //public static ListView MakeBindItemDropdown(ListView listView, SerializedProperty prop, string dropdownName, List<string> choices)
        //{
        //    listView.makeItem = () =>
        //    {
        //        var container = new PropertyField() { style = { flexDirection = FlexDirection.Row } };

        //        var label = new Label("Element") { name = "IndexLabel", style = { minWidth = 95, unityTextAlign = TextAnchor.MiddleLeft } };

        //        var dropdown = new DropdownField() { name = dropdownName, style = { flexGrow = 1, paddingBottom = 2, paddingTop = 2 } };

        //        var textElement = dropdown.Q<TextElement>();
        //        if (textElement != null)
        //        {
        //            textElement.style.textOverflow = TextOverflow.Ellipsis;
        //            textElement.style.overflow = Overflow.Hidden;
        //            textElement.style.whiteSpace = WhiteSpace.NoWrap;
        //        }
        //        else Debug.Log("textElementNulls");

        //        container.Add(label);
        //        container.Add(dropdown);
        //        return container;
        //    };

        //    listView.bindItem = (element, i) =>
        //    {
        //        element.Q<Label>("IndexLabel").text = $"Element {i}";
        //        var dropdown = element.Q<DropdownField>(dropdownName);
        //        dropdown.choices = choices;

        //        var property = prop.GetArrayElementAtIndex(i);
        //        dropdown.value = property.stringValue;

        //        dropdown.RegisterValueChangedCallback(evt =>
        //        {
        //            property.stringValue = evt.newValue;
        //            property.serializedObject.ApplyModifiedProperties();
        //        });
        //    };

        //    return listView;
        //}
        public static T GetUXMLAnimationProperty<T>(IStyle style, string propertyName, bool Duration = true) where T : struct
        {
            if (typeof(T) == typeof(TimeValue))
            {
                if (Duration)
                {
                    var durations = style.transitionDuration.value;
                    var properties = style.transitionProperty.value;

                    if (durations != null && properties != null)
                    {
                        for (int i = 0; i < properties.Count; i++)
                        {
                            if (properties[i] == propertyName)
                            {
                                return (T)(object)durations[i];
                            }
                        }
                    }
                }
                else
                {
                    var delays = style.transitionDelay.value;
                    var properties = style.transitionProperty.value;

                    if (delays != null && properties != null)
                    {
                        for (int i = 0; i < properties.Count; i++)
                        {
                            if (properties[i] == propertyName)
                            {
                                return (T)(object)delays[i];
                            }
                        }
                    }
                }
            }
            else if (typeof(T) == typeof(EasingFunction))
            {
                var easings = style.transitionTimingFunction.value;
                var properties = style.transitionProperty.value;

                if (easings != null && properties != null)
                {
                    for (int i = 0; i < properties.Count; i++)
                    {
                        if (properties[i] == propertyName)
                        {
                            return (T)(object)easings[i];
                        }
                    }
                }
            }
            else if (typeof(T) == typeof(StylePropertyName))
            {
                var properties = style.transitionProperty.value;
                if (properties != null)
                {
                    foreach (var p in properties)
                    {
                        if (p == propertyName)
                            return (T)(object)p;
                    }
                }
            }

            return default;
        }
    }
}
using UnityEngine.UIElements;

namespace ADOFAIModdingHelper.Utilities
{
    public static class UXMLUtils
    {
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
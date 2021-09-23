// ArcGISMapsSDK

using ArcGISMapsSDK.UX;
using System;

// Unity

using UnityEditor;
using UnityEngine.UIElements;

namespace ArcGISMapsSDK.Editor
{
    public static class MapControllerUtilities
    {
        public static Slider CreateSlider(VisualElement element, string sliderName, float defaultValue)
        {
            Slider slider = element.Query<Slider>(name: sliderName);
            slider.value = defaultValue;
            return slider;
        }

        public static double ParseDouble(string value)
        {
            double result;
            if (Double.TryParse(value, out result))
            {
                return result;
            }
            return 0;
        }

        public static string FindAssetPath(string name)
        {
            var results = AssetDatabase.FindAssets(name);

            if (results.Length == 0 || results == null)
            {
                UnityEngine.Debug.LogError("Asset " + name + " not found");
            }
            else if (results.Length > 1)
            {
                UnityEngine.Debug.LogError("Found more than one asset named " + name + ".\nPlease give the asset a unique name");
            }

            return AssetDatabase.GUIDToAssetPath(results[0]);
        }

        public static void ToggleCheckbox(Button toggle, bool value)
        {
            if (value)
            {
                toggle.AddToClassList("custom-toggle-enabled");
            }
            else
            {
                toggle.RemoveFromClassList("custom-toggle-enabled");
            }
        }

        public static void MarkDirty(ArcGISMapController mapController)
        {
            if (!EditorUtility.IsDirty(mapController.gameObject.GetInstanceID()))
            {
                EditorUtility.SetDirty(mapController);
            }
        }
    }
}

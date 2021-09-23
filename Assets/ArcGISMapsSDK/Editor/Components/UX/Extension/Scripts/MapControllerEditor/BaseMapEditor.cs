// ArcGISMapsSDK

using ArcGISMapsSDK.UX;

// Unity

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ArcGISMapsSDK.Editor
{
    public class BaseMapEditor
    {
        private const string BasemapElevationOption = "basemap-terrain-elevation-enabled";
        private const string BasemapSelectorWrapper = "basemap-selector-wrapper";
        private const string BasemapThumbnail = "basemap-thumbnail";
        private const string EditorBasemapStylesFileName = "BasemapStyles";

        private const string Imagery = "Imagery";
        private const string Streets = "Streets";
        private const string Topographic = "Topographic";
        private const string NatGeo = "National Geographic";
        private const string Oceans = "Oceans";
        private const string LightGrayCanvas = "Light Gray Canvas";
        private const string DarkGrayCanvas = "Dark Gray Canvas";

        private const string NatGeoClass = "NatGeo";
        private const string LightCanvasClass = "Light_Gray_Canvas";
        private const string DarkCanvasClass = "Dark_Gray_Canvas";

        private VisualElement mapEditor;
        private ArcGISMapController mapController;
        private Button baseMapThumbnail;

        public BaseMapEditor(VisualElement editor, ArcGISMapController controller)
        {
            mapEditor = editor;
            mapController = controller;

            var baseMapStylePath = MapControllerUtilities.FindAssetPath(EditorBasemapStylesFileName);
            mapEditor.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(baseMapStylePath));

            baseMapThumbnail = mapEditor.Query<Button>(name: BasemapThumbnail);
            VisualElement baseMapSelectorWrapper = mapEditor.Query<VisualElement>(name: BasemapSelectorWrapper);

            // BaseMap Selector

            var baseMapOptions = new PopupField<string>("Select Base Map", CreateChoices(), GetBaseMapChoice());
            baseMapSelectorWrapper.Add(baseMapOptions);
            baseMapSelectorWrapper.RegisterCallback<ChangeEvent<string>>(evnt =>
            {
                SetBaseMap(evnt.newValue);
                MapControllerUtilities.MarkDirty(mapController);
            });

            // Elevation

            Button elevationExt = mapEditor.Query<Button>(name: BasemapElevationOption);
            MapControllerUtilities.ToggleCheckbox(elevationExt, mapController.TerrainElevation);
            elevationExt.RegisterCallback<MouseUpEvent>(evt =>
            {
                mapController.TerrainElevation = !mapController.TerrainElevation;
                MapControllerUtilities.ToggleCheckbox(elevationExt, mapController.TerrainElevation);
                EditorUtility.SetDirty(mapController);
                MapControllerUtilities.MarkDirty(mapController);
            });
        }

        private List<string> CreateChoices()
        {
            List<string> choices = new List<string>();

            choices.Add(Imagery);
            choices.Add(Streets);
            choices.Add(Topographic);
            choices.Add(NatGeo);
            choices.Add(Oceans);
            choices.Add(LightGrayCanvas);
            choices.Add(DarkGrayCanvas);

            return choices;
        }

        private string GetBaseMapChoice()
        {
            switch (mapController.BaseMap)
            {
                case BaseMapType.Imagery:
                    baseMapThumbnail.AddToClassList(Imagery);
                    return Imagery;
                case BaseMapType.Streets:
                    baseMapThumbnail.AddToClassList(Streets);
                    return Streets;
                case BaseMapType.Topographic:
                    baseMapThumbnail.AddToClassList(Topographic);
                    return Topographic;
                case BaseMapType.NatGeo:
                    baseMapThumbnail.AddToClassList(NatGeoClass);
                    return NatGeo;
                case BaseMapType.Oceans:
                    baseMapThumbnail.AddToClassList(Oceans);
                    return Oceans;
                case BaseMapType.LightGrayCanvas:
                    baseMapThumbnail.AddToClassList(LightCanvasClass);
                    return LightGrayCanvas;
                case BaseMapType.DarkGrayCanvas:
                    baseMapThumbnail.AddToClassList(DarkCanvasClass);
                    return DarkGrayCanvas;
                default:
                    baseMapThumbnail.AddToClassList(Imagery);
                    return Imagery;
            }
        }

        private void SetBaseMap(string value)
        {
            baseMapThumbnail.ClearClassList();

            if (value == Imagery)
            {
                baseMapThumbnail.AddToClassList(Imagery);
                mapController.BaseMap = BaseMapType.Imagery;
            }
            else if (value == Streets)
            {
                baseMapThumbnail.AddToClassList(Streets);
                mapController.BaseMap = BaseMapType.Streets;
            }
            else if (value == Topographic)
            {
                baseMapThumbnail.AddToClassList(Topographic);
                mapController.BaseMap = BaseMapType.Topographic;
            }
            else if (value == NatGeo)
            {
                baseMapThumbnail.AddToClassList(NatGeoClass);
                mapController.BaseMap = BaseMapType.NatGeo;
            }
            else if (value == Oceans)
            {
                baseMapThumbnail.AddToClassList(Oceans);
                mapController.BaseMap = BaseMapType.Oceans;
            }
            else if (value == LightGrayCanvas)
            {
                baseMapThumbnail.AddToClassList(LightCanvasClass);
                mapController.BaseMap = BaseMapType.LightGrayCanvas;
            }
            else if (value == DarkGrayCanvas)
            {
                baseMapThumbnail.AddToClassList(DarkCanvasClass);
                mapController.BaseMap = BaseMapType.DarkGrayCanvas;
            }
        }
    }
}

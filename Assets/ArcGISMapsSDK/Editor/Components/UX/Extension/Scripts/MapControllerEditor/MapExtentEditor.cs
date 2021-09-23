// ArcGISMapsSDK

using ArcGISMapsSDK.UX;

// .Net

using System.Collections.Generic;

// Unity

using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ArcGISMapsSDK.Editor
{
    public class MapExtentEditor
    {
        private const string WrapperShapeSelector = "shape-selector-wrapper";
        private const string ShapeConfiguratorX = "shape-dimension-x-text";
        private const string ShapeConfiguratorY = "shape-dimension-y-text";
        private const string LatExtentName = "lat-extent-key-text";
        private const string LngExtentName = "lng-extent-key-text";
        private const string AltitudeExtentName = "slider-extent-altitude";
        private const string ResetExtentButtonName = "reset-extent-button";

        private VisualElement mapEditor;
        private ArcGISMapController mapController;
        private Foldout mapExtentFoldout;
        private VisualElement mapExtentParent;
        private TextField latExtText;
        private TextField lngExtText;
        private PopupField<string> shapeOptions;
        private Slider altitudeExtSlider;
        private TextField altitudeExtText;
        private TextField shapeConfigWidth;
        private TextField shapeConfigLength;

        private int mapExtentChildIndex;

        private List<string> choices = new List<string> { "Square", "Rectangle", "Circle" };

        public MapExtentEditor(VisualElement editor, ArcGISMapController controller)
        {
            mapEditor = editor;
            mapController = controller;
            InitializeMapExtent();
        }

        public void EnableFoldout()
        {
            mapExtentFoldout.visible = true;
            mapExtentParent.Insert(mapExtentChildIndex, mapExtentFoldout);
        }

        public void DisableFoldout()
        {
            mapExtentFoldout.visible = false;
            mapExtentFoldout.RemoveFromHierarchy();
        }

        private void InitializeMapExtent()
        {
            latExtText = mapEditor.Query<TextField>(name: LatExtentName);
            latExtText.value = mapController.Extent.Latitude.ToString();
            latExtText.RegisterValueChangedCallback(evnt =>
            {
                mapController.Extent.Latitude = MapControllerUtilities.ParseDouble(evnt.newValue);
                MapControllerUtilities.MarkDirty(mapController);
            });

            lngExtText = mapEditor.Query<TextField>(name: LngExtentName);
            lngExtText.value = mapController.Extent.Longitude.ToString();
            lngExtText.RegisterValueChangedCallback(evnt =>
            {
                mapController.Extent.Longitude = MapControllerUtilities.ParseDouble(evnt.newValue);
                MapControllerUtilities.MarkDirty(mapController);
            });

            altitudeExtSlider = MapControllerUtilities.CreateSlider(mapEditor, AltitudeExtentName, mapController.Extent.Altitude);
            altitudeExtText = mapEditor.Query<TextField>(name: AltitudeExtentName + "-text");
            altitudeExtText.value = mapController.Extent.Altitude.ToString() + "m";
            altitudeExtText.RegisterValueChangedCallback(evnt =>
            {
                string value = evnt.newValue.Replace("m", "");
                double result = MapControllerUtilities.ParseDouble(value);
                mapController.Extent.Altitude = (float)result;
                altitudeExtSlider.SetValueWithoutNotify((float)result);
                altitudeExtText.SetValueWithoutNotify(value + "m");
                MapControllerUtilities.MarkDirty(mapController);
            });
            altitudeExtSlider.RegisterValueChangedCallback(evnt =>
            {
                mapController.Extent.Altitude = evnt.newValue;
                altitudeExtText.SetValueWithoutNotify(evnt.newValue.ToString() + "m");
                MapControllerUtilities.MarkDirty(mapController);
            });

            VisualElement shapeWrapper = mapEditor.Query<VisualElement>(name: WrapperShapeSelector);
            shapeOptions = new PopupField<string>("Shape", choices, 0);
            shapeOptions.value = GetMapExtent();
            shapeWrapper.Add(shapeOptions);
            SetMapExtent(shapeOptions.value);
            shapeWrapper.RegisterCallback<ChangeEvent<string>>(evnt =>
            {
                SetMapExtent(evnt.newValue);
                MapControllerUtilities.MarkDirty(mapController);
            });

            shapeConfigWidth = mapEditor.Query<TextField>(name: ShapeConfiguratorX);
            shapeConfigWidth.value = GetWidthValueString();
            shapeConfigWidth.RegisterValueChangedCallback(evnt =>
            {
                mapController.Extent.Width = MapControllerUtilities.ParseDouble(evnt.newValue);
                MapControllerUtilities.MarkDirty(mapController);
            });

            shapeConfigLength = mapEditor.Query<TextField>(name: ShapeConfiguratorY);
            shapeConfigLength.value = (mapController.Extent.Length == 0) ? "Y" : mapController.Extent.Length.ToString();
            shapeConfigLength.RegisterValueChangedCallback(evnt =>
            {
                mapController.Extent.Length = MapControllerUtilities.ParseDouble(evnt.newValue);
                MapControllerUtilities.MarkDirty(mapController);
            });

            Button resetExtentButton = mapEditor.Query<Button>(name: ResetExtentButtonName);
            resetExtentButton.RegisterCallback<MouseUpEvent>(evnt =>
            {
                ResetExtent();
                MapControllerUtilities.MarkDirty(mapController);
            });

            mapExtentFoldout = mapEditor.Query<Foldout>(className: "map-extent-foldout");
            mapExtentParent = mapExtentFoldout.parent;
            mapExtentChildIndex = mapExtentParent.IndexOf(mapExtentFoldout);
        }

        private string GetMapExtent()
        {
            var Square = choices[0];
            var Rectangle = choices[1];
            var Circle = choices[2];

            if (mapController.Extent.ExtentType == ExtentType.Square)
            {
                return Square;
            }
            else if (mapController.Extent.ExtentType == ExtentType.Rectangle)
            {
                return Rectangle;
            }
            else if (mapController.Extent.ExtentType == ExtentType.Circle)
            {
                return Circle;
            }
            else
            {
                return Square;
            }
        }

        private void SetMapExtent(string option)
        {
            var Square = choices[0];
            var Rectangle = choices[1];
            var Circle = choices[2];

            TextField wrapper;
            if (option == Square)
            {
                mapController.Extent.ExtentType = ExtentType.Square;
                wrapper = mapEditor.Query<TextField>(name: ShapeConfiguratorX);
                wrapper.SetEnabled(true);
                wrapper.value = GetWidthValueString();
                wrapper = mapEditor.Query<TextField>(name: ShapeConfiguratorY);
                wrapper.SetEnabled(false);
            }
            else if (option == Rectangle)
            {
                mapController.Extent.ExtentType = ExtentType.Rectangle;
                wrapper = mapEditor.Query<TextField>(name: ShapeConfiguratorX);
                wrapper.SetEnabled(true);
                wrapper.value = GetWidthValueString();
                wrapper = mapEditor.Query<TextField>(name: ShapeConfiguratorY);
                wrapper.SetEnabled(true);
            }
            else if (option == Circle)
            {
                mapController.Extent.ExtentType = ExtentType.Circle;
                wrapper = mapEditor.Query<TextField>(name: ShapeConfiguratorX);
                wrapper.value = GetWidthValueString();
                wrapper.SetEnabled(true);
                wrapper = mapEditor.Query<TextField>(name: ShapeConfiguratorY);
                wrapper.SetEnabled(false);
            }
        }

        private void ResetExtent()
        {
            mapController.Extent.Latitude = 0;
            mapController.Extent.Longitude = 0;
            mapController.Extent.Altitude = 0;
            mapController.Extent.ExtentType = ExtentType.Square;
            mapController.Extent.Width = 0;
            mapController.Extent.Length = 0;

            latExtText.value = mapController.Extent.Latitude.ToString();
            lngExtText.value = mapController.Extent.Longitude.ToString();
            altitudeExtText.value = mapController.Extent.Altitude.ToString();
            altitudeExtSlider.value = (float)mapController.Extent.Altitude;
            shapeOptions.value = GetMapExtent();
            shapeConfigWidth.value = GetWidthValueString();
            shapeConfigLength.value = "Y";
        }

        private string GetWidthValueString()
        {
            return (mapController.Extent.Width == 0) ? GetDefaultWdithString() : mapController.Extent.Width.ToString();
        }

        private string GetDefaultWdithString()
        {
            if (mapController.Extent.ExtentType == ExtentType.Square || mapController.Extent.ExtentType == ExtentType.Rectangle)
            {
                return "X";
            }
            else
            {
                return "Radius";
            }
        }
    }
}

// ArcGISMapsSDK

using ArcGISMapsSDK.UX;

// Unity

using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace ArcGISMapsSDK.Editor
{
    public class CameraEditor
    {
        private const string CameraAltitudeSliderName = "slider-camera-altitude";
        private const string CameraHeadingSliderName = "slider-camera-heading";
        private const string CameraLatName = "lat-key";
        private const string CameraLngName = "lng-key";
        private const string CameraPitchSliderName = "slider-camera-pitch";
        private const string CameraRollSliderName = "slider-camera-roll";
        private const string EditorCameraSettingsStylesFileName = "CameraSettingsStyles";

        private VisualElement mapEditor;
        private ArcGISMapController mapController;

        public CameraEditor(VisualElement editor, ArcGISMapController controller)
        {
            mapEditor = editor;
            mapController = controller;

            var cameraSettingsStylesPath = MapControllerUtilities.FindAssetPath(EditorCameraSettingsStylesFileName);
            mapEditor.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(cameraSettingsStylesPath));
            
            InitializeCameraBox();
        }

        private void InitializeCameraBox()
        {
            TextField latText = mapEditor.Query<TextField>(name: (CameraLatName + "-text"));
            latText.value = mapController.CamLocation.Latitude.ToString();
            latText.RegisterValueChangedCallback(evnt =>
            {
                mapController.CamLocation.Latitude = Double.Parse(evnt.newValue);
                MapControllerUtilities.MarkDirty(mapController);
            });

            TextField lngText = mapEditor.Query<TextField>(name: (CameraLngName + "-text"));
            lngText.value = mapController.CamLocation.Longitude.ToString();
            lngText.RegisterValueChangedCallback(evnt =>
            {
                mapController.CamLocation.Longitude = Double.Parse(evnt.newValue);
                MapControllerUtilities.MarkDirty(mapController);
            });

            Slider altitudeSlider = MapControllerUtilities.CreateSlider(mapEditor, CameraAltitudeSliderName, mapController.CamLocation.Altitude);
            altitudeSlider.value = mapController.CamLocation.Altitude;
            TextField altitudeText = mapEditor.Query<TextField>(name: (CameraAltitudeSliderName + "-text"));
            altitudeText.value = mapController.CamLocation.Altitude.ToString() + "m";
            altitudeText.RegisterValueChangedCallback(evnt =>
            {
                mapController.CamLocation.Altitude = float.Parse(evnt.newValue.Replace("m", ""));
                altitudeSlider.SetValueWithoutNotify(float.Parse(evnt.newValue.Replace("m", "")));
                altitudeSlider.MarkDirtyRepaint();
                MapControllerUtilities.MarkDirty(mapController);
            });
            altitudeSlider.RegisterValueChangedCallback(evnt =>
            {
                mapController.CamLocation.Altitude = altitudeSlider.value;
                altitudeText.SetValueWithoutNotify(altitudeSlider.value.ToString() + "m");
                MapControllerUtilities.MarkDirty(mapController);
            });

            Slider headingSlider = MapControllerUtilities.CreateSlider(mapEditor, CameraHeadingSliderName, mapController.CamLocation.Heading);
            headingSlider.value = mapController.CamLocation.Heading;
            TextField headingText = mapEditor.Query<TextField>(name: CameraHeadingSliderName + "-text");
            headingText.value = mapController.CamLocation.Heading.ToString();
            headingText.RegisterValueChangedCallback(evnt =>
            {
                mapController.CamLocation.Heading = float.Parse(evnt.newValue);
                headingSlider.value = float.Parse(evnt.newValue);
                headingSlider.MarkDirtyRepaint();
                headingSlider.SetValueWithoutNotify(float.Parse(evnt.newValue));
                MapControllerUtilities.MarkDirty(mapController);
            });
            headingSlider.RegisterValueChangedCallback(evnt =>
            {
                mapController.CamLocation.Heading = headingSlider.value;
                headingText.SetValueWithoutNotify(headingSlider.value.ToString());
                MapControllerUtilities.MarkDirty(mapController);
            });

            Slider pitchSlider = MapControllerUtilities.CreateSlider(mapEditor, CameraPitchSliderName, mapController.CamLocation.Pitch);
            TextField pitchText = mapEditor.Query<TextField>(name: (CameraPitchSliderName + "-text"));
            pitchText.value = mapController.CamLocation.Pitch.ToString();
            pitchText.RegisterValueChangedCallback(evnt =>
            {
                mapController.CamLocation.Pitch = float.Parse(evnt.newValue);
                pitchSlider.value = float.Parse(evnt.newValue);
                pitchSlider.MarkDirtyRepaint();
                pitchSlider.SetValueWithoutNotify(float.Parse(evnt.newValue));
                MapControllerUtilities.MarkDirty(mapController);
            });
            pitchSlider.RegisterValueChangedCallback(evnt =>
            {
                mapController.CamLocation.Pitch = pitchSlider.value;
                pitchText.SetValueWithoutNotify(pitchSlider.value.ToString());
                MapControllerUtilities.MarkDirty(mapController);
            });

            Slider rollSlider = MapControllerUtilities.CreateSlider(mapEditor, CameraRollSliderName, mapController.CamLocation.Roll);
            TextField rollText = mapEditor.Query<TextField>(name: (CameraRollSliderName + "-text"));
            rollText.value = mapController.CamLocation.Roll.ToString();
            rollText.RegisterValueChangedCallback(evnt =>
            {
                mapController.CamLocation.Roll = float.Parse(evnt.newValue);
                rollSlider.value = float.Parse(evnt.newValue);
                rollSlider.MarkDirtyRepaint();
                rollSlider.SetValueWithoutNotify(float.Parse(evnt.newValue));
                MapControllerUtilities.MarkDirty(mapController);
            });
            rollSlider.RegisterValueChangedCallback(evnt =>
            {
                mapController.CamLocation.Roll = rollSlider.value;
                rollText.SetValueWithoutNotify(rollSlider.value.ToString());
                MapControllerUtilities.MarkDirty(mapController);
            });
        }
    }
}

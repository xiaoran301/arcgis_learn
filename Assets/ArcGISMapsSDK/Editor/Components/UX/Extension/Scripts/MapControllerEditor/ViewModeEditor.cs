// ArcGISMapsSDK

using ArcGISMapsSDK.UX;

// Esri

using Esri.GameEngine.Map;

// Unity

using UnityEditor;
using UnityEngine.UIElements;

namespace ArcGISMapsSDK.Editor
{
    public class ViewModeEditor
    {
        private const string EditorViewingModeStylesFileName = "ViewingModeStyles";
        private const string GlobalSceneButtonName = "viewing-mode-global-button";
        private const string LocalSceneButtonName = "viewing-mode-local-button";

        private VisualElement mapEditor;
        private ArcGISMapController mapController;
        private MapExtentEditor mapExtentEditor;

        private Button globalViewButton;
        private Button localViewButton;

        public ViewModeEditor(VisualElement editor, ArcGISMapController controller)
        {
            mapEditor = editor;
            mapController = controller;

            var viewModeStylesPath = MapControllerUtilities.FindAssetPath(EditorViewingModeStylesFileName);
            mapEditor.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(viewModeStylesPath));

            globalViewButton = mapEditor.Query<Button>(name: GlobalSceneButtonName);
            globalViewButton.RegisterCallback<MouseUpEvent>(envt =>
            {
                SetGlobal();
                MapControllerUtilities.MarkDirty(mapController);
            });

            localViewButton = mapEditor.Query<Button>(name: LocalSceneButtonName);
            localViewButton.RegisterCallback<MouseUpEvent>(envt =>
            {
                SetLocal();
                MapControllerUtilities.MarkDirty(mapController);
            });
        }

        public void SetMapExtentEditor(MapExtentEditor editor)
        {
            mapExtentEditor = editor;

            if (mapController.ViewMode == ArcGISMapType.Global)
            {
                SetGlobal();
            }
            else
            {
                SetLocal();
            }
        }

        private void SetGlobal()
        {
            mapController.ViewMode = ArcGISMapType.Global;
            globalViewButton.AddToClassList("viewing-mode-button-selected");
            localViewButton.RemoveFromClassList("viewing-mode-button-selected");
            mapExtentEditor.DisableFoldout();
        }

        private void SetLocal()
        {
            mapController.ViewMode = ArcGISMapType.Local;
            localViewButton.AddToClassList("viewing-mode-button-selected");
            globalViewButton.RemoveFromClassList("viewing-mode-button-selected");
            mapExtentEditor.EnableFoldout();
        }
    }
}

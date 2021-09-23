// ArcGISMapsSDK

using ArcGISMapsSDK.UX;

// Unity

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArcGISMapsSDK.Editor
{
    public class LayerEditor
    {
        private const string EditorLayerListStylesFileName = "LayerEditorStyles";
        private const string EditorLayerTabTemplateFileName = "LayerRowTemplate";
        private const string EditorLayerTemplateFileName = "LayerEditorTemplate";

        private const string LayerOpacityValueName = "layer-opacity-text";
        private const string LayerTableBody = "table-body";
        private const string LayerTextFieldName = "layer-name";
        private const string RemoveLayerButton = "delete-layer-toggle";
        private const string CopyButton = "copy-layer-toggle";
        private const string VisibilityButtonName = "layer-visibility-toggle";
        private const string MoveUpButtonName = "layer-moveup-toggle";
        private const string MoveDownButtonName = "layer-movedown-toggle";

        private ArcGISMapController mapController;
        private VisualTreeAsset layerEditorContent;
        private VisualElement mapEditor;
        private VisualElement layerTable;
        private VisualTreeAsset layerRowTemplate;

        public LayerEditor(VisualElement editor, ArcGISMapController controller)
        {
            mapController = controller;
            mapEditor = editor;

            var layerTemplatePath = MapControllerUtilities.FindAssetPath(EditorLayerTemplateFileName);
            layerEditorContent = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(layerTemplatePath);
            layerEditorContent.CloneTree(mapEditor);
            
            layerTable = mapEditor.Query<VisualElement>(name: LayerTableBody);

            var layerListStylesPath = MapControllerUtilities.FindAssetPath(EditorLayerListStylesFileName);
            mapEditor.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(layerListStylesPath));

            var layerTabTemplatePath = MapControllerUtilities.FindAssetPath(EditorLayerTabTemplateFileName);
            layerRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(layerTabTemplatePath);

            CreateAllMapLayerTabs();
        }

        public void CreateMapLayerTab(Layer layer)
        {
            TemplateContainer layerRow = layerRowTemplate.CloneTree();

            TextField layerName = layerRow.Query<TextField>(name: LayerTextFieldName);
            layerName.value = layer.Name;
            layerName.RegisterValueChangedCallback(evnt =>
            {
                layer.Name = evnt.newValue;
                MapControllerUtilities.MarkDirty(mapController);
            });

            TextField sliderText = layerRow.Query<TextField>(name: LayerOpacityValueName);
            sliderText.value = Convert.ToString(layer.Opacity) + "%";
            sliderText.RegisterValueChangedCallback(evnt =>
            {
                String tmpStr = evnt.newValue.Replace("%", "");
                float value = 0;
                if (float.TryParse(tmpStr, out value))
                {
                    layer.Opacity = value;
                }
                sliderText.value = Convert.ToString(layer.Opacity) + "%";
                MapControllerUtilities.MarkDirty(mapController);
            });

            Button visibilityButton = layerRow.Query<Button>(name: VisibilityButtonName);
            MapControllerUtilities.ToggleCheckbox(visibilityButton, layer.Visible);
            visibilityButton.RegisterCallback<MouseUpEvent>(envt =>
            {
                layer.Visible = !layer.Visible;
                MapControllerUtilities.ToggleCheckbox(visibilityButton, layer.Visible);
                MapControllerUtilities.MarkDirty(mapController);
            });

            Button moveUpButton = layerRow.Query<Button>(name: MoveUpButtonName);
            moveUpButton.RegisterCallback<MouseUpEvent>(evnt =>
            {
                MoveLayerUp(layer, layerRow);
                MapControllerUtilities.MarkDirty(mapController);
            });

            Button moveDownButton = layerRow.Query<Button>(name: MoveDownButtonName);
            moveDownButton.RegisterCallback<MouseUpEvent>(evnt =>
            {
                MoveLayerDown(layer, layerRow);
                MapControllerUtilities.MarkDirty(mapController);
            });

            Button copyButton = layerRow.Query<Button>(name: CopyButton);
            copyButton.RegisterCallback<MouseUpEvent>(evnt =>
            {
                EditorGUIUtility.systemCopyBuffer = layer.Source;
                Debug.Log("Layer Source: " + layer.Source);
            });

            Button removeLayerButton = layerRow.Query<Button>(name: RemoveLayerButton);
            removeLayerButton.RegisterCallback<MouseUpEvent>(evnt =>
            {
                layerTable.Remove(layerRow);
                mapController.Layers.Remove(layer);
                MapControllerUtilities.MarkDirty(mapController);
            });

            layerTable.Add(layerRow);
            layerTable.MarkDirtyRepaint();
        }

        private void CreateAllMapLayerTabs()
        {
            var mapLayers = mapController.Layers;
            for (int i = 0; i < mapLayers.Count; i++)
            {
                CreateMapLayerTab(mapLayers[i]);
            }
        }

        private void MoveLayerUp(Layer layer, TemplateContainer layerRow)
        {
            var index = mapController.Layers.IndexOf(layer);

            if (index == 0)
            {
                return;
            }

            RelocateLayer(layer, layerRow, index, index - 1);
        }

        private void MoveLayerDown(Layer layer, TemplateContainer layerRow)
        {
            var index = mapController.Layers.IndexOf(layer);

            if (index == mapController.Layers.Count - 1)
            {
                return;
            }

            RelocateLayer(layer, layerRow, index, index + 1);
        }

        private void RelocateLayer(Layer layer, VisualElement tab, int index, int destination)
        {
            mapController.Layers.Remove(layer);
            mapController.Layers.Insert(destination, layer);

            layerTable.RemoveAt(index);
            layerTable.Insert(destination, tab);
        }
    }
}

// ArcGISMapsSDK

using ArcGISMapsSDK.UX;

// Unity

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArcGISMapsSDK.Editor
{
    public class AddDataEditor
    {
        private const string AddNewLayerFromFile = "from-file-box-save";
        private const string ButtonFromFileName = "button-from-file";
        private const string ButtonFromUrlName = "button-from-url";
        private const string FromFileBoxName = "from-file-box";
        private const string FromUrlBoxName = "from-url-box";
        private const string AddDataSelectedName = "add-data-from-button-selected";
        private const string OpenDataDialogueName = "button-open-data-dialog";
        private const string ClearAddDataFormsButtonName = "button-clear-add-data-forms";
        private const string AddDataLayerNameFromFile = "layer-name-from-file";
        private const string AddDataLayerNameFromUrl = "layer-name-from-url";
        private const string AddDataLayerFilePath = "layer-file-path";
        private const string AddDataLayerFileUrl = "layer-url-from";
        private const string EditorAddDataStylesFileName = "AddDataStyles";

        private VisualElement mapEditor;
        private ArcGISMapController mapController;
        private LayerEditor layerEditor;

        private int currentAddDataTab = 1;

        public AddDataEditor(VisualElement editor, ArcGISMapController controller)
        {
            mapEditor = editor;
            mapController = controller;
            var addDataStylePath = MapControllerUtilities.FindAssetPath(EditorAddDataStylesFileName);
            mapEditor.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(addDataStylePath));
            Initialize();
        }

        public void SetLayerEditor(LayerEditor editor)
        {
            layerEditor = editor;
        }

        private void Initialize()
        {
            Button addNewLayerFromFileButton = mapEditor.Query<Button>(AddNewLayerFromFile);
            addNewLayerFromFileButton.RegisterCallback<MouseUpEvent>(envt =>
            {
                TextField layerName = (currentAddDataTab == 0) ? mapEditor.Query<TextField>(name: AddDataLayerNameFromFile) : mapEditor.Query<TextField>(name: AddDataLayerNameFromUrl);
                TextField path = (currentAddDataTab == 0) ? mapEditor.Query<TextField>(name: AddDataLayerFilePath) : mapEditor.Query<TextField>(name: AddDataLayerFileUrl);
                Layer layer = new Layer(layerName.value.Trim(), path.value.Trim(), true, 100);

                mapController.Layers.Add(layer);
                layerEditor.CreateMapLayerTab(layer);
                ClearAddDataForms();
                MapControllerUtilities.MarkDirty(mapController);
            });

            Button addNewLayerFromFileTab = mapEditor.Query<Button>(name: ButtonFromFileName);
            addNewLayerFromFileTab.RegisterCallback<MouseUpEvent>(envt =>
            {
                currentAddDataTab = 0;
                ClearActiveTab(ButtonFromUrlName);
                addNewLayerFromFileTab.AddToClassList(AddDataSelectedName);
                ToggleLayersFromBox(FromFileBoxName);
            });

            Button addNewLayerFromUrlTab = mapEditor.Query<Button>(name: ButtonFromUrlName);
            addNewLayerFromUrlTab.RegisterCallback<MouseUpEvent>(envt =>
            {
                currentAddDataTab = 1;
                ClearActiveTab(ButtonFromFileName);
                addNewLayerFromUrlTab.AddToClassList(AddDataSelectedName);
                ToggleLayersFromBox(FromUrlBoxName);
            });

            Button addNewLayerDialogButton = mapEditor.Query<Button>(name: OpenDataDialogueName);
            addNewLayerDialogButton.RegisterCallback<MouseUpEvent>(envt =>
            {
                string filePath = EditorUtility.OpenFilePanel("", Application.dataPath, "");
                if (filePath.Length != 0)
                {
                    TextField filePathLabel = mapEditor.Query<TextField>(name: AddDataLayerFilePath);
                    filePathLabel.value = filePath;
                }
            });

            Button clearFormsButton = mapEditor.Query<Button>(name: ClearAddDataFormsButtonName);
            clearFormsButton.RegisterCallback<MouseUpEvent>(evnt =>
            {
                ClearAddDataForms();
            });
        }

        private void ToggleLayersFromBox(string id)
        {
            Box boxWrapper;
            if (id != FromUrlBoxName)
            {
                boxWrapper = mapEditor.Query<Box>(name: FromUrlBoxName);
                boxWrapper.style.display = DisplayStyle.None;
            }
            else
            {
                boxWrapper = mapEditor.Query<Box>(name: FromFileBoxName);
                boxWrapper.style.display = DisplayStyle.None;
            }

            boxWrapper = mapEditor.Query<Box>(name: id);
            boxWrapper.style.display = DisplayStyle.Flex;
        }

        private void ClearActiveTab(string id)
        {
            Button button = mapEditor.Query<Button>(name: id);
            button.RemoveFromClassList(AddDataSelectedName);
            ClearAddDataForms();
        }

        private void ClearAddDataForms()
        {
            TextField toClear;
            toClear = mapEditor.Query<TextField>(name: AddDataLayerFileUrl);
            toClear.value = "";
            toClear = mapEditor.Query<TextField>(name: AddDataLayerNameFromUrl);
            toClear.value = "";
            toClear = mapEditor.Query<TextField>(name: AddDataLayerFilePath);
            toClear.value = "";
            toClear = mapEditor.Query<TextField>(name: AddDataLayerNameFromFile);
            toClear.value = "";
        }
    }
}

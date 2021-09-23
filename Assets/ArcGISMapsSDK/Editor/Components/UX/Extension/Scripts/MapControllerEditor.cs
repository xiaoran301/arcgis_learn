// ArcGISMapsSDK

using ArcGISMapsSDK.UX;

// Unity

using UnityEditor;
using UnityEngine.UIElements;

namespace ArcGISMapsSDK.Editor
{
	[CustomEditor(typeof(ArcGISMapController))]
	public class MapControllerEditor : UnityEditor.Editor
	{
		private const string EditorSavePrefabName = "PrefabSectionStyles";
		private const string EditorStylesFileName = "MapControllerStyles";
		private const string EditorTemplateFileName = "MapControllerTemplate";
		private const string PrefabSectionFileName = "PrefabSectionTemplate";
		private const string UpdatePrefabButtonName = "update-prefab-button";

		private ArcGISMapController mapController;
		private VisualElement mapEditor;

		private AddDataEditor addDataEditor;
		private LayerEditor layerEditor;
		private MapExtentEditor mapExtentEditor;
		private ViewModeEditor viewModeEditor;

		public override VisualElement CreateInspectorGUI()
		{
			mapController = target as ArcGISMapController;
			mapEditor = new VisualElement();

			var templatePath = MapControllerUtilities.FindAssetPath(EditorTemplateFileName);
			var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(templatePath);
			template.CloneTree(mapEditor);

			var styleSheet = MapControllerUtilities.FindAssetPath(EditorStylesFileName);
			mapEditor.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheet));

			viewModeEditor = new ViewModeEditor(mapEditor, mapController);
			new CameraEditor(mapEditor, mapController);
			mapExtentEditor = new MapExtentEditor(mapEditor, mapController);
			new BaseMapEditor(mapEditor, mapController);
			addDataEditor = new AddDataEditor(mapEditor, mapController);
			layerEditor = new LayerEditor(mapEditor, mapController);
			new AuthConfigEditor(mapEditor, mapController);

			viewModeEditor.SetMapExtentEditor(mapExtentEditor);
			addDataEditor.SetLayerEditor(layerEditor);

			if (PrefabUtility.GetPrefabAssetType(mapController.gameObject) != PrefabAssetType.NotAPrefab)
			{
				InitializeUpdatePrefabBox();
			}

			return mapEditor;
		}

		private void InitializeUpdatePrefabBox()
		{
			var prefabPath = MapControllerUtilities.FindAssetPath(PrefabSectionFileName);
			var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(prefabPath);
			template.CloneTree(mapEditor);

			var savePrefabPath = MapControllerUtilities.FindAssetPath(EditorSavePrefabName);
			mapEditor.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(savePrefabPath));

			Button savePrefabButton = mapEditor.Query<Button>(name: UpdatePrefabButtonName);
			savePrefabButton.RegisterCallback<MouseUpEvent>(envt =>
			{
				PrefabUtility.ApplyPrefabInstance(mapController.gameObject, InteractionMode.AutomatedAction);
			});
		}
	}
}

using ArcGISMapsSDK.Components;
using Esri.GameEngine.Camera;
using Esri.GameEngine.Extent;
using Esri.GameEngine.Location;
using Esri.GameEngine.View;
using Esri.GameEngine.View.Event;
using Esri.Unity;
using System;
using UnityEngine;

public class APIScene_NY : MonoBehaviour
{
	void Start()
	{
		// API Key
		string apiKey = ""; // "6b86b273ff34fce19d6b804eff5a3f5747ada4eaa22f1d49c01e52ddb7875b4b";

		var viewMode = Esri.GameEngine.Map.ArcGISMapType.Global;

		// Create the Map Document
		var arcGISMap = new Esri.GameEngine.Map.ArcGISMap(viewMode);

		// Set the Basemap
		arcGISMap.Basemap = new Esri.GameEngine.Map.ArcGISBasemap("https://www.arcgis.com/sharing/rest/content/items/8d569fbc4dc34f68abae8d72178cee05/data", apiKey);

		// Create the Elevation
		arcGISMap.Elevation = new Esri.GameEngine.Map.ArcGISMapElevation(new Esri.GameEngine.Elevation.ArcGISImageElevationSource("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer", "Elevation", apiKey));

		// Create layers
		var layer_1 = new Esri.GameEngine.Layers.ArcGISImageLayer("https://tiles.arcgis.com/tiles/nGt4QxSblgDfeJn9/arcgis/rest/services/UrbanObservatory_NYC_TransitFrequency/MapServer", "MyLayer_1", 1.0f, true, apiKey);
		arcGISMap.Layers.Add(layer_1);

		var layer_2 = new Esri.GameEngine.Layers.ArcGISImageLayer("https://tiles.arcgis.com/tiles/nGt4QxSblgDfeJn9/arcgis/rest/services/New_York_Industrial/MapServer", "MyLayer_2", 1.0f, true, apiKey);
		arcGISMap.Layers.Add(layer_2);

		var layer_3 = new Esri.GameEngine.Layers.ArcGIS3DModelLayer("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_NewYork_17/SceneServer", "MyLayer_4", 1.0f, true, apiKey);
		arcGISMap.Layers.Add(layer_3);

		var layer_4 = new Esri.GameEngine.Layers.ArcGISImageLayer("https://tiles.arcgis.com/tiles/4yjifSiIG17X0gW4/arcgis/rest/services/NewYorkCity_PopDensity/MapServer", "MyLayer_3", 1.0f, true, apiKey);
		arcGISMap.Layers.Add(layer_4);

		// Remove a layer
		arcGISMap.Layers.Remove(arcGISMap.Layers.IndexOf(layer_4));

		// Update properties
		layer_1.Opacity = 0.9f;
		layer_2.Opacity = 0.6f;
		layer_4.Opacity = 1.0f;

		if (viewMode == Esri.GameEngine.Map.ArcGISMapType.Local)
		{
			// Create Circle Extent

			var extentCenter = new ArcGISGlobalCoordinatesPosition(40.691242, -74.054921, 3000);
			var extent = new ArcGISExtentCircle(extentCenter, 100000);
			try
			{
				arcGISMap.ClippingArea = extent;

			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
			}
		}

		// Add Renderer Component
		ArcGISCameraComponent cameraGE = Camera.main.gameObject.AddComponent<ArcGISCameraComponent>();
		ArcGISCameraControllerComponent controller = Camera.main.gameObject.AddComponent<ArcGISCameraControllerComponent>();

		// Set initial position on CameraComponent will overwrite the camera position on the rendererview.
		var position = new ArcGISGlobalCoordinatesPosition(40.691242, -74.054921, 3000);
		var rotation = new ArcGISRotation(68, 0, 65);

		cameraGE.Latitude = position.Latitude;
		cameraGE.Longitude = position.Longitude;
		cameraGE.Height = position.Altitude;
		cameraGE.Heading = rotation.Heading;
		cameraGE.Pitch = rotation.Pitch;
		cameraGE.Roll = rotation.Roll;

		GameObject renderContainer = new GameObject("RenderContainer");
		ArcGISRendererComponent renderer = renderContainer.AddComponent<ArcGISRendererComponent>();

		// This position will be overwritten by the camera component
		ArcGISCamera camera = new ArcGISCamera("Camera", position, rotation);

		ArcGISRendererViewOptions options = new ArcGISRendererViewOptions();
		ArcGISRendererView rendererView = new ArcGISRendererView(arcGISMap, camera, options);


		// Adding events to show information on console
		rendererView.ArcGISElevationSourceViewStateChanged += (object sender, ArcGISElevationSourceViewStateEventArgs data) =>
		{
			Debug.Log("ArcGISElevationSourceViewState " + data.ArcGISElevationSource.Name + " changed to : " + data.Status.ToString());
		};

		rendererView.ArcGISVisualLayerViewStateChanged += (object sender, ArcGISVisualLayerViewStateEventArgs data) =>
		{
			Debug.Log("ArcGISVisualLayerViewState " + data.ArcGISVisualLayer.Name + " changed to : " + data.Status.ToString());
		};

		rendererView.ArcGISRendererViewStateChanged += (object sender, ArcGISRendererViewStateEventArgs data) =>
		{
			Debug.Log("ArcGISRendererViewState changed to : " + data.Status.ToString());
		};


		renderer.RendererView = rendererView;
		cameraGE.RendererView = rendererView;

#if !UNITY_ANDROID && !UNITY_IOS
		// Add Sky Component
		var currentSky = GameObject.FindObjectOfType<UnityEngine.Rendering.Volume>();
		if (currentSky)
		{
			ArcGISSkyRepositionComponent skyComponent = currentSky.gameObject.AddComponent<ArcGISSkyRepositionComponent>();
			skyComponent.CameraComponent = cameraGE;
			skyComponent.RendererComponent = renderer;
		}
#endif
	}
}

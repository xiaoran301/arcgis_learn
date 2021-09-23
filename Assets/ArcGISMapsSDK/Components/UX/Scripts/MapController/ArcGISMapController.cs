// ArcGISMapsSDK

using ArcGISMapsSDK.Components;
using ArcGISMapsSDK.UX.Security;

// Esri

using Esri.GameEngine.Camera;
using Esri.GameEngine.Elevation;
using Esri.GameEngine.Extent;
using Esri.GameEngine.Layers;
using Esri.GameEngine.Layers.Base;
using Esri.GameEngine.Location;
using Esri.GameEngine.Map;
using Esri.GameEngine.View;
using Esri.GameEngine.View.Event;
using Esri.Unity;

// .Net

using System;
using System.Collections.Generic;

// Unity

using UnityEngine;

namespace ArcGISMapsSDK.UX
{
	public enum BaseMapType
	{
		Imagery,
		Streets,
		Topographic,
		NatGeo,
		Oceans,
		LightGrayCanvas,
		DarkGrayCanvas,
	}

	public class ArcGISMapController : MonoBehaviour
	{
		public string APIKey;

		public CameraLocation CamLocation = new CameraLocation();

		public List<Layer> Layers = new List<Layer>();

		public ArcGISMapType ViewMode = ArcGISMapType.Global;

		public BaseMapType BaseMap;

		public Extent Extent = new Extent();

		public bool TerrainElevation = true;

		[SerializeReference]
		public List<OAuthAuthenticationConfiguration> Configurations = new List<OAuthAuthenticationConfiguration>();

		public List<OAuthAuthenticationConfigurationMapping> ConfigMappings = new List<OAuthAuthenticationConfigurationMapping>();

		private void Start()
		{
			CreateAuthConfigurations();

			var arcGISMap = new ArcGISMap(ViewMode);

			arcGISMap.Basemap = new ArcGISBasemap(GetBaseMapSource(), Esri.GameEngine.Layers.Base.ArcGISLayerType.ArcGISImageLayer, APIKey);

			if (TerrainElevation)
			{
				arcGISMap.Elevation = new ArcGISMapElevation(new ArcGISImageElevationSource("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer", "Elevation", APIKey));
			}

			foreach (Layer layer in Layers)
			{
				if (layer.Source.Contains("SceneServer") || layer.Source.Contains(".slpk"))
				{
					ArcGIS3DModelLayer modelLayer = new ArcGIS3DModelLayer(layer.Source, layer.Name, layer.Opacity / 100f, layer.Visible, APIKey);
					arcGISMap.Layers.Add(modelLayer);
					CheckLayerLoadStatus(modelLayer);
				}
				else
				{
					ArcGISImageLayer imageLayer = new ArcGISImageLayer(layer.Source, layer.Name, layer.Opacity / 100f, layer.Visible, APIKey);
					arcGISMap.Layers.Add(imageLayer);
					CheckLayerLoadStatus(imageLayer);
				}
			}

			if (ViewMode == ArcGISMapType.Local)
			{
				SetExtent(arcGISMap);
			}

			var cameraComponent = Camera.main.gameObject.AddComponent<ArcGISCameraComponent>();
			var controller = Camera.main.gameObject.AddComponent<ArcGISCameraControllerComponent>();
			var position = new ArcGISGlobalCoordinatesPosition(CamLocation.Latitude, CamLocation.Longitude, CamLocation.Altitude);
			var rotation = new ArcGISRotation(CamLocation.Pitch, CamLocation.Roll, CamLocation.Heading);

			cameraComponent.Latitude = position.Latitude;
			cameraComponent.Longitude = position.Longitude;
			cameraComponent.Height = position.Altitude;
			cameraComponent.Pitch = rotation.Pitch;
			cameraComponent.Roll = rotation.Roll;
			cameraComponent.Heading = rotation.Heading;

			var renderContainer = new GameObject("RenderContainer");
			var renderer = renderContainer.AddComponent<ArcGISRendererComponent>();
			var camera = new ArcGISCamera("Camera", position, rotation);
			var options = new ArcGISRendererViewOptions();
			var rendererView = new ArcGISRendererView(arcGISMap, camera, options);

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
			cameraComponent.RendererView = rendererView;

#if !UNITY_ANDROID && !UNITY_IOS
			// Add Sky Component
			var currentSky = GameObject.FindObjectOfType<UnityEngine.Rendering.Volume>();
			if (currentSky)
			{
				ArcGISSkyRepositionComponent skyComponent = currentSky.gameObject.AddComponent<ArcGISSkyRepositionComponent>();
				skyComponent.CameraComponent = cameraComponent;
				skyComponent.RendererComponent = renderer;
			}
#endif
		}

		private string GetBaseMapSource()
		{
			switch (BaseMap)
			{
				case BaseMapType.Imagery:
					return "https://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer";
				case BaseMapType.Streets:
					return "https://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
				case BaseMapType.Topographic:
					return "https://services.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer";
				case BaseMapType.NatGeo:
					return "https://services.arcgisonline.com/ArcGIS/rest/services/NatGeo_World_Map/MapServer";
				case BaseMapType.Oceans:
					return "https://services.arcgisonline.com/arcgis/rest/services/Ocean/World_Ocean_Base/MapServer";
				case BaseMapType.LightGrayCanvas:
					return "https://services.arcgisonline.com/arcgis/rest/services/Canvas/World_Light_Gray_Base/MapServer";
				case BaseMapType.DarkGrayCanvas:
					return "https://services.arcgisonline.com/arcgis/rest/services/Canvas/World_Dark_Gray_Base/MapServer";
				default:
					return "https://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer";
			}
		}

		private void CheckLayerLoadStatus(ArcGISVisualLayer layer)
		{
			layer.LoadStatusChanged = delegate (Esri.ArcGISRuntime.LoadStatus loadStatus)
			{
				if (loadStatus == Esri.ArcGISRuntime.LoadStatus.FailedToLoad)
				{
					var loadError = layer.LoadError;

					Debug.Log("Failed to load the ArcGISImageLayer: " + loadError.Message);
				}
			};
		}

		private void CreateAuthConfigurations()
		{
			Esri.GameEngine.Security.ArcGISAuthenticationManager.AuthenticationConfigurations.Clear();

			foreach (var config in Configurations)
			{
				Esri.GameEngine.Security.ArcGISAuthenticationConfiguration authenticationConfiguration;

				authenticationConfiguration = new Esri.GameEngine.Security.ArcGISOAuthAuthenticationConfiguration(config.ClientID, "", config.RedirectURI);

				foreach (var mapping in ConfigMappings)
				{
					if (mapping.Configuration == config)
					{
						Esri.GameEngine.Security.ArcGISAuthenticationManager.AuthenticationConfigurations.Add(mapping.ServiceURL, authenticationConfiguration);
					}
				}
			}
		}

		private void SetExtent(ArcGISMap arcGISMap)
		{
			if (Extent.Width == 0 || Extent.Length == 0)
			{
				Debug.LogWarning("An extent needs to have an area greater than zero");
			}
			try
			{
				ArcGISExtent extent;

				switch (Extent.ExtentType)
				{
					case ExtentType.Rectangle:
						extent = new ArcGISExtentRectangle(new ArcGISGlobalCoordinatesPosition(Extent.Latitude, Extent.Longitude, Extent.Altitude), Extent.Width, Extent.Length);
						break;
					case ExtentType.Square:
						extent = new ArcGISExtentRectangle(new ArcGISGlobalCoordinatesPosition(Extent.Latitude, Extent.Longitude, Extent.Altitude), Extent.Width, Extent.Width);
						break;
					case ExtentType.Circle:
						extent = new ArcGISExtentCircle(new ArcGISGlobalCoordinatesPosition(Extent.Latitude, Extent.Longitude, Extent.Altitude), Extent.Width);
						break;
					default:
						extent = new ArcGISExtentRectangle(new ArcGISGlobalCoordinatesPosition(Extent.Latitude, Extent.Longitude, Extent.Altitude), Extent.Width, Extent.Width);
						break;
				}

				arcGISMap.ClippingArea = extent;
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
			}
		}
	}
}

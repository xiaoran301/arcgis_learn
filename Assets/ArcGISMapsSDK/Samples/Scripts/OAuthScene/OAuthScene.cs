using ArcGISMapsSDK.Components;
using Esri.GameEngine.Camera;
using Esri.GameEngine.Location;
using Esri.GameEngine.View;
using Esri.GameEngine.View.Event;
using Esri.Unity;
using UnityEngine;

public class OAuthScene : MonoBehaviour
{
	private Esri.GameEngine.Layers.ArcGISImageLayer imageLayer;
	private Esri.ArcGISMapsSDK.Security.OAuthAuthenticationChallengeHandler oauthAuthenticationChallengeHandler;
	private ArcGISRendererComponent rendererComponent;

	public string clientID = "Enter Client ID";
	public string redirectURI = "Enter Redirect URI";
	public string serviceURL = "Enter Service URL";

	void Awake()
	{
		var renderContainer = new GameObject("RenderContainer");

		rendererComponent = renderContainer.AddComponent<ArcGISRendererComponent>();
	}

	void Start()
	{
#if (UNITY_ANDROID || UNITY_IOS || UNITY_WSA) && !UNITY_EDITOR
		oauthAuthenticationChallengeHandler = new MobileOAuthAuthenticationChallengeHandler();
#else
		oauthAuthenticationChallengeHandler = new DesktopOAuthAuthenticationChallengeHandler();
#endif

		Esri.ArcGISMapsSDK.Security.AuthenticationChallengeManager.OAuthChallengeHandler = oauthAuthenticationChallengeHandler;

		Esri.GameEngine.Security.ArcGISAuthenticationManager.AuthenticationConfigurations.Clear();

		Esri.GameEngine.Security.ArcGISAuthenticationConfiguration authenticationConfiguration;

		// Named user login
		authenticationConfiguration = new Esri.GameEngine.Security.ArcGISOAuthAuthenticationConfiguration(clientID, "", redirectURI);

		Esri.GameEngine.Security.ArcGISAuthenticationManager.AuthenticationConfigurations.Add(serviceURL, authenticationConfiguration);

		// Create a map
		var arcGISMap = new Esri.GameEngine.Map.ArcGISMap(Esri.GameEngine.Map.ArcGISMapType.Global);

		// Create and set a basemap
		arcGISMap.Basemap = new Esri.GameEngine.Map.ArcGISBasemap("https://www.arcgis.com/sharing/rest/content/items/716b600dbbac433faa4bec9220c76b3a/data", "");

		// Create and set an elevation layer
		arcGISMap.Elevation = new Esri.GameEngine.Map.ArcGISMapElevation(new Esri.GameEngine.Elevation.ArcGISImageElevationSource("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer", "Elevation", ""));

		// Create and set an OAuth-protected image layer
		imageLayer = new Esri.GameEngine.Layers.ArcGISImageLayer(serviceURL, "MyLayer_1", 1.0f, true, "");
		arcGISMap.Layers.Add(imageLayer);

		imageLayer.LoadStatusChanged = delegate (Esri.ArcGISRuntime.LoadStatus loadStatus)
		{
			if (loadStatus == Esri.ArcGISRuntime.LoadStatus.FailedToLoad)
			{
				var loadError = imageLayer.LoadError;

				Debug.Log("Failed to load the ArcGISImageLayer: " + loadError.Message);
			}
		};

		var position = new ArcGISGlobalCoordinatesPosition(37.0902, -95.7129, 1400000);
		var rotation = new ArcGISRotation(0, 0, 0);

		// Create a camera
		var camera = new ArcGISCamera("Camera", position, rotation);

		// Create the renderer view and set it the camera and the map
		var options = new ArcGISRendererViewOptions { LoadDataFromInvisibleLayers = false };
		var rendererView = new ArcGISRendererView(arcGISMap, camera, options);

		// Set the renderer view to the renderer component
		rendererComponent.RendererView = rendererView;

		var cameraComponent = Camera.main.gameObject.AddComponent<ArcGISCameraComponent>();
		var controller = Camera.main.gameObject.AddComponent<ArcGISCameraControllerComponent>();

		// TODO: find a better way to do this
		cameraComponent.Latitude = position.Latitude;
		cameraComponent.Longitude = position.Longitude;
		cameraComponent.Height = position.Altitude;
		cameraComponent.Heading = rotation.Heading;
		cameraComponent.Pitch = rotation.Pitch;
		cameraComponent.Roll = rotation.Roll;


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

		// Set the renderer view to the camera component
		cameraComponent.RendererView = rendererView;

#if !UNITY_ANDROID && !UNITY_IOS
		// Add Sky Component
		var currentSky = FindObjectOfType<UnityEngine.Rendering.Volume>();

		if (currentSky)
		{
			ArcGISSkyRepositionComponent skyComponent = currentSky.gameObject.AddComponent<ArcGISSkyRepositionComponent>();
			skyComponent.CameraComponent = cameraComponent;
			skyComponent.RendererComponent = rendererComponent;
		}
#endif
	}

	void OnDestroy()
	{
		if (oauthAuthenticationChallengeHandler != null)
		{
			oauthAuthenticationChallengeHandler.Dispose();
		}
	}
}

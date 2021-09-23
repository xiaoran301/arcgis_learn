using ArcGISMapsSDK.Core.SceneComponentProviders;
using ArcGISMapsSDK.Core.Utils;
using ArcGISMapsSDK.Core.Utils.Graphics;
using ArcGISMapsSDK.Core.Utils.Math;
using Esri.Core.Renderer;
using Esri.Core.SceneComponentProviders;
using Esri.Core.Utils;
using Esri.Core.Utils.GeoCoord;
using Esri.Core.Utils.Graphics;
using Esri.Core.Utils.Math;
using Esri.GameEngine.View;
using Esri.GameEngine.View.Event;
using Esri.GameEngine.Location;
using UnityEngine;
using System.Collections.Generic;

namespace ArcGISMapsSDK.Components
{
	public class ArcGISRendererComponent : MonoBehaviour
	{
		private ArcGISRenderer arcGISrenderer = null;
		private ISceneComponentProvider sceneComponentProvider = null;
		private ArcGISRendererView view = null;

		public Scene RendererScene = null;

		private Vector3d localOrigin = Vector3d.zero;
		private Quaterniond localRotation = Quaterniond.Identity;
		private double localScale = GeoUtils.EarthRadius;

		private List<ArcGISLocationComponent> locationComponents = new List<ArcGISLocationComponent>();

		public ArcGISRendererView RendererView
		{
			get
			{
				return view;
			}

			set
			{
				view = value;
#if UNITY_ANDROID
				view.SetSystemServices(new UnityAndroidSystemServices());
#else
				view.SetSystemServices(new UnitySystemServices());
#endif

				view.Start();

				IGPUResourcesProvider gpuProvider = new GPUResourcesProvider();
				INormalMapComputer normalMapComputer;
				IImageBlender imageBlender;

				if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 ||
					SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Metal)
				{
					normalMapComputer = new NormalMapComputerPS();
					imageBlender = new ImageBlenderPS();
				}
				else
				{
					normalMapComputer = new NormalMapComputerCS();
					imageBlender = new ImageBlenderCS();
				}

				RendererScene = new Scene(value.Map.MapType);
				arcGISrenderer = new ArcGISRenderer(gpuProvider, imageBlender, normalMapComputer, sceneComponentProvider, value);
			}
		}

		void Awake()
		{
			sceneComponentProvider = new SceneComponentProvider(500, gameObject);
			Application.lowMemory += OnLowMemoryCallback;
		}

		private void Start()
		{
			var locationComponentArray = FindObjectsOfType<ArcGISLocationComponent>();
			locationComponents.AddRange(locationComponentArray);
		}

		void LateUpdate()
		{
			if (arcGISrenderer != null && RendererView != null)
			{
				RendererView.ChangeViewport((uint)Screen.currentResolution.width, (uint)Screen.currentResolution.height);
				UpdateLocalTransform();

				sceneComponentProvider.Update(localRotation, localScale);
				RepositionLocationGameObjects();

				arcGISrenderer.Update(localOrigin, localRotation);
				view.Update();

				RendererView.Camera.Dirty = false;
			}
		}

		private void RepositionLocationGameObjects()
		{
			if (RendererView.Camera.Dirty)
			{
				foreach (var locationComponent in locationComponents)
				{
					var pos = RendererScene.ToGameEngineCoord(localRotation, localOrigin, new ArcGISGlobalCoordinatesPosition(locationComponent.Latitude, locationComponent.Longitude, locationComponent.Altitude));
					var rotation = RendererScene.ToGameEngineRotation(localOrigin, new ArcGISGlobalCoordinatesPosition(locationComponent.Latitude, locationComponent.Longitude, locationComponent.Altitude),
																	new Esri.GameEngine.Location.ArcGISRotation(locationComponent.Pitch, locationComponent.Roll, locationComponent.Heading));
					locationComponent.transform.position = pos.ToVector3();
					locationComponent.transform.rotation = rotation.ToQuaternion();
				}
			}
		}

		private void OnDestroy()
		{
			if (arcGISrenderer != null)
			{
				arcGISrenderer.Destroy();
				Application.lowMemory -= OnLowMemoryCallback;
			}
		}

		private void UpdateLocalTransform()
		{
			if (RendererView.Camera.Dirty)
			{
				localOrigin = RendererScene.ToCartesianCoord(RendererView.Camera.TransformationMatrix, false);
				localRotation = RendererScene.GetLocalRotation(localOrigin);
				localScale = RendererScene.GetLocalScale();
			}
		}

		private void OnLowMemoryCallback()
		{
			if (view != null)
			{
				view.HandleLowMemoryWarning();
			}
		}
	}
}

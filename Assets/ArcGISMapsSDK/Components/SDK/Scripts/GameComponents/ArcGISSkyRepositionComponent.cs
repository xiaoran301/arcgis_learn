using Esri.Core.Utils.GeoCoord;
using Esri.Core.Utils.Math;
using UnityEngine;
#if !UNITY_ANDROID && !UNITY_IOS
using UnityEngine.Rendering;
#if USE_HDRP_PACKAGE
using UnityEngine.Rendering.HighDefinition;
#endif
#endif

namespace ArcGISMapsSDK.Components
{
	public class ArcGISSkyRepositionComponent : MonoBehaviour
	{
		private Vector3d localOrigin = Vector3d.Zero;
		private double localScale = 1;

#if !UNITY_ANDROID && !UNITY_IOS && USE_HDRP_PACKAGE
		private PhysicallyBasedSky sky = null;
		private Fog fog = null;
#endif

		public ArcGISCameraComponent CameraComponent = null;
		public ArcGISRendererComponent RendererComponent = null;

		void Start()
		{
			if (CameraComponent != null && RendererComponent != null && RendererComponent.RendererScene != null)
			{			
#if !UNITY_ANDROID && !UNITY_IOS && USE_HDRP_PACKAGE
				if (GameObject.FindObjectOfType<Volume>())
				{
					Volume volume = GameObject.FindObjectOfType<Volume>();

					if (volume.profile.TryGet(out PhysicallyBasedSky tmpSky))
					{
						sky = tmpSky;
					}

					if (volume.profile.TryGet(out Fog tmpFog))
					{
						fog = tmpFog;
					}
				}
#endif

				UpdateSky();
			}
			else
			{
				Debug.LogError("ArcGISRendererComponent or ArcGISCameraComponent don't exist in the scene or RendererView doesn't set in ArcGISRendererComponent");
			}
		}

		void LateUpdate()
		{
			UpdateSky();
		}

		private void UpdateSky()
		{
			if (CameraComponent != null && RendererComponent != null && RendererComponent.RendererScene != null)
			{
				var currentLocalOrigin = RendererComponent.RendererScene.ToCartesianCoord(RendererComponent.RendererView.Camera.TransformationMatrix, false);
				var currentLocalScale = RendererComponent.RendererScene.GetLocalScale();

				if (localOrigin != currentLocalOrigin || localScale != currentLocalScale)
				{
					localOrigin = currentLocalOrigin;
					localScale = currentLocalScale;

					if (RendererComponent.RendererScene.MapType == Esri.GameEngine.Map.ArcGISMapType.Local)
					{
#if !UNITY_ANDROID && !UNITY_IOS && USE_HDRP_PACKAGE
						if (sky != null)
						{
							sky.sphericalMode.value = false;
							sky.planetaryRadius.value = 7e8f;
							sky.planetCenterPosition.value = new Vector3(0, (float)-(7e8 + localOrigin.y), 0);
						}
						if (fog != null && fog.enabled.value)
						{
							double height = localOrigin.y;
							fog.meanFreePath.value = (float)height + (float)CameraComponent.Far * 0.15f;
						}
#endif
					}
					else
					{
						double height = localOrigin.Length() - GeoUtils.EarthRadius;
						var radius = -(CameraComponent.WorldScale + GeoUtils.EarthScaleToUnityScale(height, false, CameraComponent.WorldScale));

#if !UNITY_ANDROID && !UNITY_IOS && USE_HDRP_PACKAGE
						if (sky != null)
						{
							sky.planetCenterPosition.value = new Vector3(0, (float)radius, 0);
						}
						if (fog != null && fog.enabled.value)
						{
							fog.baseHeight.value = -(float)GeoUtils.EarthScaleToUnityScale(height, false, CameraComponent.WorldScale);
							fog.maximumHeight.value = 10000;
							fog.meanFreePath.value = (float)height + (float)CameraComponent.Far * 0.15f;
						}
#endif
					}
				}
			}
		}
	}
}

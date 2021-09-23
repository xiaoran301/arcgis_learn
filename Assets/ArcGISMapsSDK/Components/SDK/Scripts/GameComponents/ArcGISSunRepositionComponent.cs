using UnityEngine;
using ArcGISMapsSDK.Core.Utils.Math;

namespace ArcGISMapsSDK.Components
{
	public class ArcGISSunRepositionComponent : MonoBehaviour
	{
		public float SunAngleFromHorizont = 0.0f;

		private Quaternion localRotation = Quaternion.identity;

		public ArcGISRendererComponent RendererComponent = null;

		void Start()
		{
			var lightComponent = gameObject.GetComponent<Light>();

			if (lightComponent == null || lightComponent.type != LightType.Directional)
			{
				Debug.LogError("ArcGISSkyRepositionComponent must be attached to a directional light");
			}

			UpdateSun();
		}

		void LateUpdate()
		{
			UpdateSun();
		}

		private void UpdateSun()
		{
			var currentLocalOrigin = RendererComponent.RendererScene.ToCartesianCoord(RendererComponent.RendererView.Camera.TransformationMatrix, false);
			var currentLocalRotation = RendererComponent.RendererScene.GetLocalRotation(currentLocalOrigin).ToQuaternion();

			if (RendererComponent != null && localRotation.Equals(currentLocalRotation))
			{
				localRotation = currentLocalRotation;
				transform.rotation = currentLocalRotation * Quaternion.Euler(0, SunAngleFromHorizont, 0) * Quaternion.Euler(25.0f, 180, 0);
			}
		}
	}
}

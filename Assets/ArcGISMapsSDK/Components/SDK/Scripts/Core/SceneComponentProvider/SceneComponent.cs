using ArcGISMapsSDK.Core.Utils.Math;
using Esri.Core.Renderer;
using Esri.Core.Utils.Math;
using Esri.Core.SceneComponentProviders;
using UnityEngine;
using ArcGISMapsSDK.Core.Renderer;

namespace ArcGISMapsSDK.Core.SceneComponentProviders
{
	public class SceneComponent : ISceneComponent
	{
		private Vector3d pivot;
		public GameObject SceneComponentGameObject { get; }

		public SceneComponent(GameObject gameObject)
		{
			SceneComponentGameObject = gameObject;
		}

		public void SetData(GPUData gpuData)
		{
			SceneComponentGameObject.name = gpuData.Name;

			pivot = gpuData.Pivot;

			var renderer = SceneComponentGameObject.GetComponent<MeshRenderer>();
			renderer.material = ((MaterialGPUData)gpuData.Material).NativeMaterial;
			renderer.enabled = true;

			SceneComponentGameObject.GetComponent<MeshFilter>().mesh = ((MeshGPUData)gpuData.Mesh).NativeMesh;
		}

		public void SetTransform(Vector3d localOrigin, Quaterniond localRotation, double localScale)
		{
			var fLocalRotation = localRotation.ToQuaternion();
			SceneComponentGameObject.transform.position = (float)localScale * (fLocalRotation * (pivot - localOrigin).ToVector3());
			SceneComponentGameObject.transform.position *= 1.0f + 0.000001f * UnityEngine.Random.Range(0.0f, 1.0f);
		}

		public void SetVisible(bool enabled)
		{
			SceneComponentGameObject.GetComponent<MeshRenderer>().enabled = enabled;
		}
	}
}

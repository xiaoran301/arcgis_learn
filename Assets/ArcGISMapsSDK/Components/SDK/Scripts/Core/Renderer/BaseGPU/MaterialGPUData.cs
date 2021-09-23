using Esri.Core.Renderer;
using Esri.Core.Utils.Math;
using ArcGISMapsSDK.Core.Utils.Math;
using UnityEngine;

namespace ArcGISMapsSDK.Core.Renderer
{
	public class MaterialGPUData : IMaterialGPUData
	{
		public Material NativeMaterial { get; }

		public MaterialGPUData(Material material)
		{
			NativeMaterial = material;
		}

		public void Destroy()
		{
			Object.Destroy(NativeMaterial);
		}

		public void SetTexture(string name, ITexture2DGPUData value)
		{
			var nativeTexture = (Texture2DGPUData)value;

			if (nativeTexture != null)
			{
				NativeMaterial.SetTexture(name, nativeTexture.NativeTexture);
			}
		}

		public void SetTexture(string name, IRenderTextureGPUData value)
		{
			var nativeTexture = (RenderTextureGPUData)value;

			if (nativeTexture != null)
			{
				NativeMaterial.SetTexture(name, nativeTexture.NativeRenderTexture);
			}
		}

		public void SetFloat(string name, float value)
		{
			NativeMaterial.SetFloat(name, value);
		}

		public void SetInt(string name, int value)
		{
			NativeMaterial.SetInt(name, value);
		}

		public void SetVector(string name, Vector4f value)
		{
			NativeMaterial.SetVector(name, value.ToVector4());
		}

		public void SetVector(string name, Vector3f value)
		{
			NativeMaterial.SetVector(name, value.ToVector3());
		}
	}
}

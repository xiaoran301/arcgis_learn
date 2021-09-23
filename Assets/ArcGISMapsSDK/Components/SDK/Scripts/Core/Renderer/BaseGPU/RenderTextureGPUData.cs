using Esri.Core.Renderer;
using UnityEngine;

namespace ArcGISMapsSDK.Core.Renderer
{
	public class RenderTextureGPUData : IRenderTextureGPUData
	{
		public int Width
		{
			get
			{
				return NativeRenderTexture.width;
			}
		}

		public int Height
		{
			get
			{
				return NativeRenderTexture.height;
			}
		}

		public RenderTexture NativeRenderTexture { get; }

		public RenderTextureGPUData(RenderTexture texture)
		{
			NativeRenderTexture = texture;
		}

		public void GenerateMips()
		{
			NativeRenderTexture.GenerateMips();
		}

		public void Release()
		{
			NativeRenderTexture.Release();
		}

		public void Destroy()
		{
			Object.Destroy(NativeRenderTexture);
		}
	}
}

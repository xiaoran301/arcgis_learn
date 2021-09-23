using Esri.Core.Renderer;
using UnityEngine;

namespace ArcGISMapsSDK.Core.Renderer
{
	public class Texture2DGPUData : ITexture2DGPUData
	{
		public int Width
		{
			get
			{
				return NativeTexture.width;
			}
		}

		public int Height
		{
			get
			{
				return NativeTexture.height;
			}
		}

		public Texture2D NativeTexture { get; }

		public Texture2DGPUData(Texture2D texture)
		{
			NativeTexture = texture;
		}

		public void Destroy()
		{
			GameObject.Destroy(NativeTexture);
		}
	}
}

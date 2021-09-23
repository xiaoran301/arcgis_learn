using ArcGISMapsSDK.Core.Renderer;
using ArcGISMapsSDK.Core.Utils.Math;
using Esri.Core.Renderer;
using Esri.Core.Utils.Graphics;
using System.Collections.Generic;
using UnityEngine;

namespace ArcGISMapsSDK.Core.Utils.Graphics
{
	public class ImageBlenderPS : IImageBlender
	{
		private readonly Material material = null;

		public ImageBlenderPS()
		{
			material = new Material(Resources.Load<Shader>("Shaders/Utils/PS/BlendImage"));
		}

		public void Blend(SortedList<int, ImageBlenderInput> input, IRenderTextureGPUData output)
		{
			int numIterations = input.Count / 8 + (input.Count % 8 == 0 ? 0 : 1);
			float[] opacities = new float[8];
			Vector4[] offsets = new Vector4[8];

			for (int i = 0; i < numIterations; i++)
			{
				int numTexturesPerIteration = i == numIterations - 1 ? input.Count % 8 : 8;

				for (int tex = 0; tex < 8; tex++)
				{
					var texture = Texture2D.blackTexture;

					if (tex < numTexturesPerIteration)
					{
						texture = ((Texture2DGPUData)input.Values[(8 * i + tex)].image).NativeTexture;

						opacities[tex] = input.Values[(8 * i + tex)].opacity;
						offsets[tex] = input.Values[(8 * i + tex)].offsetAndScale.ToVector4();
					}

					material.SetTexture("Input" + (8 * i + tex), texture);
				}

				material.SetFloatArray("Opacities", opacities);
				material.SetVectorArray("OffsetsAndScales", offsets);
				material.SetInt("NumTextures", numTexturesPerIteration);

				UnityEngine.Graphics.Blit(null, ((RenderTextureGPUData)output).NativeRenderTexture, material);
			}
		}
	}
}

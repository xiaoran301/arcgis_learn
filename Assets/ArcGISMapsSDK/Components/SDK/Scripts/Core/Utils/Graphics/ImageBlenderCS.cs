using ArcGISMapsSDK.Core.Renderer;
using ArcGISMapsSDK.Core.Utils.Math;
using Esri.Core.Renderer;
using Esri.Core.Utils.Graphics;
using UnityEngine;
using System.Collections.Generic;

namespace ArcGISMapsSDK.Core.Utils.Graphics
{
	public class ImageBlenderCS : IImageBlender
	{
		private readonly ComputeShader shader = null;

		public ImageBlenderCS()
		{
			shader = Resources.Load<ComputeShader>("Shaders/Utils/CS/BlendImage");
		}

		public void Blend(SortedList<int, ImageBlenderInput> input, IRenderTextureGPUData output)
		{
			int kernelHandle = shader.FindKernel("CSMain");

			uint x, y, z;
			shader.GetKernelThreadGroupSizes(kernelHandle, out x, out y, out z);

			shader.SetTexture(kernelHandle, "Output", ((RenderTextureGPUData)output).NativeRenderTexture);

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

#if (UNITY_EDITOR_OSX || UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR_WIN
					shader.SetTexture(kernelHandle, "Input_" + (8 * i + tex) + "_", texture);
#else
					shader.SetTexture(kernelHandle, "Input[" + (8 * i + tex) + "]", texture);
#endif
				}

				shader.SetFloats("Opacities", opacities);
				shader.SetVectorArray("OffsetsAndScales", offsets);
				shader.SetInt("NumTextures", numTexturesPerIteration);

				shader.Dispatch(kernelHandle, (int)System.Math.Ceiling(output.Width / (float)x), (int)System.Math.Ceiling(output.Height / (float)y), 1);
			}
		}
	}
}

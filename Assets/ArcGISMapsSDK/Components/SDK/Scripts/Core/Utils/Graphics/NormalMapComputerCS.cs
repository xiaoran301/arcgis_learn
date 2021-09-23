using ArcGISMapsSDK.Core.Renderer;
using Esri.Core.Renderer;
using Esri.Core.Utils;
using Esri.Core.Utils.GeoCoord;
using Esri.Core.Utils.Graphics;
using Esri.Core.Utils.Math;
using UnityEngine;

namespace ArcGISMapsSDK.Core.Utils.Graphics
{
	public class NormalMapComputerCS : INormalMapComputer
	{
		private readonly ComputeShader shader = null;

		public NormalMapComputerCS()
		{
			shader = Resources.Load<ComputeShader>("Shaders/Utils/CS/ComputeNormals");
		}

		public void Compute(ITexture2DGPUData input, TileKey tileKey, IRenderTextureGPUData output)
		{
			var min = GeoUtils.TileToCoordinateWebMercator(tileKey.X, tileKey.Y, tileKey.Lod);
			var max = GeoUtils.TileToCoordinateWebMercator(tileKey.X + 1, tileKey.Y + 1, tileKey.Lod);

			var circleLongitude = 2.0 * System.Math.PI * GeoUtils.EarthRadius;
			double minLatitude = min.Latitude * MathConstants.DegreesToRadians;
			double latitudeAngleDelta = ((max.Latitude - min.Latitude) / output.Height) * MathConstants.DegreesToRadians;
			double longitudeArc = (System.Math.Abs(max.Latitude - min.Latitude) / 360.0);

			double latitudeLength = circleLongitude * (System.Math.Abs(max.Longitude - min.Longitude) / 360.0);

			int kernelHandle = shader.FindKernel("CSMain");

			uint x, y, z;
			shader.GetKernelThreadGroupSizes(kernelHandle, out x, out y, out z);

			shader.SetTexture(kernelHandle, "Output", ((RenderTextureGPUData)output).NativeRenderTexture);
			shader.SetTexture(kernelHandle, "Input", ((Texture2DGPUData)input).NativeTexture);
			shader.SetFloat("MinLatitude", (float)minLatitude);
			shader.SetFloat("LatitudeAngleDelta", (float)latitudeAngleDelta);
			shader.SetFloat("LongitudeArc", (float)longitudeArc);
			shader.SetFloat("LatitudeLength", (float)latitudeLength);
			shader.SetFloat("CircleLongitude", (float)circleLongitude);
			shader.SetFloat("EarthRadius", (float)GeoUtils.EarthRadius);

			shader.Dispatch(kernelHandle, (int)System.Math.Ceiling(output.Width / (float)x), (int)System.Math.Ceiling(output.Height / (float)y), 1);
		}
	}
}

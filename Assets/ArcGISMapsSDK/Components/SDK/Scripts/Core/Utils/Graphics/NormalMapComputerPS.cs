using ArcGISMapsSDK.Core.Renderer;
using Esri.Core.Renderer;
using Esri.Core.Utils;
using Esri.Core.Utils.GeoCoord;
using Esri.Core.Utils.Graphics;
using Esri.Core.Utils.Math;
using UnityEngine;

namespace ArcGISMapsSDK.Core.Utils.Graphics
{
	public class NormalMapComputerPS : INormalMapComputer
	{
		private readonly Material material = null;

		public NormalMapComputerPS()
		{
			material = new Material(Resources.Load<Shader>("Shaders/Utils/PS/ComputeNormals"));
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

			material.SetTexture("Input", ((Texture2DGPUData)input).NativeTexture);
			material.SetFloat("MinLatitude", (float)minLatitude);
			material.SetFloat("LatitudeAngleDelta", (float)latitudeAngleDelta);
			material.SetFloat("LongitudeArc", (float)longitudeArc);
			material.SetFloat("LatitudeLength", (float)latitudeLength);
			material.SetFloat("CircleLongitude", (float)circleLongitude);
			material.SetFloat("EarthRadius", (float)GeoUtils.EarthRadius);

			UnityEngine.Graphics.Blit(null, ((RenderTextureGPUData)output).NativeRenderTexture, material);
		}
	}
}

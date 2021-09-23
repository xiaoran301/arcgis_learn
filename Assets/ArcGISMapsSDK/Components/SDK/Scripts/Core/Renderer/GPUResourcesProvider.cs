using ArcGISMapsSDK.Core.Renderer;
using Esri.Core.DataProvider;
using Esri.Core.Renderer;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class GPUResourcesProvider : IGPUResourcesProvider
{
	private static Dictionary<Esri.GameEngine.TextureFormat, TextureFormat> EsriToUnityTextureFormat = new Dictionary<Esri.GameEngine.TextureFormat, TextureFormat>
	{
		{ Esri.GameEngine.TextureFormat.AliasRGB8, TextureFormat.RGB24 },
		{ Esri.GameEngine.TextureFormat.AliasFloat, TextureFormat.RFloat },
		{ Esri.GameEngine.TextureFormat.AliasRGBA16, TextureFormat.RGBAHalf },
		{ Esri.GameEngine.TextureFormat.AliasRGBA8, TextureFormat.RGBA32 },
		{ Esri.GameEngine.TextureFormat.Dxt1, TextureFormat.DXT1 },
		{ Esri.GameEngine.TextureFormat.Dxt5, TextureFormat.DXT5 }
	};

	public IMaterialGPUData GetMaterial(string shaderPath)
	{
		string basePath = "Shaders/Materials";

		Material material = null;

#if (!UNITY_ANDROID && !UNITY_IOS) || UNITY_EDITOR
		if (GraphicsSettings.renderPipelineAsset != null)
		{
			var renderType = GraphicsSettings.renderPipelineAsset.GetType().ToString();

			if (renderType == "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset")
			{
				material = new Material(Resources.Load<Shader>(basePath + "/URP/" + shaderPath));
			}
			else if (renderType == "UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset")
			{
				material = new Material(Resources.Load<Shader>(basePath + "/HDRP/" + shaderPath));
			}
		}
		else
		{
			material = new Material(Resources.Load<Shader>(basePath + "/Legacy/" + shaderPath));
		}
#else
		if (GraphicsSettings.renderPipelineAsset != null)
		{
			var renderType = GraphicsSettings.renderPipelineAsset.GetType().ToString();

			if (renderType == "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset")
			{
				material = new Material(Resources.Load<Shader>(basePath + "/URP/" + shaderPath));
			}
		}
		else
		{
			material = new Material(Resources.Load<Shader>(basePath + "/Legacy/" + shaderPath));
		}
#endif

		return new MaterialGPUData(material);
	}

	public IMeshGPUData GetMesh()
	{
		return new MeshGPUData(new Mesh());
	}

	public ITexture2DGPUData CreateTextureFrom(TextureCPUData textureData)
	{
		if (EsriToUnityTextureFormat.TryGetValue(textureData.Format, out var format))
		{
			format = textureData.Type == TextureDataType.Tile && format == TextureFormat.RGBA32 ? TextureFormat.BGRA32 : format;
			Texture2D texture = new Texture2D((int)textureData.Width, (int)textureData.Height, format, false);

			texture.LoadRawTextureData(textureData.Data);
			texture.Apply(true);

			texture.wrapMode = TextureWrapMode.Clamp;
			texture.filterMode = FilterMode.Bilinear;

			return new Texture2DGPUData(texture);
		}

		Debug.LogWarning("Texture format is not supported!");

		return null;
	}

	public ITexture2DGPUData CreateTextureFromUVRegionLUT(TextureCPUData textureData)
	{
		Texture2D texture = new Texture2D((int)textureData.Width, (int)textureData.Height, TextureFormat.RGBAFloat, false);
		var size = textureData.SizeInBytes / 2;
		var floatData = new NativeArray<float>((int)size, Allocator.Temp);
		var ushortData = new ushort[size];

		Buffer.BlockCopy(textureData.Data, 0, ushortData, 0, textureData.Data.Length);

		for (int i = 0; i < size; i++)
		{
			floatData[i] = ushortData[i] / 65535.0f;
		}

		texture.LoadRawTextureData(floatData);
		texture.Apply(true);

		return new Texture2DGPUData(texture);
	}

	public ITexture2DGPUData CreateHeightmapFrom(TextureCPUData textureData)
	{
		int size = (int)textureData.Height;

		Texture2D texture = new Texture2D(size, size, TextureFormat.RFloat, false);

		texture.LoadRawTextureData(textureData.Data);
		texture.Apply(true);

		texture.wrapMode = TextureWrapMode.Clamp;
		texture.filterMode = FilterMode.Point;

		return new Texture2DGPUData(texture);
	}

	public IRenderTextureGPUData CreateImageryRenderTarget(int width, int height)
	{
		var renderTarget = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		renderTarget.enableRandomWrite = true;
		renderTarget.autoGenerateMips = false;
		renderTarget.useMipMap = true;
		renderTarget.anisoLevel = 9;
		renderTarget.Create();

		return new RenderTextureGPUData(renderTarget);
	}

	public IRenderTextureGPUData CreateNormalMapRenderTarget(int width, int height)
	{
		var normalMap = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		normalMap.enableRandomWrite = true;
		normalMap.autoGenerateMips = false;
		normalMap.useMipMap = false;
		normalMap.wrapMode = TextureWrapMode.Clamp;
		normalMap.filterMode = FilterMode.Bilinear;
		normalMap.Create();

		return new RenderTextureGPUData(normalMap);
	}
}

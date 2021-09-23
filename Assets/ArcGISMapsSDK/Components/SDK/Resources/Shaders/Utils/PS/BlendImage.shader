Shader "Unlit/BlendImage"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			int NumTextures;

			Texture2D<float4> Input0;
			SamplerState samplerInput0;
			Texture2D<float4> Input1;
			SamplerState samplerInput1;
			Texture2D<float4> Input2;
			SamplerState samplerInput2;
			Texture2D<float4> Input3;
			SamplerState samplerInput3;
			Texture2D<float4> Input4;
			SamplerState samplerInput4;
			Texture2D<float4> Input5;
			SamplerState samplerInput5;
			Texture2D<float4> Input6;
			SamplerState samplerInput6;
			Texture2D<float4> Input7;
			SamplerState samplerInput7;

			float Opacities[8];
			float4 OffsetsAndScales[8];

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			#define SAMPLE_AND_BLEND(num) \
			if (NumTextures <= ##num)	return float4(output, 1.0f); \
			texSample = Input##num.SampleLevel(samplerInput##num, input.uv * OffsetsAndScales[##num].z + OffsetsAndScales[##num].xy, 0); \
			output = lerp(output, texSample.rgb, Opacities[##num] * texSample.a);

			fixed4 frag (v2f input) : SV_Target
			{
				float3 output = 1.0f;
				float4 texSample = 0.0f;

				SAMPLE_AND_BLEND(0);
				SAMPLE_AND_BLEND(1);
				SAMPLE_AND_BLEND(2);
				SAMPLE_AND_BLEND(3);
				SAMPLE_AND_BLEND(4);
				SAMPLE_AND_BLEND(5);
				SAMPLE_AND_BLEND(6);
				SAMPLE_AND_BLEND(7);

				return float4(output, 1.0f);
			}
			ENDCG
		}
	}
}

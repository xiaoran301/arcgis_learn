Shader "Unlit/ComputeNormals"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
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

			Texture2D<float> Input;

			float MinLatitude;
			float LatitudeAngleDelta;
			float LongitudeArc;
			float LatitudeLength;
			float CircleLongitude;
			float EarthRadius;

			float GetLongitudeLength(float step)
			{
				return CircleLongitude * sin(90.0 - abs(MinLatitude + step * LatitudeAngleDelta)) * LongitudeArc;
			}

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f input) : SV_Target
			{
				uint inputWidth, inputHeight;
				Input.GetDimensions(inputWidth, inputHeight);
				int2 sizeMinusOne = uint2(inputWidth - 1, inputHeight - 1);

				int2 sampleCoord0 = int2(floor(input.uv * float2(inputWidth, inputHeight)));
				int2 sampleCoord1 = max(sampleCoord0 + int2(0, -1), 0);
				int2 sampleCoord2 = min(sampleCoord0 + int2(0, 1), sizeMinusOne);
				int2 sampleCoord3 = max(sampleCoord0 + int2(-1, 0), 0);
				int2 sampleCoord4 = min(sampleCoord0 + int2(1, 0), sizeMinusOne);

				float sample0 = Input.Load(int3(sampleCoord0, 0));
				float sample1 = Input.Load(int3(sampleCoord1, 0));
				float sample2 = Input.Load(int3(sampleCoord2, 0));
				float sample3 = Input.Load(int3(sampleCoord3, 0));
				float sample4 = Input.Load(int3(sampleCoord4, 0));

				float latitudePixelSize = LatitudeLength / inputHeight;
				float longitudePixelSize = GetLongitudeLength((float)sampleCoord0.x) / inputWidth;

				float3 v0 = float3(0.0f, latitudePixelSize * (abs(sampleCoord2.y - sampleCoord0.y)), (sample2 - sample0));
				float3 v1 = float3(0.0f, -latitudePixelSize * (abs(sampleCoord1.y - sampleCoord0.y)), (sample1 - sample0));
				float3 v2 = float3(longitudePixelSize * (abs(sampleCoord4.x - sampleCoord0.x)), 0.0f, (sample4 - sample0));
				float3 v3 = float3(-longitudePixelSize * (abs(sampleCoord3.x - sampleCoord0.x)), 0.0f, (sample3 - sample0));

				float3 normal = 0;

				if (sampleCoord0.x != 0 && sampleCoord0.y != 0 && sampleCoord0.x != sizeMinusOne.x && sampleCoord0.y != sizeMinusOne.y)
				{
					normal = normalize(cross(v0, v3) + cross(v1, v2));
				}
				else
				{
					if (sampleCoord0.x == 0 && sampleCoord0.y == 0)
					{
						normal = normalize(cross(v2, v0));
					}
					else if (sampleCoord0.x == 0 && sampleCoord0.y == sizeMinusOne.y)
					{
						normal = normalize(cross(v1, v2));
					}
					else if (sampleCoord0.x == sizeMinusOne.x && sampleCoord0.y == 0)
					{
						normal = normalize(cross(v0, v3));
					}
					else if (sampleCoord0.x == sizeMinusOne.x && sampleCoord0.y == sizeMinusOne.y)
					{
						normal = normalize(cross(v3, v1));
					}
					else if (sampleCoord0.x == 0)
					{
						normal = normalize(cross(v2, v0));
					}
					else if (sampleCoord0.y == 0)
					{
						normal = normalize(cross(v0, v3));
					}
					else if (sampleCoord0.x == sizeMinusOne.x)
					{
						normal = normalize(cross(v3, v1));
					}
					else if (sampleCoord0.y == sizeMinusOne.y)
					{
						normal = normalize(cross(v1, v2));
					}
				}

				return float4(normal, 1);
			}
			ENDCG
		}
	}
}

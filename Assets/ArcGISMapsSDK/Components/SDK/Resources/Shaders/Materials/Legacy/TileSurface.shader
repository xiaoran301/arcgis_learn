Shader "Custom/MapSurfaceShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.0
		_Metallic("Metallic", Range(0,1)) = 0.0
		_ImageryMapOffsetAndScale("ImageryMapOffsetAndScale", Vector) = (0, 0, 1, 1)
		_NormalsMapOffsetAndScale("NormalsMapOffsetAndScale", Vector) = (0, 0, 1, 1)
		_FishTankMode("FishTankMode", Int) = 0
		_MapAreaMin("MapAreaMin", Vector) = (0, 0, 0, 0)
		_MapAreaMax("MapAreaMax", Vector) = (0, 0, 0, 0)
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		#define PI 3.1415926535f
		#define E 2.7182818284

		#define EPSILON 1e-3;
		#define  EPSILON_NRM	(1.0 / _ScreenParams.x)

		#define NUM_STEPS 20

		#define ITER_GEOMETRY 3
		#define ITER_FRAGMENT 5

		#define SEA_HEIGHT 0.5
		#define SEA_CHOPPY 1.5
		#define SEA_SPEED 2.9
		#define MAX_SEA_FREQ 0.24
		#define MIN_SEA_FREQ 0.054
		#define SEA_BASE float3(0.1,0.19,0.22)
		#define SEA_WATER_COLOR float3(0.8,0.9,0.6)
		#define SEA_TIME (_Time.y * SEA_SPEED)

		#define octave_m float2x2(1.7, 1.2, -1.2, 1.4)

		float4	_ImageryMapOffsetAndScale;
		float4	_NormalsMapOffsetAndScale;
        sampler2D _MainTex;
		sampler2D _BumpMap;
		
		int _FishTankMode;
		float3 _MapAreaMin;
		float3 _MapAreaMax;

        struct Input
        {
            float2 uv_MainTex;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		float hash(float2 p)
		{
			float h = dot(p, float2(127.1, 311.7));
			return frac(sin(h) * 83758.5453123);
		}

		float noise(in float2 p)
		{
			float2 i = floor(p);
			float2 f = frac(p);

			float2 u = f * f * (3.0 - 2.0 * f);

			return -1.0 + 2.0 * lerp(
				lerp(hash(i + float2(0.0, 0.0)),
					hash(i + float2(1.0, 0.0)),
					u.x),
				lerp(hash(i + float2(0.0, 1.0)),
					hash(i + float2(1.0, 1.0)),
					u.x),
				u.y);
		}

		float sea_octave(float2 uv, float choppy)
		{
			uv += noise(uv);
			float2 wv = 1.0 - abs(sin(uv));
			float2 swv = abs(cos(uv));
			wv = lerp(wv, swv, wv);

			return pow(1.0 - pow(wv.x * wv.y, 0.65), choppy);
		}

		#define LARGE_DISTANCE 1.0f

		float map(float3 p, float distance)
		{
			float freq = MIN_SEA_FREQ;
			float amp = SEA_HEIGHT;
			float choppy = SEA_CHOPPY;
			float2 uv = p.xy; uv.x *= 0.85;

			float d, h = 0.0;
			for (int i = 0; i < ITER_GEOMETRY; i++)
			{
				d = sea_octave((uv + SEA_TIME) * freq, choppy);

				h += d * amp;

				uv = mul(octave_m, uv);

				freq *= 1.9;
				amp *= 0.22;

				choppy = lerp(choppy, 1.0, 0.2);
			}

			return p.z - h;
		}

		float mapDetailed(float3 p, float distance)
		{
			float freq = MIN_SEA_FREQ;
			float amp = SEA_HEIGHT;
			float choppy = SEA_CHOPPY;
			float2 uv = p.xy; uv.x *= 0.85;

			float d, h = 0.0;
			for (int i = 0; i < ITER_FRAGMENT; i++)
			{
				d = sea_octave((uv + SEA_TIME) * freq, choppy);
				d += sea_octave((uv - SEA_TIME) * freq, choppy);

				h += d * amp;

				uv = mul(octave_m / 1.2, uv);

				freq *= 1.9;
				amp *= 0.22;

				choppy = lerp(choppy, 1.0, 0.2);
			}

			return p.z - h;
		}

		float heightMapTracing(float3 ori, float3 dir, float distance, out float3 p)
		{
			p = float3(0, 0, 0);
			float tm = 0.0;
			float tx = 50000.0;

			float hx = map(ori + dir * tx, distance);

			if (hx > 0.0) return tx;

			float hm = map(ori + dir * tm, distance);

			float tmid = 0.0;
			for (int i = 0; i < NUM_STEPS; i++)
			{
				tmid = lerp(tm, tx, hm / (hm - hx));
				p = ori + dir * tmid;

				float hmid = map(p, distance);

				if (hmid < 0.0)
				{
					tx = tmid;
					hx = hmid;
				}
				else
				{
					tm = tmid;
					hm = hmid;
				}
			}

			return tmid;
		}

		float4x4 getTangentSpaceMatrix(float3 normal)
		{
			normal = normalize(normal);
			float3 tangent = cross(normal, float3(0, 1, 0));
			tangent = length(tangent) < 0.0001f ? (normal.y < 0 ? float3(0, 0, -1) : float3(0, 0, 1)) : normalize(tangent);
			float3 binormal = normalize(cross(tangent, normal));
			//tangent = normalize(cross(normal, binormal));

			return float4x4(float4(tangent, 0), float4(binormal, 0), float4(normal, 0), float4(0, 0, 0, 1));
		}

		float3 getNormal(float3 p, float eps, float distance)
		{
			float3 n;
			n.z = mapDetailed(p, distance);
			n.x = mapDetailed(float3(p.x + eps, p.y, p.z), distance) - n.z;
			n.y = mapDetailed(float3(p.x, p.y + eps, p.z), distance) - n.z;

			n.z = eps;
			return normalize(n);
		}

		float diffuse(float3 n, float3 l, float p)
		{
			return pow(dot(n, l) * 0.4 + 0.6, p);
		}

		float3 getSeaColor(float3 baseColor, float3 p, float3 n, float3 l, float3 eye, float3 dist)
		{
			float3 color = baseColor + diffuse(n, l, 80.0) * SEA_WATER_COLOR * 0.12;

			float atten = max(1.0 - dot(dist, dist) * 0.001, 0.0);
			color += SEA_WATER_COLOR * (p.z - SEA_HEIGHT) * 0.18 * atten;

			return color;
		}

		float2 mapTo2D(float3 coord)
		{
			return float2(atan2(coord.z, coord.x) / (2.0f*PI), acos(coord.y) / PI);
		}

		void waterEffect(float3 image, float3 worldNormal, float3 worldPosition, inout float waterAlpha, inout float3 waterColor, inout float3 waterNormal)
		{
			float a = pow(1.0f - normalize(image.rgb).r, 8.0f) * 64.0f;
			float d = pow(1.0f - image.r, 32.0f) * 8.0f;
			float e = pow(1.0f - image.g, 16.0f) * 4.0f;
			float b = a * d * e;
			waterAlpha = pow(clamp(clamp(b, 0.0f, 1.0f) - image.g, 0.0f, 1.0f), 0.25f);

			float3x3 TBN = getTangentSpaceMatrix(worldNormal);
			float3 dir = mul(TBN, mul(unity_WorldToObject, float4(normalize(worldPosition - _WorldSpaceCameraPos), 0.0f)));
			float3 tangentPos = float3(100000.0f * mapTo2D(worldNormal), 2.0f);

			float distanceToSea = length(mul(unity_WorldToObject, float4(worldPosition - _WorldSpaceCameraPos, 0.0f)));

			float3 p;
			float distance = heightMapTracing(tangentPos, dir, distanceToSea, p);
			float3 dist = p - tangentPos;

			waterNormal = getNormal(p, dot(dist, dist) * EPSILON_NRM, distanceToSea);
			waterColor = getSeaColor(SEA_BASE, p, waterNormal, mul(TBN, mul(unity_WorldToObject, _WorldSpaceLightPos0.xyz)), dir, dist);
		}

		float fishTankAlphaEffect(float3 worldPosition)
		{
			if (_FishTankMode == 1)
			{
				float3 minE = mul(unity_ObjectToWorld, float4(_MapAreaMin, 1.0f));
				float3 maxE = mul(unity_ObjectToWorld, float4(_MapAreaMax, 1.0f));

				return minE.x < worldPosition.x && minE.z < worldPosition.z && maxE.x > worldPosition.x && maxE.z > worldPosition.z ? 1.0f : 0.0f;
			}
			
			return 1.0f;
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			if (fishTankAlphaEffect(IN.worldPos) < 1.0f)
				discard;

			float4 tileColor = tex2D(_MainTex, _ImageryMapOffsetAndScale.z*IN.uv_MainTex + _ImageryMapOffsetAndScale.xy);
			float4 normalMap = tex2D(_BumpMap, _NormalsMapOffsetAndScale.z * IN.uv_MainTex + _NormalsMapOffsetAndScale.xy);
			tileColor = tileColor.a == 0.0f ? float4(0.5, 0.5, 0.5, 1.0) : tileColor;
			normalMap = normalMap.a == 0.0f ? float4(0, 0, 1, 1) : normalMap;

			float waterAlpha = 0.0f;
			float3 waterColor = 0.0f;
			float3 waterNormal = float3(0, 0, 1);

			/*float3 worldNormal = normalize(mul(unity_WorldToObject, WorldNormalVector(IN, float3(0, 0, 1))));
			waterEffect(tileColor, worldNormal, IN.worldPos, waterAlpha, waterColor, waterNormal);*/
			
			o.Albedo = lerp(tileColor.rgb, waterColor, waterAlpha);
			o.Normal = lerp(normalMap, waterNormal, waterAlpha);
            o.Metallic = 0.0f;
            o.Smoothness = lerp(0.0, 0.95, waterAlpha);
            o.Alpha = 1.0f;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

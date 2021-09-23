Shader "Lit/SceneNodeSurface"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "LightMode" = "ForwardBase" }
        Cull Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float uv1 : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            Texture2D _UVRegionLUT;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv1 = v.uv1;
                return o;
            }

            float2 UVRegionTransform(float2 uv, Texture2D lut, uint lutIndex)
            {
                float4 value = lut.Load(int3(lutIndex, 0, 0));

                if (value.x == 0 && value.y == 0 && value.z == 0 && value.w == 0)
                {
                    return uv;
                }
                else
                {
                    return frac(uv) * (value.zw - value.xy) + value.xy;
                }
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, UVRegionTransform(i.uv, _UVRegionLUT, uint(i.uv1 + 0.5f)));
                float nl = max(0, dot(i.worldNormal, _WorldSpaceLightPos0.xyz));
                float3 diffuse = _LightColor0.rgb * nl;
                float3 ambient = (float3)ShadeSH9(half4(i.worldNormal, 1));
                return float4(col.rgb * (diffuse + ambient), 1.0f);
            }
            ENDCG
        }
    }
}

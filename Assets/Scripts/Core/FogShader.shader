Shader "Custom/FogShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FogColor ("Fog Color", Color) = (0.1, 0.1, 0.2, 0.8)
        _FogIntensity ("Fog Intensity", Range(0, 1)) = 0
        _FogDistance ("Fog Distance", Range(1, 50)) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _FogColor;
            float _FogIntensity;
            float _FogDistance;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the main texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Calculate fog based on screen position
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float distanceFromCenter = length(screenUV - 0.5);
                
                // Create fog effect
                float fogFactor = saturate(distanceFromCenter * _FogDistance);
                fogFactor = pow(fogFactor, 2); // Make fog more intense at edges
                
                // Blend with fog color
                fixed4 fogCol = _FogColor;
                fogCol.a *= _FogIntensity * fogFactor;
                
                // Blend fog with original color
                return lerp(col, fogCol, fogCol.a);
            }
            ENDCG
        }
    }
} 
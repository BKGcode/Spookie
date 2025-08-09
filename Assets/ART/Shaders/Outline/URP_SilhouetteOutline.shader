Shader "Spookie/URP/SilhouetteOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1, 0.84, 0, 1)
        _Thickness ("Thickness", Float) = 0.02
        _ZOffset ("Depth Offset", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" "RenderType" = "Opaque" "Queue" = "Geometry+10" }
        LOD 100

        Cull Front      // cull front faces so backfaces outline the silhouette
        ZWrite On
        ZTest LEqual
        Blend One Zero  // opaque

        Pass
        {
            Name "SILHOUETTE_OUTLINE"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            float4 _OutlineColor;
            float  _Thickness;
            float  _ZOffset;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Expand along object-space normals
                float3 posOS = IN.positionOS.xyz + (normalize(IN.normalOS) * _Thickness);
                float4 posHCS = TransformObjectToHClip(float4(posOS, 1.0));

                // Optional depth bias to reduce z-fighting with base mesh
                posHCS.z += _ZOffset * posHCS.w;

                OUT.positionHCS = posHCS;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
    Fallback Off
}

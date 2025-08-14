Shader "Custom/EmissionControlAndOutline"
{
    Properties
    {
        _Color ("Base Color", Color) = (.5,.5,.5,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _EmissionColor("Emission Color", Color) = (0, 0, 0, 0)
        _OutlineWidth ("Outline Width", Range (0, 1)) = .1
        _EmissionIntensity("Emission Intensity", Range(0, 1)) = 1
        _BaseTexture ("Base Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _MetallicMap("Metallic Map", 2D) = "white" {}
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0
        _Glossiness ("Smoothness", Range(0.0, 1.0)) = 0.5
        _Brightness("Brightness", Range(0.0, 2.0)) = 1.0
        [Toggle] _EnableEmission("Enable Emission", Int) = 0
        [Toggle] _ShowOutline ("Show Outline", Float) = 1
    }
    
    CGINCLUDE
    #include "UnityCG.cginc"
    #include "UnityStandardUtils.cginc"
    #include "AutoLight.cginc"
    
    struct appdata
    {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float4 tangent : TANGENT;
        float2 uv : TEXCOORD0;
    };
    
    struct v2f
    {
        float4 pos : POSITION;
        float4 color : COLOR;
        float2 uv : TEXCOORD0;
    };
    
    uniform float _OutlineWidth;
    uniform float4 _OutlineColor;
    uniform bool _ShowOutline;

    v2f ModifyVert(appdata v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex * (1 + _OutlineWidth));
        o.color = _OutlineColor * (_ShowOutline ? 1.0 : 0.0);
        o.uv = v.uv;
        return o;
    }
    ENDCG
    
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _BaseTexture;
        sampler2D _NormalMap;
        sampler2D _MetallicMap;
        fixed4 _Color;
        fixed4 _EmissionColor;
        half _EmissionIntensity;
        int _EnableEmission;
        half _Metallic;
        half _Glossiness;
        half _Brightness;

        struct Input
        {
            float2 uv_BaseTexture;
            float2 uv_NormalMap;
            float2 uv_MetallicMap;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_BaseTexture, IN.uv_BaseTexture) * _Color;

            // Apply brightness adjustment
            c.rgb *= _Brightness;

            // Ensure the normal map is applied correctly
            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
            
            // Set the Albedo and Smoothness values
            o.Albedo = c.rgb;
            o.Smoothness = _Glossiness;

            // Use the metallic map if provided, otherwise use the default metallic value
            fixed metallicValue = tex2D(_MetallicMap, IN.uv_MetallicMap).r * _Metallic;
            o.Metallic = metallicValue;

            // Apply emission if enabled
            if (_EnableEmission == 1) 
            {
                o.Emission = _EmissionColor.rgb * _EmissionIntensity;
            }
        }
        ENDCG

        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            Cull Front
            ZWrite On
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha
            AlphaTest Greater 0
            CGPROGRAM
            #pragma vertex ModifyVert
            #pragma fragment frag
            
            half4 frag(v2f i) : COLOR
            {
                return i.color;
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}

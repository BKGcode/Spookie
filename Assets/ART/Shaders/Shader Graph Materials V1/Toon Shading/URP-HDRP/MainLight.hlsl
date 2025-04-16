void LWRPLightingFunction_float(float3 ObjPos, out float3 Direction, out float3 Color, out float ShadowAtten)
{

#ifdef SHADERGRAPH_PREVIEW
   Direction = float3(0.5, 0.5, 0);
   Color = 1;
   ShadowAtten = 1;

#else
   Direction = float3(0.5, 0.5, 0);
   Color = 1;
   ShadowAtten = 1;
   #if defined(UNIVERSAL_LIGHTING_INCLUDED)
    
      #if SHADOWS_SCREEN
         float4 clipPos = TransformWorldToHClip(ObjPos);
         float4 shadowCoord = ComputeScreenPos(clipPos);
      #else
         float4 shadowCoord = TransformWorldToShadowCoord(ObjPos);
      #endif

      Light mainLight = GetMainLight(shadowCoord);
      Direction = mainLight.direction;
      Color = mainLight.color;
      ShadowAtten = mainLight.shadowAttenuation;
    
    #else
         DirectionalLightData light = _DirectionalLightDatas[0];
         Direction = -light.forward.xyz;
         Color = normalize(light.color);
         ShadowAtten = 1;
    #endif
#endif
}
﻿#ifndef POTA_TOON_EYE_PASS_INCLUDED
#define POTA_TOON_EYE_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../../Common/PotaToonCommon.hlsl"

struct Attributes
{
    float4 positionOS           : POSITION;
    float3 normalOS             : NORMAL;       // Only used for bakedGI
    float2 uv                   : TEXCOORD0;
    float2 staticLightmapUV     : TEXCOORD1;
    float2 dynamicLightmapUV    : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS           : SV_POSITION;
    float2 uv                   : TEXCOORD0;
    float3 positionWS           : TEXCOORD1;
    float3 positionOS           : TEXCOORD2;
    float3 normalWS             : TEXCOORD3;    // Only used for bakedGI
    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 4);
#ifdef DYNAMICLIGHTMAP_ON
    float2  dynamicLightmapUV   : TEXCOORD5; // Dynamic lightmap UVs
#endif
#ifdef USE_APV_PROBE_OCCLUSION
    float4 probeOcclusion : TEXCOORD6;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings vert(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.positionOS = input.positionOS.xyz;
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.uv = input.uv;
    output.normalWS = TransformObjectToWorldDir(input.normalOS);

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    return output;
}

half4 frag(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    input.normalWS = normalize(input.normalWS);
    float2 uv = input.uv;
    float2 normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    InputData inputData = (InputData)0;
    inputData.positionWS = input.positionWS;
    inputData.normalizedScreenSpaceUV = normalizedScreenSpaceUV;
    
    // Apply Refraction
    const float3 F = _FaceForward.xyz;
    const float3 V = normalize(_WorldSpaceCameraPos.xyz - input.positionWS.xyz);
    if (_UseRefraction > 0)
    {
        const float3 vWorld = _FaceUp.xyz;
        const float3 uWorld = cross(vWorld, F);
        float3 offset = float3(dot(uWorld, V), dot(vWorld, V), dot(F, V));
    #if UNITY_UV_STARTS_AT_TOP
        offset.y = -offset.y;
    #endif
        uv += lerp(0, offset.xy * _RefractionWeight, _UseRefraction);
    }
    float3 normalWS = F;

    const half clippingMask = SelectMask(SAMPLE_TEXTURE2D(_ClippingMask, sampler_linear_mirror, TRANSFORM_TEX(uv, _ClippingMask)), _ClippingMaskCH);
    clip(clippingMask - _Cutoff);
    
    const half4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_linear_mirror, TRANSFORM_TEX(uv, _MainTex));
    half3 baseColor = _BaseColor.rgb * baseMap.rgb;
    
#ifdef _LIGHT_LAYERS
    uint meshRenderingLayers = GetMeshRenderingLayer();
#endif

#if _USE_EYE_HI_LIGHT
    // High light
    half3 hiColor = 0;
    half4 hiLightTexVar = SAMPLE_TEXTURE2D(_HiLightTex, sampler_linear_mirror, TRANSFORM_TEX(uv, _HiLightTex));
    float3 hiLightPower = float3(_HiLightPowerR, _HiLightPowerG, _HiLightPowerB);
    if (_UseHiLightJitter > 0)
    {
        hiLightPower = lerp(1, hiLightPower, cos(_Time.y * 40) > 0);
    }
    hiColor += (abs(PositivePow(hiLightTexVar.r, hiLightPower.r)) * _HiLightIntensityR).rrr;
    hiColor += (abs(PositivePow(hiLightTexVar.g, hiLightPower.g)) * _HiLightIntensityG).rrr;
    hiColor += (abs(PositivePow(hiLightTexVar.b, hiLightPower.b)) * _HiLightIntensityB).rrr;
    baseColor += hiColor * _HiLightColor.rgb;
#endif
    half3 color = baseColor;

    // Main Light
    Light mainLight = GetMainLight();
    half3 mainLightColor = mainLight.color.rgb * _Exposure;
    const float mainLightIntensity = 0.299 * mainLightColor.r + 0.587 * mainLightColor.g + 0.114 * mainLightColor.b;

    color = lerp(color * mainLightColor, color * _MinIntensity, mainLightIntensity < _MinIntensity);   // Don't apply lambert to guarantee whole eye shape.
    if (mainLightIntensity == 0)
    {
        color = 0;
    }

#if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();
    half3 additionalLightsColor = 0;

    // Directional Lights
#if USE_FORWARD_PLUS
    for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

        Light light = GetAdditionalLight(lightIndex, input.positionWS, 0);

    #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
    #endif
        {
            half3 lightColor = light.color.rgb * _Exposure;
            additionalLightsColor += baseColor * lightColor * light.distanceAttenuation;
        }
    }
#endif

    // Local Lights
    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, 0);

    #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
    #endif
        {
            half3 lightColor = light.color.rgb * _Exposure;
            additionalLightsColor += lightColor * light.distanceAttenuation * light.shadowAttenuation;
        }
    LIGHT_LOOP_END

    color += baseColor * additionalLightsColor;
#endif

    // GI
    half alpha = 1;
    BRDFData brdfData;
    InitializeBRDFData(baseColor, 0, 0, 1, alpha, brdfData);
    #if defined(DYNAMICLIGHTMAP_ON)
    half3 bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, normalWS);
    #elif UNITY_VERSION >= 202230 && !defined(LIGHTMAP_ON) && (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
    half3 bakedGI = SAMPLE_GI(input.vertexSH, GetAbsolutePositionWS(inputData.positionWS), normalWS, GetWorldSpaceViewDir(inputData.positionWS), input.positionCS.xy, input.probeOcclusion, inputData.shadowMask);
    #else
    half3 bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, normalWS);
    #endif
    MixRealtimeAndBakedGI(mainLight, input.normalWS, bakedGI);
#if UNITY_VERSION >= 202230
    half3 indirectLighting = GlobalIllumination(brdfData, brdfData, 0, bakedGI, 1, input.positionWS, normalWS, V, normalizedScreenSpaceUV);
#else
    half3 indirectLighting = GlobalIllumination(brdfData, brdfData, 0, bakedGI, 1, input.positionWS, normalWS, V);
#endif
    indirectLighting = max(0, indirectLighting * _IndirectDimmer); // Prevent NaN
    color += indirectLighting;

    return half4(color, 1);
}


#endif
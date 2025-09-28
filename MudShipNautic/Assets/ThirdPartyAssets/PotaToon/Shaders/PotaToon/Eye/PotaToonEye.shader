Shader "PotaToon/Eye"
{
    Properties
    {
        // Base Settings
        [HideInInspector] _ToonType("Toon Type", Int) = 2
        [Enum(OFF, 0, FRONT, 1, BACK, 2)] _CullMode("Cull Mode", Int) = 2
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        
        // Stencil
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 0
        _StencilRef("Stencil Ref", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPass("Stencil Pass Operation", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFail("Stencil Fail Operation", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail("Stencil ZFail Operation", Float) = 0
        
        // Settings
        [HDR] _BaseColor("Color", Color) = (1,1,1,1)
        _MainTex("Main Tex", 2D) = "white" {}
        _ClippingMask ("Clipping Mask", 2D) = "white" {}
        _Exposure("Exposure", Range(1, 10)) = 1
        _MinIntensity("Minium Intensity", Range(0, 1)) = 0.1
        _IndirectDimmer ("Indirect Dimmer", Range(0, 10)) = 0
        [Toggle] _UseRefraction("Use Refraction", Int) = 1
        _RefractionWeight("Refraction Weight", Range(-0.1, 0.1)) = 0
        [HideInInspector] [Toggle] _UseHiLight("_UseHiLight", Int) = 0
        _HiLightTex("HighLight Tex", 2D) = "black" {}
        [Toggle] _UseHiLightJitter("Use Jittering", Int) = 0
        [HDR] _HiLightColor("HighLight Color", Color) = (1,1,1,1)
        _HiLightPowerR("HighLight Power for R Channel", Range(1, 64)) = 1
        _HiLightPowerG("HighLight Power for G Channel", Range(1, 64)) = 1
        _HiLightPowerB("HighLight Power for B Channel", Range(1, 64)) = 1
        _HiLightIntensityR("HighLight Intensity for R Channel", Range(0, 1)) = 1
        _HiLightIntensityG("HighLight Intensity for G Channel", Range(0, 1)) = 1
        _HiLightIntensityB("HighLight Intensity for B Channel", Range(0, 1)) = 1
        
        [Enum(R, 0, G, 1, B, 2, A, 3)] _ClippingMaskCH ("ClippingMask Channel", Int) = 1
        
        [HideInInspector] _FaceForward ("_FaceForward", Vector) = (0,0,1,0)
        [HideInInspector] _FaceUp ("_FaceUp", Vector) = (0,1,0,0)
    }
    SubShader
    {
        PackageRequirements
        {
             "com.unity.render-pipelines.universal": "12.0.0"
        }    
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull[_CullMode]
            Stencil
            {
                Ref[_StencilRef]
                Comp[_StencilComp]
                Pass[_StencilPass]
                Fail[_StencilFail]
                ZFail[_StencilZFail]
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ LIGHTMAP_ON DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ PROBE_VOLUMES_L1 PROBE_VOLUMES_L2
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _USE_EYE_HI_LIGHT

            #include "./PotaToonEyeInput.hlsl"
            #include "./PotaToonEyePass.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "./PotaToonEyeInput.hlsl"
            #define _BaseMap _MainTex
            #define sampler_BaseMap sampler_linear_mirror
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
        // This pass is used when drawing to a _CameraNormalsTexture texture
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode" = "DepthNormals"}

            ZWrite On
            Cull[_Cull]

            HLSLPROGRAM
            #pragma only_renderers gles gles3 glcore d3d11
            #pragma target 2.0

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "./PotaToonEyeInput.hlsl"
            #define _BaseMap _MainTex
            #define sampler_BaseMap sampler_linear_mirror
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"

            ENDHLSL
        }

        Pass
       {
           Name "PotaToonCharacterMask"
           Tags {
               "LightMode" = "PotaToonCharacterMask"
           }
           ZWrite Off ZTest Always Cull Off
           
           HLSLPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           
           #include "./PotaToonEyeInput.hlsl"
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

           struct Attributes
           {
               float4 position     : POSITION;
           };
           struct Varyings
           {
               float4 positionCS   : SV_POSITION;
           };

           Varyings vert(Attributes input)
           {
               Varyings output = (Varyings)0;
               output.positionCS = TransformObjectToHClip(input.position.xyz);
               return output;
           }

           half frag(Varyings input) : SV_TARGET
           {
               const float2 ssUV = GetNormalizedScreenSpaceUV(input.positionCS.xy);
               const float sceneDepth = SampleSceneDepth(ssUV);
               const float linearDepth = LinearEyeDepth(sceneDepth, _ZBufferParams);
               const float inputLinearDepth = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
               if (inputLinearDepth >= linearDepth + 0.01)
                   clip(-1);
               return 1;
           }
           
           ENDHLSL
       }
    }
    CustomEditor "PotaToon.Editor.PotaToonEyeShaderGUI"
}

Shader "PotaToon/Toon"
{
    Properties
    {
        // Base Settings
        [HideInInspector] _ToonType("Toon Type", Int) = 0
        [Enum(Opaque, 0, Cutout, 1, Refraction, 2, Transparent, 3)] _SurfaceType("Surface Type", Int) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Int) = 2
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [HideInInspector] [Enum(OFF, 0, ON, 1)] _ZWriteMode("_ZWriteMode", Int) = 1
        [HideInInspector] _AutoRenderQueue("_AutoRenderQueue", Int) = 1
        
        // Stencil
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 0
        _StencilRef("Stencil Ref", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPass("Stencil Pass Operation", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFail("Stencil Fail Operation", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail("Stencil ZFail Operation", Float) = 0
        
        // Main Settings
        _MainTex ("MainTex", 2D) = "white" {}
        [HDR] _BaseColor ("Base Color", Color) = (1,1,1,1)
        [HideInInspector] _UseShadeMap ("_UseShadeMap", Int) = 0
        _ShadeMap ("ShadeMap", 2D) = "white" {}
        _ShadeColor ("Shade Color", Color) = (0.75,0.75,0.75,1)
        _ShadowBorderMask ("AOMap", 2D) = "white" {}
        _BaseStep ("Base Step", Range(0, 1)) = 0.5
        _StepSmoothness ("Step Smoothness", Range(0, 0.2)) = 0.01
        [Toggle] _ReceiveLightShadow ("Receive Light Shadow", Int) = 1
        [Toggle] _UseMidTone ("Mid Tone", Int) = 1
        [HDR] _MidColor ("Mid Color", Color) = (0.5,0.2,0.2,1)
        _MidWidth ("Mid Thickness", Range(0, 1)) = 1
        _IndirectDimmer ("Indirect Dimmer", Range(0, 10)) = 1
        [Toggle] _UseVertexColor ("Vertex Color", Int) = 0
        [Toggle] _UseDarknessMode ("Use Darkness Mode", Int) = 0
        _NormalMap ("NormalMap", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Range(0, 1)) = 0
        [Toggle] _UseNormalMap ("Use NormalMap", Int) = 0
        _ClippingMask ("Clipping Mask", 2D) = "white" {}
        
        // High Light
        [HDR] _SpecularColor ("Specular Color", Color) = (0,0,0,1)
        _SpecularMap ("SpecularMap", 2D) = "white" {}
        _SpecularMask ("SpecularMask", 2D) = "white" {}
        _SpecularPower ("Specular Power", Range(0, 1)) = 0.5
        _SpecularSmoothness ("Specular Smoothness", Range(0, 0.5)) = 0.25

        // Rim Light
        [HDR] _RimColor ("RimLight Color", Color) = (0,0,0,1)
        _RimMask ("RimLight Mask", 2D) = "white" {}
        _RimPower ("Rim Power", Range(0, 1)) = 0.5
        _RimSmoothness ("Rim Smoothness", Range(0, 0.5)) = 0.25

        // MatCap
        [Enum(None, 0, Add, 1, Multiply, 2)] _MatCapMode ("MatCap Mode", Int) = 0
        [HDR] _MatCapColor ("MatCap Color", Color) = (1,1,1,1)
        _MatCapTex ("MatCap Tex", 2D) = "black" {}
        _MatCapMask ("MatcapMask", 2D) = "white" {}
        _MatCapWeight ("Matcap Weight", Range(0, 1)) = 1
        _MatCapLightingDimmer ("Matcap Lighting Dimmer", Range(0, 1)) = 1
        
        [Enum(None, 0, Add, 1, Multiply, 2)] _MatCapMode2 ("MatCap2 Mode", Int) = 0
        [HDR] _MatCapColor2 ("MatCap2 Color", Color) = (1,1,1,1)
        _MatCapTex2 ("MatCap Tex2", 2D) = "black" {}
        _MatCapMask2 ("MatcapMask2", 2D) = "white" {}
        _MatCapWeight2 ("Matcap Weight2", Range(0, 1)) = 1
        _MatCapLightingDimmer2 ("Matcap Lighting Dimmer2", Range(0, 1)) = 1
        
        [Enum(None, 0, Add, 1, Multiply, 2)] _MatCapMode3 ("MatCap3 Mode", Int) = 0
        [HDR] _MatCapColor3 ("MatCap2 Color", Color) = (1,1,1,1)
        _MatCapTex3 ("MatCap Tex3", 2D) = "black" {}
        _MatCapMask3 ("MatcapMask3", 2D) = "white" {}
        _MatCapWeight3 ("Matcap Weight3", Range(0, 1)) = 1
        _MatCapLightingDimmer3 ("Matcap Lighting Dimmer3", Range(0, 1)) = 1
        
        [Enum(None, 0, Add, 1, Multiply, 2)] _MatCapMode4 ("MatCap4 Mode", Int) = 0
        [HDR] _MatCapColor4 ("MatCap4 Color", Color) = (1,1,1,1)
        _MatCapTex4 ("MatCap Tex4", 2D) = "black" {}
        _MatCapMask4 ("MatcapMask4", 2D) = "white" {}
        _MatCapWeight4 ("Matcap Weight4", Range(0, 1)) = 1
        _MatCapLightingDimmer4 ("Matcap Lighting Dimmer4", Range(0, 1)) = 1
        
        [Enum(None, 0, Add, 1, Multiply, 2)] _MatCapMode5 ("MatCap5 Mode", Int) = 0
        [HDR] _MatCapColor5 ("MatCap5 Color", Color) = (1,1,1,1)
        _MatCapTex5 ("MatCap Tex5", 2D) = "black" {}
        _MatCapMask5 ("MatcapMask5", 2D) = "white" {}
        _MatCapWeight5 ("Matcap Weight5", Range(0, 1)) = 1
        _MatCapLightingDimmer5 ("Matcap Lighting Dimmer5", Range(0, 1)) = 1
        
        [Enum(None, 0, Add, 1, Multiply, 2)] _MatCapMode6 ("MatCap6 Mode", Int) = 0
        [HDR] _MatCapColor6 ("MatCap6 Color", Color) = (1,1,1,1)
        _MatCapTex6 ("MatCap Tex6", 2D) = "black" {}
        _MatCapMask6 ("MatcapMask6", 2D) = "white" {}
        _MatCapWeight6 ("Matcap Weight6", Range(0, 1)) = 1
        _MatCapLightingDimmer6 ("Matcap Lighting Dimmer6", Range(0, 1)) = 1
        
        [Enum(None, 0, Add, 1, Multiply, 2)] _MatCapMode7 ("MatCap7 Mode", Int) = 0
        [HDR] _MatCapColor7 ("MatCap7 Color", Color) = (1,1,1,1)
        _MatCapTex7 ("MatCap Tex7", 2D) = "black" {}
        _MatCapMask7 ("MatcapMask7", 2D) = "white" {}
        _MatCapWeight7 ("Matcap Weight7", Range(0, 1)) = 1
        _MatCapLightingDimmer7 ("Matcap Lighting Dimmer7", Range(0, 1)) = 1
        
        [Enum(None, 0, Add, 1, Multiply, 2)] _MatCapMode8 ("MatCap8 Mode", Int) = 0
        [HDR] _MatCapColor8 ("MatCap8 Color", Color) = (1,1,1,1)
        _MatCapTex8 ("MatCap Tex8", 2D) = "black" {}
        _MatCapMask8 ("MatcapMask8", 2D) = "white" {}
        _MatCapWeight8 ("Matcap Weight8", Range(0, 1)) = 1
        _MatCapLightingDimmer8 ("Matcap Lighting Dimmer8", Range(0, 1)) = 1
        
        // Emission
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionMap ("EmissionMap", 2D) = "white" {}
        _EmissionMask ("EmissionMask", 2D) = "white" {}
        
        // Glitter
        [HideInInspector] [Toggle] _UseGlitter ("_UseGlitter", Int) = 0
        [HDR] _GlitterColor("Color", Color) = (1,1,1,1)
        _GlitterColorTex("Texture", 2D) = "white" {}
        _GlitterMainStrength("Main Color Strength", Range(0, 1)) = 0
        _GlitterEnableLighting("Enable Lighting", Range(0, 1)) = 1
        [Toggle] _GlitterBackfaceMask("Backface Mask", Int) = 0
        [Toggle] _GlitterApplyTransparency("Apply Transparency", Int) = 1
        _GlitterShadowMask("Shadow Mask", Range(0, 1)) = 0
        _GlitterParticleSize("Particle Size", Float) = 0.16
        _GlitterScaleRandomize("Scale Randomize", Range(0, 1)) = 0
        _GlitterContrast("Contrast", Float) = 50
        _GlitterSensitivity("Sensitivity", Float) = 100
        _GlitterBlinkSpeed("Blink Speed", Float) = 0.1
        _GlitterAngleLimit("Angle Limit", Float) = 0
        _GlitterLightDirection("Light Direction Strength", Float) = 0
        _GlitterColorRandomness("Color Randomness", Range(0, 1)) = 0
        _GlitterNormalStrength("NormalMap Strength", Range(0, 1)) = 1.0
        _GlitterPostContrast("Post Contrast", Float) = 1

        // Outline
        [Enum(Normal, 0, Position, 1)] _OutlineMode("Outline Mode", Int) = 0
        [HideInInspector] [Toggle] _UseOutlineNormalMap("_UseOutlineNormalMap", Int) = 0
        _OutlineNormalMap ("NormalMap", 2D) = "bump" {}
        [Toggle] _BlendOutlineMainTex("Blend MainTex", Int) = 1
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidthMask ("Outline Width Mask", 2D) = "white" {}
        _OutlineWidth ("Outline Width", Range(0, 10)) = 0.1
        _OutlineOffsetZ ("Depth Offset", Float) = 0
        
        // Refraction
        _RefractionWeight ("Refraction Weight", Range(-1, 1)) = 0
        _RefractionBlurStep ("Refraction Blur Step", Range(0, 10)) = 0
        
        // Character Shadow
        [Toggle] _DisableCharShadow ("Disable Cast Shadow", Int) = 0 
        _DepthBias ("Cast Shadow Bias", Range(-1, 1)) = 0
        _NormalBias ("Cast Shadow Normal Bias", Range(-1, 1)) = 0
        _CharShadowSmoothnessOffset ("Smoothness", Range(0, 1)) = 0
        [HideInInspector] [Enum(3D, 0, 2D Face, 1)] _CharShadowType ("_CharShadowType", Int) = 0
        _2DFaceShadowWidth ("2D Shadow Width", Range(0, 1)) = 0.1

        // Face SDF
        [HideInInspector] [Toggle] _UseFaceSDFShadow ("_UseFaceSDFShadow", Int) = 0
        _FaceSDFTex ("Face SDF", 2D) = "white" {}
        [Toggle] _SDFReverse("Reverse Face SDF", Int) = 0
        _SDFOffset("SDF_Offset", Range(-0.5, 0.5)) = 0
        _SDFBlur("SDF_Blur", Range(0, 1)) = 0
        [HideInInspector] _FaceForward ("_FaceForward", Vector) = (0,0,1,0)
        [HideInInspector] _FaceUp ("_FaceUp", Vector) = (0,1,0,0)
        
        // Hair High Light
        [Toggle] _UseHairHighLight ("Use Hair High Light", Int) = 0
        _HairHighLightTex ("Hair Highlight Tex", 2D) = "black" {}
        [Toggle] _ReverseHairHighLightTex ("Reverse Hair Highlight Tex", Int) = 0
        _HairHiStrength("Hair Hi Strength", Range(0, 2)) = 1
        _HairHiUVOffset("Hair Hi UV Offset", Range(-1, 1)) = 0
        [HideInInspector] _HeadWorldPos ("_HeadWorldPos", Vector) = (0,0,0,0)
        
        // UV Channels
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _BaseMapUV ("Base UV", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _NormalMapUV ("NormalMap UV", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _ClippingMaskUV ("ClippingMask UV", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _FaceSDFUV ("FaceSDF UV", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _SpecularMapUV ("Specular UV", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _RimMaskUV ("RimMask UV", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _HairHiMapUV ("HairHigh UV", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _GlitterMapUV ("Glitter UV", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _EmissionMapUV ("Emission UV", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _OutlineMaskUV ("OutlineMask UV", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _MatCapUV1 ("MatCap UV1", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _MatCapUV2 ("MatCap UV2", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _MatCapUV3 ("MatCap UV3", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _MatCapUV4 ("MatCap UV4", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _MatCapUV5 ("MatCap UV5", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _MatCapUV6 ("MatCap UV6", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _MatCapUV7 ("MatCap UV7", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _MatCapUV8 ("MatCap UV8", Int) = 0
        
        // Mask Channels
        [Enum(R, 0, G, 1, B, 2, A, 3)] _ClippingMaskCH ("ClippingMask Channel", Int) = 1
        [Enum(R, 0, G, 1, B, 2, A, 3)] _SpecularMaskCH ("SpecularMask Channel", Int) = 1
        [Enum(R, 0, G, 1, B, 2, A, 3)] _RimMaskCH ("RimMask Channel", Int) = 1
        [Enum(R, 0, G, 1, B, 2, A, 3)] _EmissionMaskCH ("EmissionMask Channel", Int) = 1
        [Enum(R, 0, G, 1, B, 2, A, 3)] _OutlineMaskCH ("OutlineMask Channel", Int) = 1
        [Enum(R, 0, G, 1, B, 2, A, 3)] _MatCapMaskCH1 ("MatCapMask Channel", Int) = 0
        [Enum(R, 0, G, 1, B, 2, A, 3)] _MatCapMaskCH2 ("MatCapMask Channel", Int) = 0
        [Enum(R, 0, G, 1, B, 2, A, 3)] _MatCapMaskCH3 ("MatCapMask Channel", Int) = 0
        [Enum(R, 0, G, 1, B, 2, A, 3)] _MatCapMaskCH4 ("MatCapMask Channel", Int) = 0
        [Enum(R, 0, G, 1, B, 2, A, 3)] _MatCapMaskCH5 ("MatCapMask Channel", Int) = 0
        [Enum(R, 0, G, 1, B, 2, A, 3)] _MatCapMaskCH6 ("MatCapMask Channel", Int) = 0
        [Enum(R, 0, G, 1, B, 2, A, 3)] _MatCapMaskCH7 ("MatCapMask Channel", Int) = 0
        [Enum(R, 0, G, 1, B, 2, A, 3)] _MatCapMaskCH8 ("MatCapMask Channel", Int) = 0
        [Enum(R, 0, G, 1, B, 2, A, 3)] _AOMapCH ("AOMap Channel", Int) = 1
        [Enum(R, 0, G, 1, B, 2, A, 3)] _FaceSDFTexCH ("FaceSDF Channel", Int) = 0
        
        // Debug
        [Enum(None, 0, Lighting, 1, Texture, 2)] _DebugFaceSDF ("Debug Face SDF", Int) = 0
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
            Name "Outline"
            Tags
            {
                "LightMode"="SRPDefaultUnlit"
            }
            Cull Front
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

            #pragma target 4.5

            #pragma multi_compile_fragment _ _POTA_TOON_OIT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile _ _LIGHT_LAYERS

            #include "./PotaToonInput.hlsl"
            #include "./PotaToonOutlinePass.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}
            ZWrite[_ZWriteMode]
            Cull[_Cull]
            Blend SrcAlpha OneMinusSrcAlpha
            Stencil
            {
                Ref[_StencilRef]
                Comp[_StencilComp]
                Pass[_StencilPass]
                Fail[_StencilFail]
                ZFail[_StencilZFail]
            }

            HLSLPROGRAM
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag

            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // -------------------------------------
            // Universal Render Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ PROBE_VOLUMES_L1 PROBE_VOLUMES_L2
            #pragma multi_compile_fog

            // -------------------------------------
            // Material keywords
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            
            // -------------------------------------
            // POTA Toon keywords
            #pragma multi_compile_fragment _ _POTA_TOON_OIT
            #pragma multi_compile_fragment _ _USE_FACE_SDF
            #pragma multi_compile_fragment _ _USE_2D_FACE_SHADOW
            #pragma multi_compile_fragment _ _USE_GLITTER
            #pragma shader_feature_fragment _ _DEBUG_POTA_TOON

            // #pragma enable_d3d11_debug_symbols

            #include "./PotaToonInput.hlsl"
            #include "./PotaToonPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0
	    
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles

            #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "./PotaToonInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
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

            #include "./PotaToonInput.hlsl"
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

            #include "./PotaToonInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"

            ENDHLSL
        }

        Pass
       {
           Name "CharacterDepth"
           Tags{"LightMode" = "CharacterDepth"}

           ZWrite On
           ZTest LEqual
           Cull Off
           BlendOp Max

           HLSLPROGRAM
           #pragma target 2.0
    
           // Required to compile gles 2.0 with standard srp library
           #pragma prefer_hlslcc gles
           #pragma exclude_renderers d3d11_9x
           #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
           #pragma shader_feature_local _ALPHATEST_ON

           #pragma vertex CharShadowVertex
           #pragma fragment CharShadowFragment

           #include "./PotaToonInput.hlsl"
           #include "../ChracterShadow/CharacterShadowDepthPass.hlsl"
           ENDHLSL
       }

       Pass
       {
           Name "TransparentShadow"
           Tags {"LightMode" = "TransparentShadow"}

           ZWrite Off
           ZTest Off
           Cull Off
           Blend One One
           BlendOp Max

           HLSLPROGRAM
           #pragma target 2.0
    
           #pragma prefer_hlslcc gles
           #pragma exclude_renderers d3d11_9x
           #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
           #pragma shader_feature_local _ALPHATEST_ON

           #pragma vertex TransparentShadowVert
           #pragma fragment TransparentShadowFragment

           #include "./PotaToonInput.hlsl"
           #include "../ChracterShadow/TransparentShadowPass.hlsl"
           ENDHLSL
       }
       Pass
       {
           Name "TransparentAlphaSum"
           Tags {"LightMode" = "TransparentAlphaSum"}

           ZWrite Off
           ZTest Off
           Cull Off
           Blend One One
           BlendOp Add

           HLSLPROGRAM
           #pragma target 4.5
    
           #pragma prefer_hlslcc gles
           #pragma exclude_renderers d3d11_9x
           #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
           #pragma shader_feature_local _ALPHATEST_ON

           #pragma vertex TransparentAlphaSumVert
           #pragma fragment TransparentAlphaSumFragment

           #include "./PotaToonInput.hlsl"
           #include "../ChracterShadow/TransparentShadowPass.hlsl"
           ENDHLSL
       }
       Pass
       {
           Name "OITDepth"
           Tags {
               "LightMode" = "OITDepth"
           }
           ZWrite Off
           ZTest Always
           Cull OFF
           ColorMask R
           BlendOp Max

           HLSLPROGRAM
           #pragma target 2.0
           #pragma vertex vert
           #pragma fragment frag

           #include "./PotaToonInput.hlsl"

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

           float frag(Varyings input) : SV_TARGET
           {
               return input.positionCS.z;
           }
           ENDHLSL
       }
       Pass
       {
           Name "TransparentOutline"
           Tags {
               "LightMode" = "TransparentOutline"
           }
           Cull Front
           ColorMask RGBA
           Stencil
            {
                Ref[_StencilRef]
                Comp[_StencilComp]
                Pass[_StencilOpPass]
                Fail[_StencilOpFail]
                ZFail[_StencilOpZFail]
            }
           
           HLSLPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           #pragma target 4.5
           
           #pragma multi_compile_fragment _ _POTA_TOON_OIT
           
           #include "./PotaToonInput.hlsl"
           #include "./PotaToonOutlinePass.hlsl"
           
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
           
           #include "./PotaToonCharMaskPass.hlsl"
           
           ENDHLSL
       }
    }
    CustomEditor "PotaToon.Editor.PotaToonShaderGUI"
}
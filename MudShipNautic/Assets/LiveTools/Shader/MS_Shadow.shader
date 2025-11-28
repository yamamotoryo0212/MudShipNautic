Shader "MudShip/MS_Shadow"
{
    Properties
    {
        _ShadowAlpha("Shadow Alpha", float) = 1
        _ShadowColor("Shadow Color", Color) = (0, 0, 0, 1)
        _ShadowOffset("Shadow Sample Offset", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry+1"
        }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            
            // -------------------------------------
            // Universal Render Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                      
            CBUFFER_START(UnityPerMaterial)
            float _ShadowAlpha;
            float4 _ShadowColor;
            float4 _ShadowOffset;
            CBUFFER_END
            
            struct appdata
            {
                float4 vertex : POSITION;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 posWS : TEXCOORD0;
                float fogFactor: TEXCOORD1;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.posWS = TransformObjectToWorld(v.vertex.xyz);
                o.fogFactor = ComputeFogFactor(o.vertex.z);
                return o;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                // シャドウマップのサンプリング位置をずらす
                float3 biasedPosWS = i.posWS + _ShadowOffset.xyz;
                float4 shadowCoord = TransformWorldToShadowCoord(biasedPosWS);
                
                Light mainLight = GetMainLight(shadowCoord);
                half shadow = mainLight.shadowAttenuation;
                float4 col = float4(_ShadowColor.rgb, (1 - shadow) * _ShadowAlpha);
                col.rgb = MixFog(col.rgb, i.fogFactor);
                return col;
            }
            ENDHLSL
        }
        // Used for rendering shadowmaps
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
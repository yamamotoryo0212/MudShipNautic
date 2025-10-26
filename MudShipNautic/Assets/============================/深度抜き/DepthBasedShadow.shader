Shader "Custom/URP/GroundShadowFromDepth"
{
    Properties
    {
        _DepthTexture ("Depth Texture (R16_UNORM)", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 0.7)
        _ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 0.5
        _ShadowSharpness ("Shadow Sharpness", Range(0.1, 10)) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float fogFactor : TEXCOORD1;
            };
            
            TEXTURE2D(_DepthTexture);
            SAMPLER(sampler_DepthTexture);
            
            float4x4 _DepthViewProj;
            float _DepthCameraNear;
            float _DepthCameraFar;
            
            CBUFFER_START(UnityPerMaterial)
                float4 _ShadowColor;
                float _ShadowIntensity;
                float _ShadowSharpness;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.fogFactor = ComputeFogFactor(posInputs.positionCS.z);
                
                return output;
            }
            
            float CalculateShadow(float3 worldPos)
            {
                // ワールド座標を深度カメラのクリップ空間に変換
                float4 depthClipPos = mul(_DepthViewProj, float4(worldPos, 1.0));
                
                // 透視除算
                float3 depthNDC = depthClipPos.xyz / depthClipPos.w;
                
                // NDC [-1,1] を UV [0,1] に変換
                float2 depthUV = depthNDC.xy * 0.5 + 0.5;
                
                // UVが範囲外なら影なし
                if (depthUV.x < 0.0 || depthUV.x > 1.0 || 
                    depthUV.y < 0.0 || depthUV.y > 1.0)
                {
                    return 0.0;
                }
                
                // 深度カメラの範囲外（後ろ側）なら影なし
                if (depthNDC.z < 0.0 || depthNDC.z > 1.0)
                {
                    return 0.0;
                }
                
                // 深度テクスチャをサンプリング
                float sampledDepth = SAMPLE_TEXTURE2D(_DepthTexture, sampler_DepthTexture, depthUV).r;
                
                // 現在のピクセルの深度
                float currentDepth = depthNDC.z;
                
                // 深度差分を計算（サンプル深度 - 現在深度）
                // 正の値 = オブジェクトが手前にある = 影
                float depthDiff = currentDepth - sampledDepth;
                
                // 小さな差分は影と判定しない（バイアス）
                if (depthDiff < 0.001)
                {
                    return 0.0;
                }
                
                // 影の強度を計算
                float shadow = saturate(depthDiff * _ShadowSharpness * 1000.0);
                
                return shadow * _ShadowIntensity;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                // 影を計算
                float shadow = CalculateShadow(input.positionWS);
                
                // 影がない場合は完全に透過
                if (shadow <= 0.0)
                {
                    discard;
                }
                
                // 影の色とアルファを設定
                float4 shadowColor = _ShadowColor;
                shadowColor.a *= shadow;
                
                // フォグを適用
                shadowColor.rgb = MixFog(shadowColor.rgb, input.fogFactor);
                
                return shadowColor;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}
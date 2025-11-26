Shader "Custom/AudioSpectrumGradient"
{
    Properties
    {
        _ColorStart ("Start Color", Color) = (0, 1, 1, 1)
        _ColorEnd ("End Color", Color) = (1, 0, 1, 1)
        _ColorMid ("Mid Color", Color) = (0, 0.5, 1, 1)
        
        [Header(Gradient Settings)]
        _GradientCenter ("Gradient Center", Range(0, 1)) = 0.5
        _GradientWidth ("Gradient Width", Range(0.1, 2)) = 1.0
        _GradientPower ("Gradient Power", Range(0.1, 5)) = 1.0
        
        [Header(Glow Effect)]
        _EmissionStrength ("Emission Strength", Range(0, 10)) = 2.0
        _RimPower ("Rim Power", Range(0.1, 8)) = 3.0
        _RimIntensity ("Rim Intensity", Range(0, 5)) = 1.5
        
        [Header(Pulse Animation)]
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 2.0
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.2
        
        [Header(Scan Line Effect)]
        _ScanLineSpeed ("Scan Line Speed", Range(0, 10)) = 1.0
        _ScanLineWidth ("Scan Line Width", Range(0, 1)) = 0.1
        _ScanLineIntensity ("Scan Line Intensity", Range(0, 5)) = 2.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 100
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float fogFactor : TEXCOORD4;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _ColorStart;
                float4 _ColorEnd;
                float4 _ColorMid;
                float _GradientCenter;
                float _GradientWidth;
                float _GradientPower;
                float _EmissionStrength;
                float _RimPower;
                float _RimIntensity;
                float _PulseSpeed;
                float _PulseAmount;
                float _ScanLineSpeed;
                float _ScanLineWidth;
                float _ScanLineIntensity;
            CBUFFER_END
            
            // グラデーションの基準位置（全オブジェクトで共有）
            float _GradientOrigin;
            float _GradientRange;
            
            // グラデーション計算関数
            float3 CalculateGradient(float worldPosX)
            {
                // グラデーションの基準位置からの相対位置を計算
                float relativePos = worldPosX - _GradientOrigin;
                
                // 範囲を0-1に正規化
                float gradient = relativePos / _GradientRange;
                
                // Gradient Centerを適用（中心位置のオフセット）
                gradient = gradient - _GradientCenter + 0.5;
                
                // Gradient Widthを適用（グラデーションの広がり）
                gradient = (gradient - 0.5) * _GradientWidth + 0.5;
                
                // 0-1の範囲にクランプ
                gradient = saturate(gradient);
                
                // グラデーションカーブの調整（Gradient Power）
                gradient = pow(gradient, _GradientPower);
                
                // 3色グラデーション
                float3 color;
                if (gradient < 0.5)
                {
                    color = lerp(_ColorStart.rgb, _ColorMid.rgb, gradient * 2.0);
                }
                else
                {
                    color = lerp(_ColorMid.rgb, _ColorEnd.rgb, (gradient - 0.5) * 2.0);
                }
                
                return color;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionHCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.uv = input.uv;
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                // 基本的なグラデーション色
                float3 baseColor = CalculateGradient(input.positionWS.x);
                
                // パルス効果（時間経過で明滅）
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                pulse = lerp(1.0, pulse, _PulseAmount);
                
                // スキャンライン効果
                float scanLine = frac((input.positionWS.y + _Time.y * _ScanLineSpeed) * 5.0);
                scanLine = smoothstep(0.5 - _ScanLineWidth, 0.5, scanLine) * 
                           smoothstep(0.5 + _ScanLineWidth, 0.5, scanLine);
                scanLine *= _ScanLineIntensity;
                
                // リムライト（エッジ光沢）
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                float rimDot = 1.0 - saturate(dot(normalWS, viewDirWS));
                float rim = pow(rimDot, _RimPower) * _RimIntensity;
                
                // 最終カラー合成
                float3 finalColor = baseColor * pulse;
                finalColor += baseColor * rim;
                finalColor += baseColor * scanLine;
                finalColor *= _EmissionStrength;
                
                // フォグ適用
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
Shader "Custom/URP/RippleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _RippleCount ("Ripple Count", Int) = 10
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float fogFactor : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                int _RippleCount;
            CBUFFER_END

            // 波紋データ: xyz = 位置, w = 開始時刻
            float4 _RippleData[10];
            float _RippleSpeed;
            float _RippleWidth;
            float _RippleStrength;

            Varyings vert (Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                
                float rippleEffect = 0.0;
                float currentTime = _Time.y;
                
                // 各波紋を計算
                for(int i = 0; i < _RippleCount; i++)
                {
                    float3 ripplePos = _RippleData[i].xyz;
                    float startTime = _RippleData[i].w;
                    
                    // 波紋が有効かチェック（startTime > 0）
                    if(startTime > 0.0)
                    {
                        float timeSinceStart = currentTime - startTime;
                        
                        // 波紋の最大持続時間
                        float maxDuration = 3.0;
                        
                        if(timeSinceStart < maxDuration)
                        {
                            // ピクセルと波紋中心の距離
                            float dist = distance(input.positionWS.xz, ripplePos.xz);
                            
                            // 波紋の半径
                            float rippleRadius = timeSinceStart * _RippleSpeed;
                            
                            // 波の形状を計算
                            float wave = abs(dist - rippleRadius);
                            
                            // 波紋の幅内にあるかチェック
                            if(wave < _RippleWidth)
                            {
                                // 波の強度（中心が最も強い）
                                float waveIntensity = 1.0 - (wave / _RippleWidth);
                                
                                // 時間経過で減衰
                                float fadeOut = 1.0 - (timeSinceStart / maxDuration);
                                
                                // 波紋効果を加算
                                rippleEffect += waveIntensity * fadeOut * _RippleStrength;
                            }
                        }
                    }
                }
                
                // 波紋効果を色に適用（明るくする）
                col.rgb += rippleEffect;
                
                // フォグを適用
                col.rgb = MixFog(col.rgb, input.fogFactor);
                
                return col;
            }
            ENDHLSL
        }
    }
}
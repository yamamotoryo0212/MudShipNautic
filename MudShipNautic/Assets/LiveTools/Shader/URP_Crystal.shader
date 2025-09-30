Shader "Custom/URP_Crystal"
{
    Properties
    {
        [Header(Crystal Base)]
        _CrystalColor ("Crystal Color", Color) = (1, 0.2, 0.2, 1)
        _ColorIntensity ("Color Intensity", Range(0, 3)) = 1.5
        _CrystalTint ("Crystal Tint", Range(0, 1)) = 0.8
        
        [Header(Transparency)]
        _IOR ("Index of Refraction", Range(1.0, 3.0)) = 1.52
        _Thickness ("Thickness", Range(0.001, 0.1)) = 0.01
        _AbsorptionStrength ("Absorption Strength", Range(0, 5)) = 2.0
        _Transparency ("Transparency", Range(0, 1)) = 0.9
        _MinTransparency ("Min Transparency", Range(0, 1)) = 0.1
        
        [Header(Surface)]
        _Smoothness ("Smoothness", Range(0, 1)) = 0.95
        _Metallic ("Metallic", Range(0, 1)) = 0.1
        _FresnelIntensity ("Fresnel Intensity", Range(0, 2)) = 1.0
        
        [Header(Internal Structure)]
        _InternalComplexity ("Internal Complexity", Range(0, 2)) = 1.0
        _CrackIntensity ("Crack Intensity", Range(0, 1)) = 0.3
        _CrackScale ("Crack Scale", Range(1, 20)) = 8.0
        
        [Header(Environment)]
        _ReflectionCube ("Reflection Cubemap", Cube) = "black" {}
        _ReflectionIntensity ("Reflection Intensity", Range(0, 2)) = 1.0
        _EnvironmentBlur ("Environment Blur", Range(0, 7)) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent-100" 
            "RenderPipeline" = "UniversalPipeline"
        }
        
        // Front face pass
        Pass
        {
            Name "CrystalFront"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float2 uv : TEXCOORD4;
                float4 screenPos : TEXCOORD5;
                float3 viewDirWS : TEXCOORD6;
                float4 shadowCoord : TEXCOORD7;
                float depth : TEXCOORD8;
            };
            
            TEXTURECUBE(_ReflectionCube);
            SAMPLER(sampler_ReflectionCube);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _CrystalColor;
                float _ColorIntensity;
                float _CrystalTint;
                float _IOR;
                float _Thickness;
                float _AbsorptionStrength;
                float _Transparency;
                float _MinTransparency;
                float _Smoothness;
                float _Metallic;
                float _FresnelIntensity;
                float _InternalComplexity;
                float _CrackIntensity;
                float _CrackScale;
                float _ReflectionIntensity;
                float _EnvironmentBlur;
            CBUFFER_END
            
            // 高品質ノイズ関数
            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }
            
            float noise3D(float3 x)
            {
                float3 i = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                
                return lerp(lerp(lerp(hash(i + float3(0, 0, 0)),
                                     hash(i + float3(1, 0, 0)), f.x),
                                lerp(hash(i + float3(0, 1, 0)),
                                     hash(i + float3(1, 1, 0)), f.x), f.y),
                           lerp(lerp(hash(i + float3(0, 0, 1)),
                                     hash(i + float3(1, 0, 1)), f.x),
                                lerp(hash(i + float3(0, 1, 1)),
                                     hash(i + float3(1, 1, 1)), f.x), f.y), f.z);
            }
            
            // フラクタルノイズ
            float fbm(float3 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                for(int i = 0; i < 4; i++)
                {
                    value += amplitude * noise3D(p * frequency);
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }
                return value;
            }
            
            // 内部構造の計算
            float3 calculateInternalStructure(float3 worldPos, float3 viewDir)
            {
                float3 p = worldPos * _CrackScale;
                
                // 内部のクラック構造
                float cracks = fbm(p + _Time.y * 0.1);
                cracks = pow(cracks, 2.0);
                
                // 内部の光の散乱
                float scattering = fbm(p * 0.5 + float3(_Time.y * 0.05, 0, 0));
                scattering = smoothstep(0.3, 0.7, scattering);
                
                return lerp(1.0, cracks + scattering * 0.5, _CrackIntensity);
            }
            
            // Schlickフレネル近似
            float3 fresnelSchlick(float cosTheta, float3 F0)
            {
                return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
            }
            
            // 色の吸収計算（ベールの法則）
            float3 calculateAbsorption(float3 color, float distance)
            {
                float3 absorption = exp(-_AbsorptionStrength * distance * (1.0 - color));
                return absorption;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(vertexInput.positionCS);
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.shadowCoord = GetShadowCoord(vertexInput);
                output.depth = -TransformWorldToView(vertexInput.positionWS).z;
                
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                // 基本ベクトル
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                
                // ライティング
                Light mainLight = GetMainLight(input.shadowCoord);
                float3 lightDirWS = mainLight.direction;
                float lightAttenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                
                // 基本的な物理値
                float NdotV = saturate(dot(normalWS, viewDirWS));
                float NdotL = saturate(dot(normalWS, lightDirWS));
                float3 F0 = lerp(0.04, _CrystalColor.rgb, _Metallic);
                
                // フレネル効果
                float3 fresnel = fresnelSchlick(NdotV, F0) * _FresnelIntensity;
                
                // 屈折計算
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float3 refractionDir = refract(-viewDirWS, normalWS, 1.0 / _IOR);
                float2 refractionOffset = refractionDir.xy * _Thickness;
                
                // 背景色の取得（屈折あり）
                float3 backgroundColor = SampleSceneColor(screenUV + refractionOffset).rgb;
                
                // 内部構造の計算（ライティングに依存）
                float3 internalStructure = calculateInternalStructure(input.positionWS, viewDirWS);
                float lightInfluence = saturate(NdotL + 0.2); // 最低20%の明度を保つ
                internalStructure *= lightInfluence;
                
                // 色の吸収（距離ベース）
                float thickness = _Thickness * (2.0 - NdotV); // 角度による厚み調整
                float3 absorption = calculateAbsorption(_CrystalColor.rgb, thickness);
                
                // クリスタルの基本色（ライティングを考慮）
                float3 crystalColor = _CrystalColor.rgb * _ColorIntensity;
                
                // アンビエント光の計算
                float3 ambientColor = unity_AmbientSky.rgb * 0.3;
                
                // ライティングされた色
                float3 litColor = crystalColor * (NdotL * lightAttenuation * mainLight.color + ambientColor);
                
                // 透過色の計算（ライティング情報を含む）
                float3 transmissionColor = lerp(backgroundColor, backgroundColor * absorption * litColor, _CrystalTint);
                transmissionColor *= internalStructure;
                
                // 環境反射（ライティング考慮）
                float3 reflectionDir = reflect(-viewDirWS, normalWS);
                float3 environmentColor = SAMPLE_TEXTURECUBE_LOD(_ReflectionCube, sampler_ReflectionCube, 
                                                               reflectionDir, _EnvironmentBlur).rgb;
                environmentColor *= _ReflectionIntensity * lightInfluence;
                
                // スペキュラハイライト（ライトがある場合のみ）
                float3 specularColor = 0;
                if (lightAttenuation > 0.01)
                {
                    float3 halfwayDir = normalize(lightDirWS + viewDirWS);
                    float NdotH = saturate(dot(normalWS, halfwayDir));
                    float specular = pow(NdotH, lerp(1, 512, _Smoothness));
                    specularColor = mainLight.color * specular * fresnel * lightAttenuation;
                }
                
                // 最終色の合成
                float3 finalColor = 0;
                
                // 透過色をベースに
                finalColor += transmissionColor;
                
                // 反射を透明度に応じて混合
                float reflectionMix = fresnel.r * lightInfluence;
                finalColor = lerp(finalColor, environmentColor, reflectionMix * 0.5);
                
                // スペキュラハイライト追加
                finalColor += specularColor;
                
                // 暗い場所では色を暗くする
                finalColor *= saturate(lightInfluence + 0.1);
                
                // アルファ計算（より透明に）
                float baseAlpha = lerp(_MinTransparency, _Transparency, NdotV);
                float alpha = lerp(baseAlpha, 1.0, fresnel.r * 0.3);
                alpha = saturate(alpha);
                
                return float4(finalColor, alpha);
            }
            ENDHLSL
        }
        
        // Back face pass (内部反射用)
        Pass
        {
            Name "CrystalBack"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Front
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragBack
            
            // 同じインクルードとstructure
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            // 同じvertex shaderとCBuffer
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _CrystalColor;
                float _ColorIntensity;
                float _InternalComplexity;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                
                return output;
            }
            
            float4 fragBack(Varyings input) : SV_Target
            {
                // 背面は非常に暗く、ライティングに依存
                Light mainLight = GetMainLight();
                float3 normalWS = normalize(-input.normalWS); // 背面なので法線を反転
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                float lightAttenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                
                float3 color = _CrystalColor.rgb * _ColorIntensity * 0.1 * NdotL * lightAttenuation;
                color += unity_AmbientSky.rgb * _CrystalColor.rgb * 0.05; // 微量のアンビエント
                
                return float4(color, 0.05);
            }
            ENDHLSL
        }
        
        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
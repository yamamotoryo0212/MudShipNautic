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
                // ���[���h���W��[�x�J�����̃N���b�v��Ԃɕϊ�
                float4 depthClipPos = mul(_DepthViewProj, float4(worldPos, 1.0));
                
                // �������Z
                float3 depthNDC = depthClipPos.xyz / depthClipPos.w;
                
                // NDC [-1,1] �� UV [0,1] �ɕϊ�
                float2 depthUV = depthNDC.xy * 0.5 + 0.5;
                
                // UV���͈͊O�Ȃ�e�Ȃ�
                if (depthUV.x < 0.0 || depthUV.x > 1.0 || 
                    depthUV.y < 0.0 || depthUV.y > 1.0)
                {
                    return 0.0;
                }
                
                // �[�x�J�����͈̔͊O�i��둤�j�Ȃ�e�Ȃ�
                if (depthNDC.z < 0.0 || depthNDC.z > 1.0)
                {
                    return 0.0;
                }
                
                // �[�x�e�N�X�`�����T���v�����O
                float sampledDepth = SAMPLE_TEXTURE2D(_DepthTexture, sampler_DepthTexture, depthUV).r;
                
                // ���݂̃s�N�Z���̐[�x
                float currentDepth = depthNDC.z;
                
                // �[�x�������v�Z�i�T���v���[�x - ���ݐ[�x�j
                // ���̒l = �I�u�W�F�N�g����O�ɂ��� = �e
                float depthDiff = currentDepth - sampledDepth;
                
                // �����ȍ����͉e�Ɣ��肵�Ȃ��i�o�C�A�X�j
                if (depthDiff < 0.001)
                {
                    return 0.0;
                }
                
                // �e�̋��x���v�Z
                float shadow = saturate(depthDiff * _ShadowSharpness * 1000.0);
                
                return shadow * _ShadowIntensity;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                // �e���v�Z
                float shadow = CalculateShadow(input.positionWS);
                
                // �e���Ȃ��ꍇ�͊��S�ɓ���
                if (shadow <= 0.0)
                {
                    discard;
                }
                
                // �e�̐F�ƃA���t�@��ݒ�
                float4 shadowColor = _ShadowColor;
                shadowColor.a *= shadow;
                
                // �t�H�O��K�p
                shadowColor.rgb = MixFog(shadowColor.rgb, input.fogFactor);
                
                return shadowColor;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}
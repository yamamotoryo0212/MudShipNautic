Shader "MudShip/MS_DropShadow"
{
    Properties
    {
        _ShadowColor("Shadow Color", Color) = (0, 0, 0, 0.5)
        _GroundY("Ground Y Position", Float) = 0
        _ShadowDirection("Shadow Direction (XZ)", Vector) = (0, 0, 1, 0)
        _ShadowLength("Shadow Length", Float) = 1
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent"
            "Queue"="Transparent-1"
            "RenderPipeline"="UniversalPipeline"
        }
        
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            Offset -1, -1
            
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
                float fogFactor : TEXCOORD0;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _ShadowColor;
                float _GroundY;
                float4 _ShadowDirection;
                float _ShadowLength;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // ワールド空間の頂点位置
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                
                // 地面からの高さを計算
                float heightAboveGround = worldPos.y - _GroundY;
                
                // 影の方向（XZ平面）を正規化
                float2 shadowDir = normalize(_ShadowDirection.xz);
                
                // 高さに応じて影をXZ方向にオフセット
                worldPos.xz += shadowDir * heightAboveGround * _ShadowLength;
                
                // Y座標を地面に固定
                worldPos.y = _GroundY + 0.01; // 地面より少し上に配置してZ-fightingを防ぐ
                
                // クリップ空間へ変換
                output.positionCS = TransformWorldToHClip(worldPos);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                half4 color = _ShadowColor;
                color.rgb = MixFog(color.rgb, input.fogFactor);
                return color;
            }
            ENDHLSL
        }
    }
}
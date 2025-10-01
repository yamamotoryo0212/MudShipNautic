Shader "Custom/Channel Merger"
{
    Properties
    {
        _TexA ("Render Texture A (RGB)", 2D) = "white" {}
        _TexB ("Render Texture B (Alpha)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            // �|�X�g�v���Z�X��Blit�����ɓK�����ݒ�
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // �e�N�X�`���錾
            sampler2D _TexA;
            sampler2D _TexB;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. RenderTextureA (RGB�\�[�X) ���T���v�����O
                fixed4 colorA = tex2D(_TexA, i.uv);
                
                // 2. RenderTextureB (Alpha�\�[�X) ���T���v�����O
                fixed4 colorB = tex2D(_TexB, i.uv);

                // 3. �`�����l��������
                // R�`�����l��: A��R
                // G�`�����l��: A��G
                // B�`�����l��: A��B
                // A�`�����l��: B��A
                fixed4 result;
                result.rgb = colorA.rgb;
                result.a = colorB.a;

                return result;
            }
            ENDHLSL
        }
    }
}
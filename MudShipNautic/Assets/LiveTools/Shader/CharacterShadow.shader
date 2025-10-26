Shader "Custom/CharacterShadow"
{
    Properties
    {
        _MainTex ("Render Texture", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 0.8)
        _ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 1.0
        [Toggle] _DebugAlpha ("Debug Alpha Channel", Float) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }
        
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _DEBUGALPHA_ON
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
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _ShadowColor;
            float _ShadowIntensity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // RenderTexture����e�N�X�`�����擾
                fixed4 tex = tex2D(_MainTex, i.uv);
                
                #ifdef _DEBUGALPHA_ON
                    // �f�o�b�O���[�h: �A���t�@�l�𔒍��ŕ\��
                    return fixed4(tex.aaa, 1.0);
                #else
                    // �ʏ탂�[�h: �A���t�@���e�Ƃ��ĕ\��
                    fixed shadowAlpha = tex.a * _ShadowIntensity;
                    
                    fixed4 col = _ShadowColor;
                    col.a = shadowAlpha;
                    
                    return col;
                #endif
            }
            ENDCG
        }
    }
    
    Fallback "Transparent/VertexLit"
}
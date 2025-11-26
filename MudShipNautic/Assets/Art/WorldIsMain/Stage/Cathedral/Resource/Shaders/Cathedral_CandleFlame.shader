Shader "Cathedral/Cathedral_CandleFlame"
{
    Properties
    {
        [HDR][NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}
        _SubUV("Sub UV",float) = 4.0
        _Speed("Speed",float) = 24.0
        _Cutoff("Cutoff",Range(0,1)) = 0.5

        [HDR]_ColorTint("Color Tint",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "AlphaTest"}
        
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _SubUV;
            float _Speed;
            fixed4 _ColorTint;
            fixed _Cutoff;


            v2f vert (appdata v)
            {
                v2f o;

                float4 origin = float4(0, 0, 0, 1);
                float4 worldOrigin = mul(UNITY_MATRIX_M, origin);
                float4 viewOrigin = mul(UNITY_MATRIX_V, worldOrigin);
                float4 worldToViewTranslation = viewOrigin - worldOrigin;

                float4 worldPosition = mul(UNITY_MATRIX_M, v.vertex);
                float4 viewPosition = worldPosition + worldToViewTranslation;
                float4 clipPosition = mul(UNITY_MATRIX_P, viewPosition);

                o.vertex = clipPosition;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y * _Speed;      
                float sub = _SubUV;

                uv.x += (int)fmod(time,sub);
                uv.y -= (int)fmod(time / sub, sub);

                uv.x /= sub;
                uv.y /= sub;
                    

                fixed4 col = tex2D(_MainTex, uv) * _ColorTint;
                clip(col.a - _Cutoff);
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}

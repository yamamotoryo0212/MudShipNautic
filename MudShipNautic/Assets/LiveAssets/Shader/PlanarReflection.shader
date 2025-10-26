Shader "MudShip/PlanarReflectionTransparent (Toggle Clip)"
{
    Properties
    {
        // 既存のプロパティ
        _Color("Base Color", Color) = (1, 1, 1, 1)
        _MainTex("Main Texture", 2D) = "white" {}
        _ReflectionTex("Reflection Texture", 2D) = "white" {}
        _reflectionFactor("Reflection Factor", Range(0, 1)) = 1.0
        _Roughness("Roughness", Range(0, 1)) = 0.0
        _BlurRadius("Blur Radius", Range(0, 10)) = 2.5
        _Alpha("Alpha", Range(0, 1)) = 1.0
        
        // --- Alpha Clipping用のトグルとCutoffを追加 (既存機能の変更なし) ---
        [Toggle(_ALPHACLIP_ON)] _AlphaClipToggle("Alpha Clipping (ON/OFF)", Float) = 0
        [Gamma] _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5
        // ---------------------------------------------------------------------
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            // --- トグル機能のためのシェーダーフィーチャーを追加 ---
            #pragma shader_feature _ALPHACLIP_ON
            // --------------------------------------------------------

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD1;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD1;
                float4 screenPos : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            // パラメータ宣言
            float4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _ReflectionTex;
            float4 _ReflectionTex_ST;
            float _reflectionFactor;
            float _Roughness;
            float _BlurRadius;
            float _Alpha;
            // --- 追加したパラメータ ---
            float _Cutoff;
            // --------------------------

            float gaussianWeight(float x, float sigma)
            {
                return exp(-(x * x) / (2.0 * sigma * sigma));
            }
            
            // ガウスぼかしサンプリング関数
            half4 gaussianBlur(sampler2D tex, float2 uv, float blurAmount)
            {
                if (blurAmount <= 0.001)
                {
                    return tex2D(tex, uv);
                }
                
                half4 color = half4(0, 0, 0, 0);
                float totalWeight = 0.0;
                float2 texelSize = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);
                
                int sampleCount = (int)lerp(3, 9, _Roughness); 
                float stepSize = blurAmount * _BlurRadius;
                float sigma = stepSize * 0.5;
                
                for (int x = -sampleCount; x <= sampleCount; x++)
                {
                    for (int y = -sampleCount; y <= sampleCount; y++)
                    {
                        float2 offset = float2(x, y) * texelSize * stepSize;
                        float2 sampleUV = uv + offset;
                        
                        if (sampleUV.x >= 0.0 && sampleUV.x <= 1.0 &&
                            sampleUV.y >= 0.0 && sampleUV.y <= 1.0)
                        {
                            float distance = length(float2(x, y));
                            float weight = gaussianWeight(distance, sigma);
                            color += tex2D(tex, sampleUV) * weight;
                            totalWeight += weight;
                        }
                    }
                }
                return color / totalWeight;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex); 
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // メインテクスチャをサンプリング (既存ロジック)
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                half4 tex_col = tex2D(_MainTex, i.uv);
                
                // ベースカラーを適用 (既存ロジック)
                fixed4 baseColor = tex_col * _Color;
                
                // 反射UVを計算 (既存ロジック)
                float2 reflectionUV = float2(1 - screenUV.x, screenUV.y);
                
                // ガウスぼかしを適用 (既存ロジック)
                half4 reflectionColor = gaussianBlur(_ReflectionTex, reflectionUV, _Roughness);
                
                // 反射色にもベースカラーを適用 (既存ロジック)
                fixed4 tintedReflection = reflectionColor * _Color;
                
                // アルファ値の計算 (既存ロジック)
                float baseAlpha = tex_col.a * _Color.a * _Alpha;
                
                // --- トグルがONの時だけ Alpha Clipping を実行 ---
                #if _ALPHACLIP_ON
                    // ベースアルファがCutoff未満の場合、ピクセルを破棄
                    clip(baseAlpha - _Cutoff);
                #endif
                // ----------------------------------------------
                
                // 最終的な色の計算 (既存ロジック)
                fixed4 col;
                float reflectionAlpha = tintedReflection.a; 
                
                // RGBは反射とベースのブレンド (既存ロジック)
                col.rgb = lerp(baseColor.rgb, tintedReflection.rgb, _reflectionFactor);
                
                // アルファ値もブレンド (既存ロジック)
                col.a = lerp(baseAlpha, reflectionAlpha, _reflectionFactor);

                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/Diffuse"
}

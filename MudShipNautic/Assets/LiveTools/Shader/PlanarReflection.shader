Shader "MudShip/PlanarReflection"
{
    Properties
    {
        _Color("Base Color", Color) = (1, 1, 1, 1)
        _MainTex("Main Texture", 2D) = "white" {}
        _ReflectionTex("Reflection Texture", 2D) = "white" {} // PlanarReflectionスクリプトで渡されるリフレクションテクスチャ
        _reflectionFactor("Reflection Factor", Range(0, 1)) = 1.0 // 反射強度 0:反射なし ベースカラーのみ　1:完全に反射のみ
        _Roughness("Roughness", Range(0, 1)) = 0.0 // ぼかし強さ 0:ぼかし無し 1:最大ぼかし
        _BlurRadius("Blur Radius", Range(0, 10)) = 5.0 // ぼかし量を制御
    }
    SubShader
    {
        // "Queue"="Geometry" → 描画順をGeometryキュー（通常の不透明オブジェクト：2000）に設定
        // "RenderType"="Opaque" → 不透明オブジェクトとして扱う（レンダリングパスやポストプロセスで使用）
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }

        // 深度バッファでの位置を調整する。
        // 負の値でカメラに近づける → 同じ座標の他オブジェクトよりも優先的に描画される
        // 今回は Offset -1, -1 で、わずかに手前扱いにする
        Offset -1, -1

        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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
            float4 _BaseColor;

            // ガウス重み関数
            float gaussianWeight(float x, float sigma)
            {
                // ガウス重み: exp(-x²/(2σ²))
                return exp(-(x * x) / (2.0 * sigma * sigma));
            }
            // ガウスぼかしサンプリング関数
            half4 gaussianBlur(sampler2D tex, float2 uv, float blurAmount)
            {
                // _Roughness が小さい場合は計算コストを避けるためオリジナルテクスチャをそのまま返す
                if (blurAmount <= 0.001)
                {
                    return tex2D(tex, uv);
                }
                half4 color = half4(0, 0, 0, 0);
                float totalWeight = 0.0;
                // ピクセルサイズ: 画面解像度から取得
                float2 texelSize = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);
                // 動的にサンプル範囲とステップを調整
                int sampleCount = (int)lerp(3, 9, _Roughness);
                float stepSize = blurAmount * _BlurRadius;
                float sigma = stepSize * 0.5;
                // 2次元畳み込み: 水平方向・垂直方向のぼかし
                for (int x = -sampleCount; x <= sampleCount; x++)
                {
                    for (int y = -sampleCount; y <= sampleCount; y++)
                    {
                        // オフセットの計算（ピクセル空間→UV空間）
                        float2 offset = float2(x, y) * texelSize * stepSize;
                        float2 sampleUV = uv + offset;
                        // 境界チェック: UVが 0～1 の範囲内か確認
                        if (sampleUV.x >= 0.0 && sampleUV.x <= 1.0 &&
                            sampleUV.y >= 0.0 && sampleUV.y <= 1.0)
                        {
                            // 中心からの距離を計算
                            float distance = length(float2(x, y));
                            // ガウス重みを取得
                            float weight = gaussianWeight(distance, sigma);
                            // 重み付きで色を加算
                            color += tex2D(tex, sampleUV) * weight;
                            totalWeight += weight;
                        }
                    }
                }
                // 正規化した色を返す（加重平均）
                return color / totalWeight;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex); // スクリーン座標に変換。これがないと反射描画が正しくならない
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // カメラが描画した位置のUVを取得
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                half4 tex_col = tex2D(_MainTex, i.uv);

                // screenUV の X を反転して鏡面UVとする
                float2 reflectionUV = float2(1 - screenUV.x, screenUV.y);
                // ガウスぼかしを適用
                half4 reflectionColor = gaussianBlur(_ReflectionTex, reflectionUV, _Roughness);
                // 反射とメインテクスチャを混合
                fixed4 col = tex_col * _BaseColor * reflectionColor;
                col = lerp(tex_col * _BaseColor, col, _reflectionFactor);

                return col;
            }
            ENDCG
        }
    }
}

Shader "Custom/mist/mist_notUseLights"
{
    Properties
    {


         [NoScaleOffset]  _MainTex ("Texture", 2D) = "white" {}
 

         [Header(_)]
         [Header(soft edge on contact)]


        [Toggle] _softToggle("            on" , range(0,1))=1
        _soft ("      soft on contact  " , float ) = 1


         [Header(_)]
         [Header(less alpha near camera)]

        [Toggle] _nearToggle("            on       ( scale * 0.1 ) " , range(0,1))=1
        _nearCam ("      less alpha    near  " , float ) = 0
        _farCam ("      less alpha    far" , float ) = .64



         [Header(_)]
         [Header(distance color)]

        [Toggle] _distanceColorToggle("            on        ( scale * 1.0 ) " , range(0,1))=0
        _distantCol("       distance    color" , Color) = (1,1,1,1)
        _nearDistance ("       distance    near  " , float ) = 20
        _farDistance ("       distance    far  " , float ) = 100
        _distanceColorRate("       distance    color rate " , range(0,1))=1
        [Toggle] _inheritCol ("       multiply particle color for ditance color " , range(0,1))=0

         [Header(_)]
        [Toggle] _useOrigColToggle("     use particle original color" , int)=1
        _multiplyCol ("       multiply    color" , Color) = (1,1,1,1)



    }
    SubShader
    {

        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
        Cull Back
        Lighting Off
        ZWrite Off

        Pass

                {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

                //
                sampler2D _MainTex;
                vector _multiplyCol ;
                int _useOrigColToggle ;
                int _softToggle ;
                float _soft ;
                int _nearToggle ;
                int _inheritCol ;
                float _nearCam ;
                float _farCam ;
                vector _distantCol ;


                int _distanceColorToggle ;

                float _distanceColorRate ;
                float _nearDistance ;
                float _farDistance ;

                sampler2D _CameraDepthTexture;


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 pCol : COLOR ;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : WORLD_POS;
                float2 uv : TEXCOORD0;

                    float4 projPos : TEXCOORD2;
                    half depth : TEXCOORD1;
                float4 pCol : COLOR ;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); 

                    if(_softToggle==1){    o.projPos = ComputeScreenPos(o.vertex);
                                            COMPUTE_EYEDEPTH(o.depth);
                                        }
                o.pCol = v.pCol ;
                    

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {



                    
                float4 col = tex2D(_MainTex, i.uv);
                float br = length(col.xyz);
                col *= _multiplyCol;
                float br2 = length ( _multiplyCol.xyz ) ;
    
                  
                    
                   float le,le2;
                if(_nearToggle){
                    le = length(_WorldSpaceCameraPos - i.worldPos ) ;//cameraToObjLength ;
                    le2 = le;
                    le*=.1;
                }

                if(_nearToggle){
                    //le = lerp(  _nearCam, _farCam , le );
                    float near = _nearCam;
                    float far = _farCam;
                    //if(far<near)near = far;
                    le = (le-near)/(far - near) ;//+near ;
                    le = clamp(le,0,1);
                    col.a *= le ;
                }

                    

            
                    if(_softToggle==1 && _soft > 0 ) {
                        half bgDepth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, i.projPos).r);
                        float alpha = saturate((bgDepth - i.depth) /_soft);

                        col.a*=alpha ;
                    }

                    if(_useOrigColToggle)col.rgb *= i.pCol.rgb ;
                    col.a *= i.pCol.a ;

                

                if(_distanceColorToggle){
                    float nearB = _nearDistance;
                    float farB = _farDistance;
                    le2 = ( le2 - nearB ) / ( farB - nearB ) ;//+ nearB ;
                    le2 = clamp ( le2 , 0 , 1 ) ;
                    le2 *= _distanceColorRate ;
                    col.rgb = lerp ( col.rgb , _distantCol.rgb*lerp ( 1 , col.rgb , _inheritCol)  , le2 );
                    col.a*= lerp ( 1, _distantCol.a , pow(le2 , .64) ) ;
                }
                
                

                return col;
            }
            ENDCG
        }
    }
}
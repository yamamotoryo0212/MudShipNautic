Shader "Custom/mist/mist_useLight"
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

         [Header(_)]
         [Header(lights)]
        [Toggle] _useLights ("               use lights" ,int )=1
        _atten ("       attenuation   distance " , float ) = 15
        _attenPow ("       attenuation    exp " , range ( 0 , 1 ) ) = .5
        
        // [Header(_)]


    }
    SubShader
    {

            Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }

        Pass {

            Tags { "LightMode"="ForwardAdd" }
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB
            Cull Back
            Lighting On
            ZWrite Off
            LOD 100


            CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           #pragma multi_compile_fwdadd

           #include "UnityCG.cginc"
           #include "AutoLight.cginc"


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

            int _useLights;
            float _atten ;
            float _attenPow;

            sampler2D _CameraDepthTexture;




            struct appdata 
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
                float4 pCol : COLOR ;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: TEXCOORD1;
                float3 ambient: TEXCOORD2;
                float3 worldPos: TEXCOORD3;
                LIGHTING_COORDS(4, 5)

               
                float4 projPos : TEXCOORD6;
                float depth : TEXCOORD7;
                float4 pCol : COLOR ;


                float3 viewDir : TEXCOORD8;
             
            };
            
            float4 _MainTex_ST;
            float4 _LightColor0;




            v2f vert (appdata v) 
            {
                v2f o = (v2f)0;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                

                if(_softToggle==1){   
                    o.projPos = ComputeScreenPos(o.pos);
                    COMPUTE_EYEDEPTH(o.depth);
                }
                
                o.pCol = v.pCol ;


                return o;
            }







            float4 frag(v2f i) : COLOR 
            {
             

                float4 col = tex2D(_MainTex, i.uv);
                float br = length(col.xyz);
                col *= _multiplyCol;
                float br2 = length ( _multiplyCol.xyz ) ;
    
                  
                    
          
                float le,le2;
                if(_nearToggle){
                    le = length(_WorldSpaceCameraPos - i.worldPos ) ; // cam to obj ;
                    le2 = le;
                    le*=.1;
                }

                if(_nearToggle){
                    float near = _nearCam;
                    float far = _farCam;
                    le = (le-near)/(far - near) ;
                    le = clamp(le,0,1);
                    col.a *= le ;
                }

                    

            
                    if(_softToggle==1 && _soft > 0 ) {
                        float bgDepth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, i.projPos).r);
                        float alpha = saturate((bgDepth - i.depth) /_soft);

                        col.a*=alpha ;
                    }

                    if(_useOrigColToggle)col.rgb *= i.pCol.rgb ;
                    col.a *= i.pCol.a ;


                if(_distanceColorToggle){
                    float nearB = _nearDistance;
                    float farB = _farDistance;
                    le2 = ( le2 - nearB ) / ( farB - nearB ) ;
                    le2 = clamp ( le2 , 0 , 1 ) ;
                    le2 *= _distanceColorRate ;
                    col.rgb = lerp ( col.rgb , _distantCol.rgb*lerp ( 1 , col.rgb , _inheritCol)  , le2 );
                    col.a*= lerp ( 1, _distantCol.a , pow(le2 , .64) ) ;
                }
                
            
                if(_useLights){
                    float3 lightAtten = length(_WorldSpaceLightPos0.xyz - i.worldPos.xyz);
                    float lightDistance =     _atten;
                    lightAtten = max(0,lightDistance-lightAtten)/lightDistance;
                    lightAtten = pow(lightAtten , 1*(1/_attenPow));
                    col.rgb += _LightColor0 * lightAtten *lerp(br,1,br2);
                }


                return col;
            }
            ENDCG
        }
    }
}
Shader "PotaToon/Hidden/OITFullscreenRender"
{
	Properties
	{
	}
	SubShader
	{
        Tags { "RenderPipeline" = "UniversalPipeline" }
		Pass
		{
			ZTest Always
			ZWrite Off
			Cull Off
			// Blend One One
			Blend One OneMinusSrcAlpha

			HLSLPROGRAM
			#pragma vertex OITVert
			#pragma fragment OITFrag
			#pragma target 5.0
			// #pragma require randomwrite

			#pragma multi_compile_local_fragment _ _OIT_ADDITIVE

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
			#include "./LinkedListRendering.hlsl"

			SAMPLER(sampler_BlitTexture);

			Varyings OITVert(Attributes input)
			{
				Varyings output;

			#if SHADER_API_GLES
				float4 pos = input.positionOS;
				float2 uv  = input.uv;
			#else
				float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
				float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);
			#endif

				output.positionCS = pos;
				output.texcoord   = uv;
				return output;
			}

			//Pixel function returns a solid color for each point.
			float4 OITFrag(Varyings i, uint uSampleIndex : SV_SampleIndex) : SV_Target
			{
				// Retrieve current color from background texture
				float4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, i.texcoord);
				float4 finalColor = renderLinkedList(color, i.positionCS.xy, uSampleIndex);
				return finalColor;
			}
			ENDHLSL
		}
	}
}
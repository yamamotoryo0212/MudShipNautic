#ifndef POTA_TOON_CHAR_MASK_PASS_INCLUDED
#define POTA_TOON_CHAR_MASK_PASS_INCLUDED

#include "./PotaToonInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

struct Attributes
{
   float4 position     : POSITION;
};
struct Varyings
{
    float4 positionCS   : SV_POSITION;
};

Varyings vert(Attributes input)
{
    Varyings output = (Varyings)0;
    output.positionCS = TransformObjectToHClip(input.position.xyz);
    return output;
}

half2 frag(Varyings input) : SV_TARGET
{
    const float2 ssUV = GetNormalizedScreenSpaceUV(input.positionCS.xy);
    const float sceneDepth = SampleSceneDepth(ssUV);
    const float linearDepth = LinearEyeDepth(sceneDepth, _ZBufferParams);
    const float inputLinearDepth = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
    if (inputLinearDepth >= linearDepth + 0.01)
        clip(-1);
    return half2(1, _ToonType == FACE_TYPE ? 0 : 1);
}

#endif
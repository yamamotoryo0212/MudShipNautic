#ifndef POTA_TOON_COMMON_INCLUDED
#define POTA_TOON_COMMON_INCLUDED

#define COS_45 0.7071

half LinearStep(float m, float M, float x)
{
    return saturate((x - m) / (M - m));
}

float2 SelectUVVertex(uint channel, float2 uv0, float2 uv1, float2 uv2, float2 uv3)
{
    uint mask0 = channel & 1;
    uint mask1 = (channel >> 1) & 1;
    
    return lerp(lerp(uv0, uv1, mask0), lerp(uv2, uv3, mask0), mask1);
}

float2 SelectUV(uint channel, const float2 uvArray[4])
{
    return uvArray[channel & 3]; // Ensures channel is within [0, 3]
}

float SelectMask(float4 v, uint i)
{
    uint mask0 = i & 1;
    uint mask1 = (i >> 1) & 1;
    
    return lerp(lerp(v.x, v.y, mask0), lerp(v.z, v.w, mask0), mask1);
}

#endif
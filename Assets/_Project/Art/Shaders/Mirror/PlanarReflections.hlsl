#ifndef PLANAR_REFLECTIONS_INCLUDED
#define PLANAR_REFLECTIONS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

// Define textures and samplers conditionally based on probe ID
#if defined(_PRID_ONE)
    TEXTURE2D_X(_PlanarReflectionsTex1);
    SAMPLER(sampler_PlanarReflectionsTex1);
#elif defined(_PRID_TWO)
    TEXTURE2D_X(_PlanarReflectionsTex2);
    SAMPLER(sampler_PlanarReflectionsTex2);
#elif defined(_PRID_THREE)
    TEXTURE2D_X(_PlanarReflectionsTex3);
    SAMPLER(sampler_PlanarReflectionsTex3);
#elif defined(_PRID_FOUR)
    TEXTURE2D_X(_PlanarReflectionsTex4);
    SAMPLER(sampler_PlanarReflectionsTex4);
#endif

// Single function to sample reflections based on active keyword
float4 SamplePlanarReflections(float4 screenUV) { 
    float2 uv = screenUV.xy / screenUV.w;
    uv.x = 1 - uv.x; // Flip horizontally for mirror effect

    // Sample the correct texture based on probe ID
    #if defined(_PRID_ONE)
        return SAMPLE_TEXTURE2D_X(_PlanarReflectionsTex1, sampler_PlanarReflectionsTex1, uv);
    #elif defined(_PRID_TWO)
        return SAMPLE_TEXTURE2D_X(_PlanarReflectionsTex2, sampler_PlanarReflectionsTex2, uv);
    #elif defined(_PRID_THREE)
        return SAMPLE_TEXTURE2D_X(_PlanarReflectionsTex3, sampler_PlanarReflectionsTex3, uv);
    #elif defined(_PRID_FOUR)
        return SAMPLE_TEXTURE2D_X(_PlanarReflectionsTex4, sampler_PlanarReflectionsTex4, uv);
    #else
        return float4(0, 0, 0, 1); // Fallback if no probe ID is defined
    #endif
}

#endif // PLANAR_REFLECTIONS_INCLUDED
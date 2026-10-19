#ifndef JUST_SAIL_DIORAMA_FOG
#define JUST_SAIL_DIORAMA_FOG
float4 _HazeColor;
float _HazeDensity;
float3 DioramaFog(float3 color,float3 world)
{
    float3 ray=world-_WorldSpaceCameraPos;
    float dy=ray.y*.045;
    float integral=abs(dy)<.01?1:(1-exp(clamp(-dy,-12,12)))/dy;
    float layer=exp(-max(0,_WorldSpaceCameraPos.y)*.045)*integral;
    float density=max(0,length(ray)-35)*_HazeDensity*clamp(layer,.05,1);
    return lerp(color,_HazeColor.rgb,min(.58,1-exp(-density)));
}
#endif

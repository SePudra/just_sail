#ifndef JUST_SAIL_OCEAN_WAVES
#define JUST_SAIL_OCEAN_WAVES
float _SeaTime,_WaveScale,_HasSeabed;
float4 _OceanWaves[6],_OceanShapes[6],_DepthBounds;
TEXTURE2D(_Seabed);SAMPLER(sampler_Seabed);
float FloorHeight(float2 p)
{
    float2 uv=(p-_DepthBounds.xy)/max(_DepthBounds.zw,1);
    if(_HasSeabed<.5||any(uv<0)||any(uv>1))return -60;
    return SAMPLE_TEXTURE2D_LOD(_Seabed,sampler_Seabed,uv,0).r;
}
float3 Shore(float2 p)
{
    float s=saturate((-FloorHeight(p)-.6)/11);
    float f=.08+.92*s*s*(3-2*s),g=.92*6*s*(1-s)/11;
    float x=-(FloorHeight(p+float2(1,0))-FloorHeight(p-float2(1,0)))*.5*g;
    float z=-(FloorHeight(p+float2(0,1))-FloorHeight(p-float2(0,1)))*.5*g;
    return float3(f,x,z);
}
void OceanSurface(float2 p,float t,out float3 position,out float3 normal,out float jacobian,out float3 velocity)
{
    float3 shore=Shore(p);float f=shore.x*_WaveScale,gx=shore.y*_WaveScale,gz=shore.z*_WaveScale;
    position=float3(p.x,0,p.y);float3 tx=float3(1,0,0),tz=float3(0,0,1);velocity=0;
    [unroll]for(int i=0;i<6;i++)
    {
        float4 w=_OceanWaves[i],s=_OceanShapes[i];float sn,cs;
        sincos(w.w*dot(w.xy,p)-s.y*t+s.z,sn,cs);float h=s.x;
        position+=float3(h*w.x*f*cs,w.z*f*sn,h*w.y*f*cs);
        tx+=float3(h*w.x*(gx*cs-f*w.w*w.x*sn),w.z*(gx*sn+f*w.w*w.x*cs),h*w.y*(gx*cs-f*w.w*w.x*sn));
        tz+=float3(h*w.x*(gz*cs-f*w.w*w.y*sn),w.z*(gz*sn+f*w.w*w.y*cs),h*w.y*(gz*cs-f*w.w*w.y*sn));
        velocity+=float3(h*w.x*f*s.y*sn,-w.z*f*s.y*cs,h*w.y*f*s.y*sn);
    }
    normal=normalize(cross(tz,tx));jacobian=tx.x*tz.z-tz.x*tx.z;
}
#endif

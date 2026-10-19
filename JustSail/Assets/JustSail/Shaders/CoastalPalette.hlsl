#ifndef JUST_SAIL_COASTAL_PALETTE
#define JUST_SAIL_COASTAL_PALETTE
// Shared, linear-space pigments. Terrain and meadow roots must belong to
// the same colour fields; individual triangles must not choose random hues.
float CoastHash(float2 p)
{
    p=frac(p*float2(.1031,.1030));p+=dot(p,p.yx+33.33);
    return frac((p.x+p.y)*p.x);
}
float CoastPatch(float2 p)
{
    float2 cell=floor(p),f=frac(p);f=f*f*(3-2*f);
    return lerp(lerp(CoastHash(cell),CoastHash(cell+float2(1,0)),f.x),
                lerp(CoastHash(cell+float2(0,1)),CoastHash(cell+1),f.x),f.y);
}
float3 MeadowPigment(float2 world)
{
    float broad=CoastPatch(world*.035),small=CoastPatch(world*.11+13.8);
    float3 ochre=float3(.4564,.2542,.0865);
    float3 straw=float3(.5647,.3564,.1329);
    float3 copper=float3(.4179,.1714,.0802);
    float3 sage=float3(.2086,.2232,.1046);
    float3 ground=lerp(ochre,straw,smoothstep(.25,.8,broad));
    ground=lerp(ground,copper,smoothstep(.6,.85,small)*.4);
    return lerp(ground,sage,(1-smoothstep(.25,.46,broad))*.7);
}
#endif

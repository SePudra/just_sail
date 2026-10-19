#ifndef JUST_SAIL_ATMOSPHERE
#define JUST_SAIL_ATMOSPHERE
float4 _SkyTop,_SkyHorizon,_CloudLight,_CloudDark,_SunDirection,_MoonDirection,_WeatherData,_StormRegion,_WindData;
float _WorldClock,_NightBlend,_Lightning,_AuroraStrength;
float Hash31(float3 p){p=frac(p*.1031);p+=dot(p,p.yzx+33.33);return frac((p.x+p.y)*p.z);}
float Noise3(float3 p)
{
    float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);
    return lerp(lerp(lerp(Hash31(i),Hash31(i+float3(1,0,0)),f.x),lerp(Hash31(i+float3(0,1,0)),Hash31(i+float3(1,1,0)),f.x),f.y),lerp(lerp(Hash31(i+float3(0,0,1)),Hash31(i+float3(1,0,1)),f.x),lerp(Hash31(i+float3(0,1,1)),Hash31(i+1),f.x),f.y),f.z);
}
float CloudField(float3 p)
{
    p.xz-=_WindData.xy*_WorldClock*1.1;
    p*=.005;
    return Noise3(p)*.65+Noise3(p*2.03+4.7)*.25+Noise3(p*4.13+13.1)*.1;
}
float3 SkyBase(float3 d)
{
    float3 c=lerp(_SkyHorizon.rgb,_SkyTop.rgb,pow(saturate(d.y),.48));
    float sun=dot(d,_SunDirection.xyz),moon=dot(d,_MoonDirection.xyz);
    c+=float3(1,.65,.32)*(smoothstep(.99965,.9999,sun)*2+pow(saturate(sun),48)*.16)*(1-_NightBlend);
    c+=float3(.6,.72,1)*(smoothstep(.99975,.9999,moon)*1.3+pow(saturate(moon),70)*.035)*_NightBlend;
    float star=Hash31(floor(d*650));c+=pow(saturate((star-.9975)*400),3)*smoothstep(.8,1,_NightBlend)*smoothstep(.08,.3,d.y)*.6;
    float azimuth=atan2(d.x,d.z),time=_WorldClock*.035;
    float curtain=.19+sin(azimuth*2.6+time)*.045+sin(azimuth*5.1-time*.7)*.018;
    float ribbon=exp(-abs(d.y-curtain)*27)*smoothstep(.06,.15,d.y)*(1-smoothstep(.35,.65,d.y));
    float folds=.4+.6*pow(.5+.5*sin(azimuth*38+sin(azimuth*5+time)*4),2);
    float3 aurora=lerp(float3(.11,.62,.37),float3(.42,.2,.57),smoothstep(curtain,curtain+.17,d.y));
    c+=aurora*ribbon*folds*_AuroraStrength*.65;
    return c;
}
float StormAt(float2 p){return 1-smoothstep(_StormRegion.z*.2,_StormRegion.z,distance(p,_StormRegion.xy));}
float3 AtmosphereColor(float3 d,float3 origin,bool detailed)
{
    float3 color=SkyBase(d);
    if(d.y<.005)return color;
    float nearDistance=(170-origin.y)/max(d.y,.005);
    if(nearDistance>3500)return color;
    if(!detailed)
    {
        float3 p=origin+d*((220-origin.y)/max(d.y,.04));
        float cover=saturate((CloudField(p)-lerp(.76,.35,_WeatherData.x))*4);
        float3 reflectedCloud=lerp(_CloudDark.rgb,_CloudLight.rgb,.65);
        return lerp(color,reflectedCloud,cover*.55*saturate(1-nearDistance/3500));
    }
    float a=0;float3 cloud=0;
    int steps=20;
    float start=nearDistance;
    float finish=min(3500,(280-origin.y)/d.y);
    float stride=(finish-start)/steps;
    [loop]for(int j=0;j<steps;j++)
    {
        float3 p=origin+d*(start+stride*(j+.5));float fraction=saturate((p.y-170)/110);
        float coverage=saturate(_WeatherData.x+StormAt(p.xz)*_StormRegion.w*.3);
        float shape=smoothstep(0,.22,fraction)*(1-smoothstep(.65,1,fraction));
        float density=saturate((CloudField(p)-lerp(.76,.35,coverage))*5)*shape;
        float opacity=1-exp(-density*stride*.018);opacity*=1-a;
        float silver=pow(saturate(dot(d,_SunDirection.xyz)),12)*(1-_NightBlend);
        float3 lit=lerp(_CloudDark.rgb,_CloudLight.rgb,saturate(fraction*.85+.12))+silver*.18;
        cloud+=lit*opacity;a+=opacity;
    }
    color=lerp(color,cloud/max(a,.001),a*smoothstep(.005,.08,d.y)*saturate((3500-nearDistance)/1000));
    return color+_Lightning*.18;
}
#endif

Shader "JustSail/Sea"
{
    Properties { _DeepColor("Deep",Color)=(.02,.15,.24,1) _ShallowColor("Shallow",Color)=(.12,.55,.49,1) _FoamColor("Foam",Color)=(.8,.9,.85,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-20" "RenderType"="Transparent" }
        Pass
        {
            Name "Sea" Tags { "LightMode"="UniversalForward" }
            ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "OceanWaves.hlsl"
            #include "Atmosphere.hlsl"
            #include "DioramaFog.hlsl"
            float4 _SeaDeep,_SeaMiddle,_SeaShallow,_SeaFoam,_BoatPose;
            float _BoatSpeed;
            TEXTURE2D(_ShoreReflection);SAMPLER(sampler_ShoreReflection);
            float4x4 _ShoreReflectionVP;float _ShoreReflectionAvailable;
            float4 _OceanWake[48],_OceanWakeDirection[48];int _WakeCount;
            struct A {float4 positionOS:POSITION;};
            struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float3 surface:TEXCOORD2;float fog:TEXCOORD3;};
            V vert(A v)
            {
                V o;float3 original=TransformObjectToWorld(v.positionOS.xyz);float3 velocity;float jac;
                OceanSurface(original.xz,_SeaTime,o.world,o.normal,jac,velocity);
                o.surface=float3(o.world.y,jac,-FloorHeight(original.xz));
                o.positionCS=TransformWorldToHClip(o.world);o.fog=ComputeFogFactor(o.positionCS.z);return o;
            }
            half4 frag(V i):SV_Target
            {
                float2 p=i.world.xz;float distanceToCamera=distance(_WorldSpaceCameraPos,i.world);
                float detail=1-smoothstep(70,350,distanceToCamera);
                float2 uv1=p*.9+float2(_SeaTime*.35,_SeaTime*.25);
                float2 uv2=p*2.1-float2(_SeaTime*.55,-_SeaTime*.45);
                float2 uv3=p*4.2+float2(-_SeaTime*.7,_SeaTime*.6);
                float2 rip1=float2(cos(uv1.x*2.2+uv1.y*1.5),sin(uv1.y*2.1-uv1.x*1.4))*.024;
                float2 rip2=float2(sin(uv2.x*3.1-uv2.y*2.3),cos(uv2.y*2.8+uv2.x*1.9))*.014;
                float2 rip3=float2(cos(uv3.x*5.0+uv3.y*4.2),sin(uv3.y*4.8-uv3.x*3.6))*.008*detail;
                float2 ripple=(rip1+rip2+rip3)*.3;
                float3 n=normalize(i.normal+float3(ripple.x,0,ripple.y));
                float3 v=normalize(GetWorldSpaceViewDir(i.world));float facing=saturate(dot(n,v));
                Light light=GetMainLight(TransformWorldToShadowCoord(i.world));
                float ndl=saturate(dot(n,light.direction));
                float floor=FloorHeight(p);
                float depth=max(0,i.world.y-floor);
                // Filter colour depth only: buoyancy, geometry and shoreline contacts keep their exact samples.
                float colourFloor=(floor*4+FloorHeight(p+float2(3,0))+FloorHeight(p-float2(3,0))+FloorHeight(p+float2(0,3))+FloorHeight(p-float2(0,3)))*.125;
                float colourDepth=max(0,-colourFloor+i.world.y*.2);
                float shallow=exp(-colourDepth*.13);
                float deep=1-exp(-colourDepth*.055);
                float3 col=lerp(_SeaMiddle.rgb,_SeaShallow.rgb,shallow);
                col=lerp(col,_SeaDeep.rgb,deep);
                col*=.82+.18*ndl;
                float2 screen=GetNormalizedScreenSpaceUV(i.positionCS);
                float rawDepth=SampleSceneDepth(screen);
                float eye=LinearEyeDepth(rawDepth,_ZBufferParams);
                float waterEye=-TransformWorldToView(i.world).z;
                float thickness=max(0,eye-waterEye)*max(.08,abs(v.y));
                float transmission=exp(-depth*.28)*.22;
                float2 refractUV=saturate(screen+n.xz*.007*min(1,thickness));
                col=lerp(col,SampleSceneColor(refractUV)*lerp(_SeaShallow.rgb,1,.35),transmission);
                float3 reflectionNormal=normalize(lerp(float3(0,1,0),n,.3));
                float3 reflected=reflect(-v,reflectionNormal);
                reflected.y=.5*(reflected.y+sqrt(reflected.y*reflected.y+.0064))+.025;
                float3 reflection=AtmosphereColor(normalize(reflected),i.world,false);
                float4 projected=mul(_ShoreReflectionVP,float4(i.world.x,0,i.world.z,1));
                float2 mirrorUV=projected.xy/max(.001,projected.w)*.5+.5;
                #if UNITY_UV_STARTS_AT_TOP
                mirrorUV.y=1-mirrorUV.y;
                #endif
                mirrorUV+=n.xz*.006+ripple*.06;
                float border=min(min(mirrorUV.x,1-mirrorUV.x),min(mirrorUV.y,1-mirrorUV.y));
                float valid=smoothstep(0,.035,border)*step(0,projected.w);
                float3 shoreReflection=SAMPLE_TEXTURE2D_LOD(_ShoreReflection,sampler_ShoreReflection,saturate(mirrorUV),1.8).rgb;
                shoreReflection*=lerp(float3(.32,.8,1),float3(1,1,1),_NightBlend);
                reflection=lerp(reflection,shoreReflection,valid*_ShoreReflectionAvailable);
                float fresnel=.1+.34*pow(1-facing,3.5);
                col=lerp(col,reflection,fresnel);
                float back=pow(saturate(dot(v,-light.direction)),4);
                float crest=saturate(i.surface.x*.6+.3)*saturate(1-n.y)*2.8;
                col+=_SeaShallow.rgb*back*crest*light.color*.22*light.shadowAttenuation;
                float hdot=saturate(dot(n,normalize(light.direction+v)));
                float specTight=pow(hdot,160)*.55;
                float specBroad=pow(hdot,26)*.055;
                float spec=(specTight+specBroad)*light.shadowAttenuation;
                col+=light.color*spec;
                float foamNoise=Noise3(float3(p*.18,_SeaTime*.13));
                float crestFoam=saturate((.78-i.surface.y)*4.2)*smoothstep(.43,.7,foamNoise)*(1-smoothstep(60,380,distanceToCamera));
                float contactFoam=pow(saturate(1.0-thickness/.55),2.2)*smoothstep(.2,.6,foamNoise+.35);
                float shoreBand=1-smoothstep(.4,2.5,depth);
                float shore=pow(saturate(sin(depth*4-_SeaTime*1.25+foamNoise*2)),4)*shoreBand;
                float wake=0;
                [loop]for(int w=0;w<_WakeCount;w++)
                {
                    float age=_OceanWake[w].z;float speed=_OceanWakeDirection[w].z;float2 delta=p-_OceanWake[w].xy;float radius=1+age*(.45+speed*.3);
                    if(age<7.5&&dot(delta,delta)<radius*radius)
                    {
                        float2 dir=_OceanWakeDirection[w].xy;float along=dot(delta,dir),side=abs(dot(delta,float2(-dir.y,dir.x)));
                        float spread=.32+age*speed*.3;
                        float wing=exp(-pow((side-spread)/(.16+age*.045),2)-pow(along/(.5+speed*.12+age*.14),2));
                        float center=exp(-dot(delta,delta)/(.35+age*.4));
                        wake=max(wake,(wing*.4+center*.15)*pow(saturate(1-age/7.5),1.8)*_OceanWake[w].w);
                    }
                }
                float2 boatDelta=p-_BoatPose.xy,dir=_BoatPose.zw;
                float bowAlong=dot(boatDelta,dir),bowSide=abs(dot(boatDelta,float2(-dir.y,dir.x)));
                float bow=exp(-pow((bowAlong-2.05)/.48,2)-pow((bowSide-.65)/.25,2))*saturate(_BoatSpeed/4)*.55;
                float foam=saturate(crestFoam*.58+max(shore*.8,contactFoam*.85)+wake+bow);
                col=lerp(col,_SeaFoam.rgb,foam);
                float trough=saturate(dot(n,light.direction)*.5+.5);
                col*=lerp(.95,1,trough);
                float3 fogged=MixFog(col,i.fog);
                return half4(lerp(fogged,DioramaFog(fogged,i.world),.3),1);
            }
            ENDHLSL
        }
    }
}

Shader "JustSail/Landscape"
{
    Properties { _BaseMap("Texture",2D)="white"{} _BaseColor("Base",Color)=(1,1,1,1) _Cutoff("Cutoff",Float)=.5 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "DioramaFog.hlsl"
            float _WorldClock,_Wetness;
            struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;float4 color:COLOR;};
            struct V{float4 positionCS:SV_POSITION;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;float4 color:COLOR;float fog:TEXCOORD2;};
            V vert(A a){V o;o.world=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(a.normalOS);o.color=a.color;o.fog=ComputeFogFactor(o.positionCS.z);return o;}
            half4 frag(V i):SV_Target{
                float3 n=normalize(i.normal);
                Light l=GetMainLight(TransformWorldToShadowCoord(i.world));
                float ndl=saturate(dot(n,l.direction));
                float3 ambient=SampleSH(n)*.85;
                float3 direct=l.color*ndl*l.shadowAttenuation;
                float3 col=i.color.rgb*(1-_Wetness*.12)*(ambient+direct);
                float depth=max(0,-i.world.y);
                float a=sin(i.world.x*2.1+i.world.z*.9+_WorldClock*.8),b=sin(i.world.z*2.4-i.world.x*.7-_WorldClock*.63);
                float caustic=pow(saturate(1-abs(a+b)*1.7),7)*exp(-depth*.55)*step(i.world.y,0);
                col+=l.color*caustic*.45*l.shadowAttenuation;
                return half4(DioramaFog(MixFog(col,i.fog),i.world),1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}

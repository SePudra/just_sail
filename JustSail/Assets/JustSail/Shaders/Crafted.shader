Shader "JustSail/Crafted"
{
    Properties
    {
        _BaseMap("Texture",2D)="white"{} _BaseColor("Color",Color)=(1,1,1,1)
        _Smoothness("Smoothness",Range(0,1))=.05 _EmissionColor("Emission",Color)=(0,0,0,0)
        _Cutoff("Cutoff",Float)=.5 _Cull("Cull",Float)=0 _Foliage("Leaf diffusion",Range(0,1))=0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            Tags {"LightMode"="UniversalForward"} Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "DioramaFog.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor,_EmissionColor;float _Smoothness,_Foliage;
            CBUFFER_END
            struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;};
            struct V{float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float height:TEXCOORD2;float fog:TEXCOORD3;};
            V vert(A a){V o;o.world=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(a.normalOS);o.height=o.world.y-TransformObjectToWorld(float3(0,0,0)).y;o.fog=ComputeFogFactor(o.positionCS.z);return o;}
            half4 frag(V i,bool front:SV_IsFrontFace):SV_Target
            {
                float3 n=normalize(i.normal)*(front?1:-1);Light key=GetMainLight(TransformWorldToShadowCoord(i.world));
                float light=lerp(saturate(dot(n,key.direction)),saturate(dot(n,key.direction)*.5+.5),_Foliage);float3 ambient=SampleSH(n)*.85;
                float gradient=lerp(.88,1.08,saturate(i.height*.16+.48));
                float3 direct=key.color*light*lerp(key.shadowAttenuation,1,_Foliage*.22);
                float3 color=_BaseColor.rgb*gradient*(ambient+direct);
                float3 eye=normalize(GetWorldSpaceViewDir(i.world));float shine=pow(saturate(dot(n,normalize(key.direction+eye))),32);
                color+=key.color*shine*_Smoothness*key.shadowAttenuation*.4+_EmissionColor.rgb;
                return half4(DioramaFog(MixFog(color,i.fog),i.world),1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}

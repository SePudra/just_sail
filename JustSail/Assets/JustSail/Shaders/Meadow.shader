Shader "JustSail/Meadow"
{
    SubShader
    {
        Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
        Pass
        {
            Tags{"LightMode"="UniversalForward"} Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "DioramaFog.hlsl"
            float _WorldClock;
            struct A{float4 p:POSITION;float4 color:COLOR;};
            struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;float4 color:COLOR;float fog:TEXCOORD1;};
            V vert(A a){V o;o.world=TransformObjectToWorld(a.p.xyz);o.world.x+=sin(o.world.x*.6+o.world.z*.4+_WorldClock*1.5)*.055;o.p=TransformWorldToHClip(o.world);o.color=a.color;o.fog=ComputeFogFactor(o.p.z);return o;}
            half4 frag(V i):SV_Target
            {
                Light l=GetMainLight(TransformWorldToShadowCoord(i.world));
                float3 c=i.color.rgb*(SampleSH(float3(0,1,0))*.65+l.color*(.35+.65*saturate(l.direction.y))*lerp(.46,1,l.shadowAttenuation));
                return half4(DioramaFog(MixFog(c,i.fog),i.world),1);
            }
            ENDHLSL
        }
    }
}

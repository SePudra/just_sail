Shader "JustSail/Meadow"
{
    SubShader
    {
        Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
        Pass
        {
            Tags{"LightMode"="UniversalForward"} Cull Off
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "DioramaFog.hlsl"
            #include "CoastalPalette.hlsl"
            float _WorldClock;float4 _WindData;
            struct A{float4 p:POSITION;float4 color:COLOR;};
            struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;float4 color:COLOR;float fog:TEXCOORD1;};
            V vert(A a)
            {
                V o;o.world=TransformObjectToWorld(a.p.xyz);
                // Grass() stores root alpha .84 and tip alpha 1.15 as float
                // colours. This anchor survives batching and index reordering.
                float tip=saturate((a.color.a-.84)/.31);
                float gust=sin(dot(o.world.xz,float2(.12,.08))-_WorldClock*1.4);
                float flutter=sin(o.world.x*.7+o.world.z*.4-_WorldClock*2.7)*.18;
                o.world.xz+=_WindData.xy*(gust+flutter)*lerp(.12,.32,_WindData.z)*tip;
                o.p=TransformWorldToHClip(o.world);o.color=float4(MeadowPigment(o.world.xz)*lerp(.88,1.12,tip),1);
                o.fog=ComputeFogFactor(o.p.z);return o;
            }
            half4 frag(V i):SV_Target
            {
                Light l=GetMainLight(TransformWorldToShadowCoord(i.world));
                // Broad clearings break up the former uniform orange carpet.
                clip(CoastPatch(i.world.xz*.07)-.27);
                float3 c=i.color.rgb*(SampleSH(float3(0,1,0))*.85+l.color*(.15+.85*saturate(l.direction.y))*lerp(.18,1,l.shadowAttenuation));
                return half4(DioramaFog(c,i.world),1);
            }
            ENDHLSL
        }
    }
}

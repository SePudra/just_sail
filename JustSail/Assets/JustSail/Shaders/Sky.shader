Shader "JustSail/Sky"
{
    Properties { _Top("Top",Color)=(.29,.53,.61,1) _Horizon("Horizon",Color)=(.75,.80,.70,1) _SunColor("Sun",Color)=(1,.87,.61,1) }
    SubShader { Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" } Cull Off ZWrite Off
        Pass { HLSLPROGRAM
        #pragma target 4.5
        #pragma vertex vert
        #pragma fragment frag
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Atmosphere.hlsl"
        struct V {float4 p:SV_POSITION;float3 d:TEXCOORD0;};
        V vert(float4 p:POSITION){V o;o.p=TransformObjectToHClip(p.xyz);o.d=p.xyz;return o;}
        half4 frag(V i):SV_Target{return half4(AtmosphereColor(normalize(i.d),_WorldSpaceCameraPos,true),1);}
        ENDHLSL }
    }
}

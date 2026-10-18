Shader "ROG ZOMBIE/Ground Mist"
{
    Properties { _Tint("Tint", Color) = (.64,.72,.8,1) _Density("Density", Float) = .14 _NoiseScale("Noise scale", Float) = 3 _Drift("Drift", Vector) = (0,0,0,0) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                float _Density, _NoiseScale;
                float4 _Drift;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 world : TEXCOORD1; };
            Varyings Vert(Attributes v)
            {
                Varyings o; o.world=TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(o.world); o.uv=v.uv; return o;
            }
            float Hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float Noise(float2 p)
            {
                float2 cell=floor(p), f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(Hash(cell),Hash(cell+float2(1,0)),f.x),lerp(Hash(cell+float2(0,1)),Hash(cell+1),f.x),f.y);
            }
            half4 Frag(Varyings i) : SV_Target
            {
                float radius=length((i.uv-.5)*2);
                float edge=1-smoothstep(.25,1,radius);
                float2 p=(i.world.xz-_Drift.xy)/max(_NoiseScale,.05);
                float cloud=Noise(p)*.65+Noise(p*2.07+17)*.35;
                float alpha=edge*smoothstep(.15,.8,cloud)*saturate(_Density)*_Tint.a;
                return half4(_Tint.rgb,alpha);
            }
            ENDHLSL
        }
    }
}

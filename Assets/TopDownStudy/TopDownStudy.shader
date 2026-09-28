Shader "ROG/TopDownStudy"
{
    Properties { _BaseColor("Color", Color) = (1,1,1,1) _Facade("Facade", Float) = 0 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            ZWrite On Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct V { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; float3 normal:TEXCOORD1; };
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float _Facade;
            CBUFFER_END
            V Vert(A a) { V o; o.world=TransformObjectToWorld(a.positionOS.xyz); o.positionCS=TransformWorldToHClip(o.world); o.normal=TransformObjectToWorldNormal(a.normalOS); return o; }
            half4 Frag(V i):SV_Target
            {
                float3 n=normalize(i.normal);
                float shade=.62+.38*saturate(dot(n,normalize(float3(-.5,.7,-1))));
                float3 pixel=floor(i.world*8);
                float noise=frac(sin(dot(pixel,float3(12.9898,78.233,37.719)))*43758.5453);
                float3 color=_BaseColor.rgb*(.92+noise*.12)*shade;
                if (_Facade>.5 && abs(n.z)<.5)
                {
                    float u=abs(n.x)>.5 ? i.world.y : i.world.x;
                    float2 cell=frac(float2(u/1.8,-i.world.z/2.5));
                    float window=step(.22,cell.x)*step(cell.x,.72)*step(.22,cell.y)*step(cell.y,.7);
                    float lit=step(.7,frac(sin(floor(u/1.8)*23+floor(-i.world.z/2.5)*71)*135.13));
                    color=lerp(color,lerp(float3(.07,.14,.18),float3(.6,.5,.25),lit)*shade,window);
                    color*=lerp(.65,1,step(.055,cell.y));
                }
                return half4(color,1);
            }
            ENDHLSL
        }
    }
}

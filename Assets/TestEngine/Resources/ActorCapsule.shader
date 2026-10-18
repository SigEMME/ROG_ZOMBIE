Shader "ROG/ActorCapsule"
{
    Properties { _BaseColor("Color", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            ZWrite On Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            UNITY_INSTANCING_BUFFER_START(Actor)
                UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
            UNITY_INSTANCING_BUFFER_END(Actor)
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct V { float4 positionCS:SV_POSITION; float3 normal:TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            V Vert(A input)
            {
                V output; UNITY_SETUP_INSTANCE_ID(input); UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS=TransformObjectToHClip(input.positionOS.xyz);
                output.normal=TransformObjectToWorldNormal(input.normalOS);
                return output;
            }
            half4 Frag(V input):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 color=UNITY_ACCESS_INSTANCED_PROP(Actor,_BaseColor);
                clip(color.a-.001);
                // Fixed soft shading keeps identification colors readable in both 2D and 3D scenes.
                half shade=.55+.45*saturate(dot(normalize(input.normal),normalize(float3(-.5,.7,-1))));
                return half4(color.rgb*shade,1);
            }
            ENDHLSL
        }
    }
}

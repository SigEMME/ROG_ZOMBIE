Shader "ROG/TopDownStudy"
{
    Properties { _SurfaceTex("Surface", 2D) = "white" {} _Textured("Textured", Float) = 0 _TileMeters("Tile metres", Float) = 4 _CurbAxis("Curb axis", Float) = 0  _BaseColor("Color", Color) = (1,1,1,1) _Facade("Facade", Float) = 0 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            ZWrite On Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct V { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; float3 normal:TEXCOORD1; float2 local:TEXCOORD2; };
            TEXTURE2D(_SurfaceTex); SAMPLER(sampler_SurfaceTex);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float _Facade, _Textured, _TileMeters, _CurbAxis;
            CBUFFER_END
            V Vert(A a) { V o; o.local=a.positionOS.xy; o.world=TransformObjectToWorld(a.positionOS.xyz); o.positionCS=TransformWorldToHClip(o.world); o.normal=TransformObjectToWorldNormal(a.normalOS); return o; }
            half4 Frag(V i):SV_Target
            {
                if (_Textured > .5)
                {
                    float2 uv = i.world.xy / max(_TileMeters, .01);
                    if (_Textured > 1.5)
                    {
                        // Repeat an 8-column, 18-row module every _TileMeters: the source has no
                        // matching outer edges, so its full image must not be tiled.
                        float2 tile = frac(uv);
                        float row = floor(tile.y * 18);
                        float2 brick = float2(tile.x * 8 + frac(row * .5), tile.y * 18);
                        float2 cell = fmod(floor(brick), float2(8, 18));
                        float2 within = frac(brick);
                        float2 edge = min(within, 1 - within) * float2(_TileMeters / 8, _TileMeters / 18);
                        float mortar = 1 - step(.006, min(edge.x, edge.y));
                        // Sample only an interior stone patch, away from source joints.
                        float2 patch = float2(lerp(.14,.22,within.x),lerp(.970,.985,within.y));
                        float variation = .94 + .12 * frac(sin(dot(cell,float2(12.9898,78.233))) * 43758.5453);
                        float3 stone = SAMPLE_TEXTURE2D(_SurfaceTex,sampler_SurfaceTex,patch).rgb * variation;
                        return half4(lerp(stone,float3(.23,.23,.21),mortar),1);
                    }
                    if (_CurbAxis > .5)
                    {
                        float along = (_CurbAxis < 1.5 ? i.world.x : i.world.y) / _TileMeters;
                        float across = (_CurbAxis < 1.5 ? i.local.y : i.local.x) + .5;
                        // Original strip occupies the middle ~9% of the image height.
                        // Map its full width instead of stretching one scanline.
                        uv = float2(along, lerp(.453,.535,saturate(across)));
                    }
                    return half4(SAMPLE_TEXTURE2D(_SurfaceTex, sampler_SurfaceTex, uv).rgb, 1);
                }
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

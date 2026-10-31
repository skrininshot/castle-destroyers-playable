Shader "Game/Castle"
{
    // Рисунок замка, вырезанный по маске (ТЗ §3.6, D-042). Каналы маски: R материал, G кайма, B трещины.
    // TEXCOORD0 — локальные координаты вершины: в UV маски их переводит _MaskRect, в UV рисунка _ArtRect (xy — сдвиг, zw — масштаб).
    Properties
    {
        _MainTex ("Art", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "black" {}
        _MaskRect ("Mask Rect", Vector) = (0, 0, 1, 1)
        _ArtRect ("Art Rect", Vector) = (0, 0, 1, 1)
        _CrackColor ("Crack", Color) = (0, 0, 0, 1)
        _RimDarken ("Rim Darken", Range(0, 1)) = 0.7
        _EdgeSoftness ("Edge Softness", Range(0.01, 0.5)) = 0.15
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "Unlit"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MaskTex_ST;
                float4 _MaskRect;
                float4 _ArtRect;
                half4 _CrackColor;
                half _RimDarken;
                half _EdgeSoftness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 local : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 art : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.local * _MaskRect.zw + _MaskRect.xy;
                output.art = input.local * _ArtRect.zw + _ArtRect.xy;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv);
                half4 art = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.art);
                half coverage = smoothstep(0.5 - _EdgeSoftness, 0.5 + _EdgeSoftness, mask.r) * art.a;
                clip(coverage - 0.001);
                half3 color = art.rgb * lerp(1.0, _RimDarken, smoothstep(0.42, 0.58, mask.g));
                color = lerp(color, _CrackColor.rgb, smoothstep(0.42, 0.58, mask.b));
                return half4(color, coverage);
            }
            ENDHLSL
        }
    }
}

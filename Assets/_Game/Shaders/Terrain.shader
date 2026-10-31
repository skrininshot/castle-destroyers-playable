Shader "Game/Terrain"
{
    // Земля по маске (ТЗ §3.6, §3.8). Каналы тайла: R материал, G кайма кратера, B кромка ступенями, A закопанные декоры ступенями.
    // Ступени запекает Tools/rasterize_svg.py. Все тона плоские: узкий порог сглаживает только границу тона, без градиента.
    Properties
    {
        _MaskTex ("Mask", 2D) = "black" {}
        _GroundColor ("Ground", Color) = (1, 1, 1, 1)
        _BandColor ("Band", Color) = (1, 1, 1, 1)
        _RimColor ("Rim", Color) = (1, 1, 1, 1)
        _RimLightColor ("Rim Light", Color) = (1, 1, 1, 1)
        _BlobColor ("Blob", Color) = (1, 1, 1, 1)
        _BuriedColor ("Buried", Color) = (1, 1, 1, 1)
        _CraterColor ("Crater", Color) = (1, 1, 1, 1)
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

            #define TONE_EDGE 0.035

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MaskTex_ST;
                half4 _GroundColor;
                half4 _BandColor;
                half4 _RimColor;
                half4 _RimLightColor;
                half4 _BlobColor;
                half4 _BuriedColor;
                half4 _CraterColor;
                half _EdgeSoftness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half Tone(half value, half threshold)
            {
                return smoothstep(threshold - TONE_EDGE, threshold + TONE_EDGE, value);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv);
                half coverage = smoothstep(0.5 - _EdgeSoftness, 0.5 + _EdgeSoftness, mask.r);
                clip(coverage - 0.001);
                half3 color = _GroundColor.rgb;
                color = lerp(color, _BlobColor.rgb, Tone(mask.a, 0.25));
                color = lerp(color, _BuriedColor.rgb, Tone(mask.a, 0.75));
                color = lerp(color, _BandColor.rgb, Tone(mask.b, 0.167));
                color = lerp(color, _RimColor.rgb, Tone(mask.b, 0.5));
                color = lerp(color, _RimLightColor.rgb, Tone(mask.b, 0.833));
                color = lerp(color, _CraterColor.rgb, step(0.5, mask.g));
                return half4(color, coverage);
            }
            ENDHLSL
        }
    }
}

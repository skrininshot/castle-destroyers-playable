Shader "Game/Terrain"
{
    // Земля по маске (ТЗ §3.6, §3.8). Каналы тайла: R материал, G кайма кратера, B цветная кромка, A закопанные декоры.
    // Все тона плоские: границы через step, сглаживается только внешний край материала.
    Properties
    {
        _MaskTex ("Mask", 2D) = "black" {}
        _GroundColor ("Ground", Color) = (1, 1, 1, 1)
        _RimColor ("Rim", Color) = (1, 1, 1, 1)
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

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MaskTex_ST;
                half4 _GroundColor;
                half4 _RimColor;
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

            half4 Frag(Varyings input) : SV_Target
            {
                half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv);
                half coverage = smoothstep(0.5 - _EdgeSoftness, 0.5 + _EdgeSoftness, mask.r);
                clip(coverage - 0.001);
                // Кромка и декоры обрезаны материалом, поэтому порог берём от R: иначе у края появится полоса земли.
                half threshold = mask.r * 0.5;
                half3 color = _GroundColor.rgb;
                color = lerp(color, _BuriedColor.rgb, step(threshold, mask.a) * step(0.001, mask.a));
                color = lerp(color, _RimColor.rgb, step(threshold, mask.b) * step(0.001, mask.b));
                color = lerp(color, _CraterColor.rgb, step(0.5, mask.g));
                return half4(color, coverage);
            }
            ENDHLSL
        }
    }
}

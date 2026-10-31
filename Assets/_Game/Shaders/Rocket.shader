Shader "Game/Rocket"
{
    // Ракета с плоскими тонами (ТЗ §3.4). Каналы текстуры: R силуэт, G детали в цвет стороны, B теневая полоса.
    // Цвет стороны приходит из вершин, остальное из палитры: цветов в текстуре нет.
    Properties
    {
        _MainTex ("Rocket", 2D) = "black" {}
        _BodyColor ("Body", Color) = (1, 1, 1, 1)
        _ShadeColor ("Shade", Color) = (1, 1, 1, 1)
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

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _BodyColor;
                half4 _ShadeColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 shape = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half coverage = smoothstep(0.35, 0.65, shape.r);
                clip(coverage - 0.001);
                half threshold = shape.r * 0.5;
                half3 color = _BodyColor.rgb;
                color = lerp(color, _ShadeColor.rgb, step(threshold, shape.b) * step(0.001, shape.b));
                color = lerp(color, input.color.rgb, step(threshold, shape.g) * step(0.001, shape.g));
                return half4(color, coverage);
            }
            ENDHLSL
        }
    }
}

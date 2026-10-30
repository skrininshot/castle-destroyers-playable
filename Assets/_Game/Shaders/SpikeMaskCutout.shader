Shader "Game/Spike/MaskCutout"
{
    Properties
    {
        _MainTex ("Material", 2D) = "white" {}
        _MaskTex ("Mask (R solid, G rim)", 2D) = "white" {}
        _WorldTiling ("World Tiling", Float) = 0.5
        _RimStrength ("Rim Strength", Range(0, 1)) = 0.85
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "AlphaTest" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "Unlit"
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
                float _WorldTiling;
                float _RimStrength;
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
                float2 worldUv : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = input.uv;
                output.worldUv = positionWS.xy * _WorldTiling;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv);
                clip(mask.r - 0.5);
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.worldUv);
                color.rgb *= 1.0 - mask.g * _RimStrength;
                color.a = 1.0;
                return color;
            }
            ENDHLSL
        }
    }
}

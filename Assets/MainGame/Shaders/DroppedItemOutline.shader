Shader "Perkimon/DroppedItemOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineSize ("Outline Size", Range(0.5, 5)) = 1

        [HideInInspector] _Color ("Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags
            {
                "LightMode" = "Universal2D"
            }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineSize;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 pixel =
                    _MainTex_TexelSize.xy * _OutlineSize;

                float centerAlpha =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv
                    ).a;

                // Don't draw over the actual item.
                if (centerAlpha > 0.01)
                    return half4(0, 0, 0, 0);

                float outlineAlpha = 0;

                // Right
                outlineAlpha = max(
                    outlineAlpha,
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv + float2(pixel.x, 0)
                    ).a
                );

                // Left
                outlineAlpha = max(
                    outlineAlpha,
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv + float2(-pixel.x, 0)
                    ).a
                );

                // Up
                outlineAlpha = max(
                    outlineAlpha,
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv + float2(0, pixel.y)
                    ).a
                );

                // Down
                outlineAlpha = max(
                    outlineAlpha,
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv + float2(0, -pixel.y)
                    ).a
                );

                // Top-right
                outlineAlpha = max(
                    outlineAlpha,
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv + float2(pixel.x, pixel.y)
                    ).a
                );

                // Top-left
                outlineAlpha = max(
                    outlineAlpha,
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv + float2(-pixel.x, pixel.y)
                    ).a
                );

                // Bottom-right
                outlineAlpha = max(
                    outlineAlpha,
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv + float2(pixel.x, -pixel.y)
                    ).a
                );

                // Bottom-left
                outlineAlpha = max(
                    outlineAlpha,
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv + float2(-pixel.x, -pixel.y)
                    ).a
                );

                return half4(
                    _OutlineColor.rgb,
                    outlineAlpha * _OutlineColor.a
                );
            }

            ENDHLSL
        }
    }
}
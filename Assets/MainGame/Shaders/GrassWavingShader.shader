Shader "Custom/2DGrassSwayLit"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}  // Grass texture
        _Color("Tint Color", Color) = (1,1,1,1)
        _SwayStrength("Sway Strength", Range(0, 0.2)) = 0.05
        _SwaySpeed("Sway Speed", Range(0, 5)) = 1.0
        _WindFrequency("Wind Frequency", Range(1, 10)) = 3.0
    }

        SubShader
        {
            Tags
            {
                "RenderType" = "Transparent"
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderPipeline" = "UniversalPipeline"
            }

            Pass
            {
                Name "GrassSway"
                Tags{"LightMode" = "Universal2D"}

                Blend SrcAlpha OneMinusSrcAlpha
                Cull Off
                ZWrite Off

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    float3 worldPos : TEXCOORD1;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _Color;
                float _SwayStrength;
                float _SwaySpeed;
                float _WindFrequency;
                float _TimeY;

                v2f vert(appdata v)
                {
                    v2f o;

                    // Apply a sine wave motion for the sway effect
                    float sway = sin(_Time.y * _SwaySpeed + v.vertex.y * _WindFrequency) * _SwayStrength;

                    // Move the vertex left and right (stronger effect at the top)
                    v.vertex.x += sway * (1.0 - v.uv.y);

                    o.vertex = TransformObjectToHClip(v.vertex.xyz);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;  // Store world position for lighting
                    return o;
                }

                half4 frag(v2f i) : SV_Target
                {
                    // Sample the grass texture
                    half4 texColor = tex2D(_MainTex, i.uv);

                    // Get main 2D light color
                   // half3 lightColor = _MainLightColor.rgb;

                    // Apply lighting by multiplying texture color with light
                    half4 finalColor = texColor * _Color; //* half4(lightColor, 1.0);

                    return finalColor;
                }
                ENDHLSL
            }
        }
}
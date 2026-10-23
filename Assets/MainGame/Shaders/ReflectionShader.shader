Shader "Custom/2DWaterReflectionShader"
{
    Properties
    {
        _MainTex("Base Texture", 2D) = "white" {}
        _ReflectionColor("Reflection Color", Color) = (0.6, 0.8, 1, 1)
        _ReflectionIntensity("Reflection Intensity", Range(0, 1)) = 0.5
        _DistortionStrength("Distortion Strength", Range(0, 0.1)) = 0.02
        _WaveSpeed("Wave Speed", Range(0, 5)) = 1.0
        _Transparency("Water Transparency", Range(0, 1)) = 0.8
    }

        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
            LOD 100

            Pass
            {
                Blend SrcAlpha OneMinusSrcAlpha
                Cull Off
                ZWrite Off

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _ReflectionColor;
                float _ReflectionIntensity;
                float _DistortionStrength;
                float _WaveSpeed;
                float _Transparency;
                float _TimeY;

                // Ripple effect using sine waves
                float rippleEffect(float2 uv)
                {
                    return sin(uv.y * 50 + _Time.y * _WaveSpeed) * _DistortionStrength;
                }

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Apply ripple distortion
                    float distortion = rippleEffect(i.uv);
                    float2 distortedUV = i.uv + float2(0, distortion);

                    // Sample the original texture
                    fixed4 baseColor = tex2D(_MainTex, i.uv);

                    // Reflection (flipped vertically)
                    float2 reflectionUV = float2(i.uv.x, 1.0 - i.uv.y);
                    reflectionUV += float2(0, distortion); // Apply distortion to reflection
                    fixed4 reflection = tex2D(_MainTex, reflectionUV) * _ReflectionColor;

                    // Blend the reflection with the base color
                    fixed4 finalColor = lerp(baseColor, reflection, _ReflectionIntensity);

                    // Apply transparency to simulate water
                    finalColor.a *= _Transparency;

                    return finalColor;
                }
                ENDCG
            }
        }
}

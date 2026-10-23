Shader "Custom/2DWetSurfaceShader"
{
    Properties
    {
        _MainTex("Base Texture", 2D) = "white" {}
        _WetMap("Wet Mask (Optional)", 2D) = "white" {}
        _ReflectionColor("Reflection Color", Color) = (0.6, 0.8, 1, 1)
        _Glossiness("Glossiness", Range(0, 1)) = 0.8
        _RippleSpeed("Ripple Speed", Range(0, 5)) = 1.0
        _DistortionStrength("Distortion Strength", Range(0, 0.1)) = 0.03
        _ReflectionIntensity("Reflection Intensity", Range(0, 1)) = 0.5
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
                sampler2D _WetMap;
                float4 _MainTex_ST;
                float4 _ReflectionColor;
                float _Glossiness;
                float _RippleSpeed;
                float _DistortionStrength;
                float _ReflectionIntensity;
                float _TimeY;

                // Ripple Effect (Simple Sin Waves)
                float rippleEffect(float2 uv)
                {
                    return sin(uv.y * 50 + _Time.y * _RippleSpeed) * _DistortionStrength;
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
                    // Base texture
                    fixed4 baseColor = tex2D(_MainTex, i.uv);

                // Apply ripple distortion
                float distortion = rippleEffect(i.uv);
                float2 distortedUV = i.uv + float2(0, distortion);

                // Reflection (mirrored vertically)
                float2 reflectionUV = float2(i.uv.x, 1.0 - i.uv.y);
                fixed4 reflection = tex2D(_MainTex, reflectionUV) * _ReflectionColor;

                // Wetness mask (optional)
                float wetMask = tex2D(_WetMap, i.uv).r;

                // Blend base color with reflection
                fixed4 finalColor = lerp(baseColor, reflection, _ReflectionIntensity * wetMask);

                // Add glossiness effect
                finalColor.rgb = lerp(finalColor.rgb, _ReflectionColor.rgb, _Glossiness * wetMask);

                // Preserve alpha for transparency
                finalColor.a = baseColor.a;

                return finalColor;
            }
            ENDCG
        }
        }
}
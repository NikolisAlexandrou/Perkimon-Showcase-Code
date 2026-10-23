Shader "Custom/2DFlameShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _FlameColor("Flame Color", Color) = (1, 0.5, 0, 1) // Orange
        _DistortionStrength("Distortion Strength", Range(0, 0.1)) = 0.05
        _FlameSpeed("Flame Speed", Range(0, 5)) = 1.0
        _FlickerIntensity("Flicker Intensity", Range(0, 1)) = 0.5
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
                float4 _FlameColor;
                float _DistortionStrength;
                float _FlameSpeed;
                float _FlickerIntensity;
                float _TimeY;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Wavy distortion for flame flicker
                    float distortion = sin(i.uv.y * 20 + _Time.y * _FlameSpeed) * _DistortionStrength;
                    distortion += sin(i.uv.y * 40 + _Time.y * _FlameSpeed * 1.5) * (_DistortionStrength * 0.5);

                    // Apply distortion
                    float2 distortedUV = i.uv + float2(distortion, 0);

                    // Flicker effect by modulating alpha
                    float flicker = sin(_Time.y * 10) * _FlickerIntensity;

                    // Sample the texture
                    fixed4 texColor = tex2D(_MainTex, distortedUV);

                    // Apply flame color and flicker
                    fixed4 flame = texColor * _FlameColor;
                    flame.a *= 1.0 - flicker;

                    return flame;
                }
                ENDCG
            }
        }
}
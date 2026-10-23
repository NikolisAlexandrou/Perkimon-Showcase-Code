Shader "Custom/2DHologramShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ScanlineIntensity("Scanline Intensity", Range(0, 1)) = 0.5
        _GlitchSpeed("Glitch Speed", Range(0, 10)) = 2.0
        _DistortionStrength("Distortion Strength", Range(0, 0.1)) = 0.02
        _HologramColor("Hologram Color", Color) = (0, 1, 1, 1) // Cyan color
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
                float _ScanlineIntensity;
                float _GlitchSpeed;
                float _DistortionStrength;
                float4 _HologramColor;
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
                    // Apply distortion for glitch effect
                    float distortion = sin(i.uv.y * 50 + _Time.y * _GlitchSpeed) * _DistortionStrength;
                    float2 distortedUV = i.uv + float2(distortion, 0);

                    // Sample the texture
                    fixed4 texColor = tex2D(_MainTex, distortedUV);

                    // Apply hologram color tint
                    texColor.rgb *= _HologramColor.rgb;

                    // Add scanline effect
                    float scanline = sin(i.uv.y * 500 + _Time.y * _GlitchSpeed * 5.0) * _ScanlineIntensity;
                    texColor.rgb += scanline;

                    // Apply transparency and glow effect
                    texColor.a *= 0.8 + 0.2 * sin(_Time.y * 4.0);

                    return texColor;
                }
                ENDCG
            }
        }
}

Shader "Custom/RainbowShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _RainbowSpeed("Rainbow Speed", Range(0, 5)) = 1.0
        _Intensity("Color Intensity", Range(0, 2)) = 1.0
        _Transparency("Transparency", Range(0, 1)) = 1.0
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
                float _RainbowSpeed;
                float _Intensity;
                float _Transparency;
                float _TimeY;

                // Convert HSV to RGB for rainbow cycling
                fixed3 HSVtoRGB(float h, float s, float v)
                {
                    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                    float3 p = abs(frac(h + K.xyz) * 6.0 - K.www);
                    return v * lerp(K.xxx, saturate(p - K.xxx), s);
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
                    // Animate the hue based on time and UV coordinates
                    float hue = frac(i.uv.x + _Time.y * _RainbowSpeed);

                // Convert HSV to RGB for the rainbow effect
                fixed3 rainbowColor = HSVtoRGB(hue, 1.0, _Intensity);

                // Sample the original texture
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Apply the rainbow effect and maintain texture alpha
                fixed4 finalColor = fixed4(rainbowColor, texColor.a * _Transparency);

                return finalColor;
            }
            ENDCG
        }
        }
}

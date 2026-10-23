Shader "Custom/RGBShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Intensity("RGB Split Intensity", Range(0, 0.05)) = 0.01
        _GlitchSpeed("Glitch Speed", Range(0, 10)) = 1.0
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
                float _Intensity;
                float _GlitchSpeed;
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
                    // Calculate dynamic shift based on time for glitch effect
                    float shift = sin(_Time.y * _GlitchSpeed) * _Intensity;

                // Apply RGB channel shifts
                float2 redUV = i.uv + float2(shift, 0);
                float2 greenUV = i.uv;
                float2 blueUV = i.uv - float2(shift, 0);

                // Sample each color channel separately
                fixed4 red = tex2D(_MainTex, redUV);
                fixed4 green = tex2D(_MainTex, greenUV);
                fixed4 blue = tex2D(_MainTex, blueUV);

                // Combine channels
                fixed4 finalColor;
                finalColor.r = red.r;
                finalColor.g = green.g;
                finalColor.b = blue.b;
                finalColor.a = (red.a + green.a + blue.a) / 3.0; // Preserve average alpha

                return finalColor;
            }
            ENDCG
        }
        }
}
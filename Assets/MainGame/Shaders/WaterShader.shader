Shader "Custom/WaterShader"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _WaveStrength("Wave Strength", Range(0, 0.1)) = 0.02
        _WaveSpeed("Wave Speed", Range(0, 5)) = 1.0
        _DistortionFrequency("Distortion Frequency", Range(1, 20)) = 10
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
                float _WaveStrength;
                float _WaveSpeed;
                float _DistortionFrequency;
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
                    // Wave distortion based on time
                    float wave = sin((i.uv.y * _DistortionFrequency + _Time.y * _WaveSpeed)) * _WaveStrength;
                    float2 distortedUV = i.uv + float2(wave, 0);

                    // Sample the texture with distorted UV
                    fixed4 col = tex2D(_MainTex, distortedUV);
                    return col;
                }
                ENDCG
            }
        }
}

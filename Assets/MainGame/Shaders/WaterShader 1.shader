Shader "Custom/WaterShader 1"
{
    Properties
    {
        _MainTex("Water Texture", 2D) = "white" {}
        _WaveSpeed("Wave Speed", Range(0, 5)) = 1.0
        _WaveStrength("Wave Strength", Range(0, 0.1)) = 0.02
        _DistortionFrequency("Distortion Frequency", Range(1, 20)) = 10.0
        _ReflectionTint("Reflection Tint", Color) = (0.5, 0.8, 1, 1)
        _Transparency("Transparency", Range(0, 1)) = 0.8
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
                float _WaveSpeed;
                float _WaveStrength;
                float _DistortionFrequency;
                float4 _ReflectionTint;
                float _Transparency;
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
                    wave += sin((i.uv.x * _DistortionFrequency * 1.5 + _Time.y * _WaveSpeed * 1.2)) * (_WaveStrength * 0.5);

                    // Apply distortion to UVs
                    float2 distortedUV = i.uv + float2(wave, wave * 0.5);

                    // Sample the texture
                    fixed4 waterColor = tex2D(_MainTex, distortedUV);

                    // Add reflection tint
                    waterColor.rgb = lerp(waterColor.rgb, _ReflectionTint.rgb, 0.3);

                    // Apply transparency
                    waterColor.a *= _Transparency;

                    return waterColor;
                }
                ENDCG
            }
        }
}
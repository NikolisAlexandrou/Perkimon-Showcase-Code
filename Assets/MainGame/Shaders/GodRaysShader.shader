Shader "Custom/2DGodRaysShader"
{
    Properties
    {
        _MainTex("Base Texture", 2D) = "white" {}
        _LightColor("Light Color", Color) = (1, 1, 0.8, 1)
        _Intensity("Intensity", Range(0, 5)) = 1.5
        _RayLength("Ray Length", Range(0, 1)) = 0.5
        _LightPosition("Light Position (UV)", Vector) = (0.5, 0.5, 0, 0)
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
                float4 _LightColor;
                float _Intensity;
                float _RayLength;
                float4 _LightPosition;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Direction from light source to current fragment
                    float2 lightDir = i.uv - _LightPosition.xy;
                    float dist = length(lightDir);

                    // Accumulate light samples along the direction
                    float numSamples = 50.0;
                    float decay = 0.95;
                    float exposure = _Intensity;
                    float density = _RayLength;
                    float weight = 0.3;

                    fixed4 color = 0;
                    float2 deltaTexCoord = lightDir * density / numSamples;
                    float2 texCoord = i.uv;

                    for (int i = 0; i < numSamples; i++)
                    {
                        texCoord -= deltaTexCoord;
                        fixed4 sample = tex2D(_MainTex, texCoord);
                        sample *= weight;
                        color += sample;
                        weight *= decay;
                    }

                    color *= exposure;
                    color.rgb *= _LightColor.rgb;
                    color.a = 1.0;

                    return color;
                }
                ENDCG
            }
        }
}
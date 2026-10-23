Shader "Custom/2DBlurShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BlurSize("Blur Size", Range(0, 5)) = 1.0
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
                float _BlurSize;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Offsets for blur sampling
                    float2 offsets[9] = {
                        float2(-1, -1), float2(0, -1), float2(1, -1),
                        float2(-1,  0), float2(0,  0), float2(1,  0),
                        float2(-1,  1), float2(0,  1), float2(1,  1)
                    };

                    float kernel[9] = { // Gaussian kernel
                        1, 2, 1,
                        2, 4, 2,
                        1, 2, 1
                    };

                    fixed4 color = fixed4(0, 0, 0, 0);
                    float totalWeight = 16.0; // Sum of kernel values

                    for (int j = 0; j < 9; j++)
                    {
                        float2 sampleUV = i.uv + offsets[j] * _BlurSize / _ScreenParams.xy;
                        color += tex2D(_MainTex, sampleUV) * kernel[j];
                    }

                    return color / totalWeight;
                }
                ENDCG
            }
        }
}

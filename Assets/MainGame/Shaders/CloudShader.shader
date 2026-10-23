Shader "Custom/2DCloudShader"
{
    Properties
    {
        _MainTex("Base Texture", 2D) = "white" {}
        _CloudColor("Cloud Color", Color) = (1, 1, 1, 1)
        _Speed("Cloud Speed", Range(0, 5)) = 0.5
        _Density("Cloud Density", Range(0, 1)) = 0.5
        _Scale("Noise Scale", Range(1, 10)) = 3.0
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
                float4 _CloudColor;
                float _Speed;
                float _Density;
                float _Scale;
                float _Transparency;
                float _TimeY;

                // Simple 2D noise function
                float noise(float2 p)
                {
                    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
                }

                // Smooth noise for cloud texture
                float smoothNoise(float2 p)
                {
                    float2 ip = floor(p);
                    float2 u = frac(p);

                    u = u * u * (3.0 - 2.0 * u); // Smoothstep

                    float n00 = noise(ip);
                    float n01 = noise(ip + float2(0.0, 1.0));
                    float n10 = noise(ip + float2(1.0, 0.0));
                    float n11 = noise(ip + float2(1.0, 1.0));

                    return lerp(lerp(n00, n10, u.x), lerp(n01, n11, u.x), u.y);
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
                    // Moving UVs for cloud animation
                    float2 uv = i.uv;
                    uv.x += _Time.y * _Speed;

                    // Generate cloud noise
                    float noiseValue = smoothNoise(uv * _Scale);

                    // Apply density to control cloud formation
                    float alpha = smoothstep(_Density, 1.0, noiseValue) * _Transparency;

                    // Final color with adjustable alpha
                    fixed4 cloudColor = _CloudColor;
                    cloudColor.a = alpha;

                    return cloudColor;
                }
                ENDCG
            }
        }
}

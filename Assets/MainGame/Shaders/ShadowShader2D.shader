Shader "Custom/ShadowShader2D"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _ShadowColor("Shadow Color", Color) = (0, 0, 0, 0.5)
        _ShadowOffset("Shadow Offset", Vector) = (0.05, -0.05, 0, 0)
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
                float4 _ShadowColor;
                float4 _ShadowOffset;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Sample the shadow texture with an offset
                    float2 shadowUV = i.uv + _ShadowOffset.xy;
                    fixed4 shadow = tex2D(_MainTex, shadowUV) * _ShadowColor;

                    // Sample the original sprite texture
                    fixed4 original = tex2D(_MainTex, i.uv);

                    // Combine shadow and original sprite
                    return shadow * (1 - original.a) + original;
                }
                ENDCG
            }
        }
}
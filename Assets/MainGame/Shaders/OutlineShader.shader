Shader "Custom/2DOutlineShader"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineThickness("Outline Thickness", Range(0, 10)) = 2
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
                float4 _OutlineColor;
                float _OutlineThickness;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float2 pixelSize = _OutlineThickness / _ScreenParams.xy;

                    // Sample surrounding pixels to detect edges
                    float alpha = tex2D(_MainTex, i.uv).a;
                    alpha += tex2D(_MainTex, i.uv + float2(pixelSize.x, 0)).a;
                    alpha += tex2D(_MainTex, i.uv + float2(-pixelSize.x, 0)).a;
                    alpha += tex2D(_MainTex, i.uv + float2(0, pixelSize.y)).a;
                    alpha += tex2D(_MainTex, i.uv + float2(0, -pixelSize.y)).a;

                    // Determine if this pixel is part of the outline
                    fixed4 outline = _OutlineColor;
                    outline.a = step(0.01, alpha) * (1 - tex2D(_MainTex, i.uv).a);

                    // Combine the original sprite with the outline
                    fixed4 original = tex2D(_MainTex, i.uv);
                    return original + outline;
                }
                ENDCG
            }
        }
}

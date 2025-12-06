Shader "Custom/CameraFade"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _HideRadius ("Hide Radius", Range(0, 0.5)) = 0.2
        _EdgeSoftness ("Edge Softness", Range(0, 0.25)) = 0.05
        _MinAlpha ("Min Alpha", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _HideRadius;
            float _EdgeSoftness;
            float _MinAlpha;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                float2 vp = (i.screenPos.xy / i.screenPos.w);
                float d = distance(vp, float2(0.5, 0.5));
                float fade = smoothstep(_HideRadius - _EdgeSoftness, _HideRadius + _EdgeSoftness, d);
                float alphaFade = max(fade, _MinAlpha);
                return fixed4(tex.rgb * i.color.rgb, tex.a * i.color.a * alphaFade);
            }
            ENDCG
        }
    }
}

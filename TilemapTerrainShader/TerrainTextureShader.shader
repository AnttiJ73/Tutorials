Shader "Custom/TextureWorldShader"
{
    Properties
    {
        _MainTex    ("Alpha Tex (UV0 A)", 2D) = "white" {}
        _ColorTex   ("World-Sampled Color Tex", 2D) = "white" {}
        _WorldScale ("World UV Scale (x,y)", Vector) = (1,1,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _ColorTex;
            float4 _WorldScale;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;      // SpriteRenderer.color
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float2 worldUV  : TEXCOORD1;
                fixed4 color    : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uv    = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;

                float3 wp = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldUV = wp.xy * _WorldScale.xy + _WorldOffset.xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed a = tex2D(_MainTex, i.uv).a;
                fixed3 rgb = tex2D(_ColorTex, i.worldUV).rgb;

                rgb *= i.color.rgb;
                a   *= i.color.a;

                return fixed4(rgb, a);
            }
            ENDCG
        }
    }
}

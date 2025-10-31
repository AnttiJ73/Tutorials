Shader "Custom/TextureFilledShader"
{
    Properties
    {
        _MainTex("Main Tex", 2D)= "white" {}
        _FillColor ("Fill Color", Color) = (0,0,0,1)
        _BGColor ("BG Color", Color) = (0,0,0,1)
        _Value ("Current Value", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Value;
            fixed4 _BGColor;
            fixed4 _FillColor;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 mask = tex2D(_MainTex, i.uv);
               
                if (mask.x > _Value * 1.015 - 0.01){
                    return fixed4(_FillColor.rgb, mask.a);
                }else{
                    return fixed4(_BGColor.rgb, mask.a);;
                }
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}
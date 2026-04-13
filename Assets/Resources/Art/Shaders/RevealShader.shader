Shader "Custom/RevealShader"
{
    Properties
    {
        _MainTex ("Base Texture (Background)", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "black" {}
        _Color ("Overlay Color", Color) = (0.5, 1, 0.5, 1)
        _RevealSoftness ("Reveal Softness", Range(0, 0.1)) = 0.01
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
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
            sampler2D _MaskTex;
            float4 _Color;
            float _RevealSoftness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float mask = tex2D(_MaskTex, i.uv).r;
                float reveal = smoothstep(0, _RevealSoftness, mask);
                
                fixed4 overlayColor = _Color;
                overlayColor.a *= (1 - reveal);
                
                return overlayColor;
            }
            ENDCG
        }
    }
}
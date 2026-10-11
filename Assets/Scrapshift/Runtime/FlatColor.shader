Shader "Scrapshift/FlatColor"
{
    Properties { _Color ("Color", Color) = (1,1,1,1) _Flash ("Hit flash", Range(0,1)) = 0 }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            float _Flash;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 position : SV_POSITION; };
            v2f vert(appdata v) { v2f o; o.position = UnityObjectToClipPos(v.vertex); return o; }
            fixed4 frag(v2f i) : SV_Target { return lerp(_Color, fixed4(1,1,.9,1), _Flash); }
            ENDCG
        }
    }
}

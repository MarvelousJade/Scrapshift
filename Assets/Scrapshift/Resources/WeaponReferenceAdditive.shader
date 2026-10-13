Shader "Scrapshift/WeaponReferenceAdditive" {
 Properties { _MainTex ("Texture", 2D) = "white" {} }
 SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" } Cull Off ZWrite Off Blend SrcAlpha One
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
 v2f vert(appdata_full v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord;o.color=v.color;return o;}
 fixed4 frag(v2f i):SV_Target {return tex2D(_MainTex,i.uv)*i.color;}
 ENDCG }
 } }

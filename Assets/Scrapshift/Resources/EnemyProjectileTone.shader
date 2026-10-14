Shader "Scrapshift/EnemyProjectileTone"
{
    Properties { [PerRendererData] _MainTex ("Sprite",2D)="white"{} }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct Input {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            struct Output {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            sampler2D _MainTex;
            Output vert(Input v){Output o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
            fixed4 frag(Output i):SV_Target
            {
                fixed4 s=tex2D(_MainTex,i.uv);
                // Source enemy double-color shader: black becomes red, white stays white.
                return fixed4(1,s.g,s.b,s.a)*i.color;
            }
            ENDCG
        }
    }
}

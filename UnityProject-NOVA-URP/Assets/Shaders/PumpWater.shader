Shader "LightFeast/PumpWater" {
 Properties {_MainTex("Water",2D)="white"{} _Color("Tint",Color)=(1,1,1,1)}
 SubShader {Tags {"Queue"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _Color;
 struct v{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};v vert(appdata_base i){v o;o.p=UnityObjectToClipPos(i.vertex);o.uv=i.texcoord;return o;}
 fixed4 frag(v i):SV_Target{float2 q=i.uv;q.x+=sin(i.uv.y*220+_Time.y*1.5)*.0007;q.y+=sin(i.uv.x*120-_Time.y*1.3)*.0005;return tex2D(_MainTex,q)*_Color;}
 ENDCG}
 }
}

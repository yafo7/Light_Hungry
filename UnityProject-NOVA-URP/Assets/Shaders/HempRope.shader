Shader "LightFeast/HempRope" {
 Properties {_MainTex("Painted hemp fibers",2D)="white"{} _Color("Matte brown tint",Color)=(0.85,0.8,0.74,1) _TileLength("World length of a repeat",Float)=1.1}
 SubShader { Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"} Cull Off ZWrite On
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _Color;float _TileLength;
 struct v {float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
 v vert(appdata_base a){v o;o.p=UnityObjectToClipPos(a.vertex);o.uv=a.texcoord;return o;}
 fixed4 frag(v i):SV_Target{
  // Symmetric repeat avoids transparent joins. The vertical window retains the original PNG alpha.
  float u=1-abs(frac(i.uv.x/(2*_TileLength))*2-1);
  fixed4 c=tex2D(_MainTex,float2(u,lerp(.24,.76,i.uv.y)))*_Color;
  clip(c.a-.08);return fixed4(c.rgb,1);
 }
 ENDCG }
 }
}

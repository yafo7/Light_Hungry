Shader "LightFeast/PaintedLayer" {
 Properties { _MainTex("Watercolour",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) _Cutoff("Cutout",Range(0,1))=0.025 _Erase("Hide baked mirror",Float)=0 }
 SubShader { Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" } Cull Off ZWrite On
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex; float4 _Color; float _Cutoff,_Erase;
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord;return o;}
 fixed4 frag(v2f i):SV_Target {
  // The source remains unchanged. Only the silver panel is hidden for its rotating 3D replacement.
  float2 p=float2(i.uv.x*1374,(1-i.uv.y)*1145);
  if(_Erase>0.5 && p.y>238 && p.y<770 && p.x>860 && p.x<1202) {
   float left=941-(p.y-251)*0.174; float right=1188-(p.y-310)*0.157;
   if(p.x>left-13 && p.x<right+13) discard;
  }
  fixed4 c=tex2D(_MainTex,i.uv)*_Color;clip(c.a-_Cutoff);return c;
 }
 ENDCG }
 }
}

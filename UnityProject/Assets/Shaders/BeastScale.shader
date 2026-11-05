Shader "LightFeast/BeastScale" {
 Properties {_MainTex("Painted beast",2D)="white"{} _Exposure("Target scale awakening",Range(0,1))=0}
 SubShader {Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"} Cull Off ZWrite On
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float _Exposure;
 struct v {float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
 v vert(appdata_base a){v o;o.p=UnityObjectToClipPos(a.vertex);o.uv=a.texcoord;return o;}
 float side(float2 a,float2 b,float2 p){float2 d=b-a;return (d.x*(p.y-a.y)-d.y*(p.x-a.x))/length(d);}
 fixed4 frag(v i):SV_Target{
  fixed4 c=tex2D(_MainTex,i.uv);clip(c.a-.025);float2 p=float2(i.uv.x,1-i.uv.y)*1254;
  float edge=side(float2(1094,988),float2(1085,1050),p);
  edge=min(edge,side(float2(1085,1050),float2(1060,1092),p));
  edge=min(edge,side(float2(1060,1092),float2(960,1141),p));
  edge=min(edge,side(float2(960,1141),float2(971,1065),p));
  edge=min(edge,side(float2(971,1065),float2(989,1028),p));
  edge=min(edge,side(float2(989,1028),float2(1030,1006),p));
  edge=min(edge,side(float2(1030,1006),float2(1094,988),p));
  float target=smoothstep(-3,3,edge);float lum=dot(c.rgb,float3(.299,.587,.114));
  float foliage=saturate((c.g-c.b-.15)*5)*saturate((.75-c.r)*4);
  float3 pale=lerp(float3(.35,.38,.40),float3(.96,.96,.91),pow(lum,.55));
  pale=lerp(pale,c.rgb,foliage);
  float3 dark=float3(.42,.45,.46)*(.3+.7*lum);
  return float4(lerp(pale,lerp(dark,c.rgb,_Exposure),target),c.a);
 }
 ENDCG}
 }
}

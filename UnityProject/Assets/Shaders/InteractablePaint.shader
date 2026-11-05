Shader "LightFeast/InteractablePaint" {
 Properties {_MainTex("Painted prop",2D)="white"{} _Color("Tint",Color)=(1,1,1,1)}
 SubShader {Tags {"Queue"="AlphaTest+10" "RenderType"="TransparentCutout"} Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite On
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _Color;
 struct v {float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
 v vert(appdata_base a){v o;o.p=UnityObjectToClipPos(a.vertex);o.uv=a.texcoord;return o;}
 fixed4 frag(v i):SV_Target{
  float2 dx=ddx(i.uv),dy=ddy(i.uv);fixed4 c=tex2D(_MainTex,i.uv);float nearA=0,farA=0;
  for(int n=0;n<8;n++){float a=n*.785398;float2 d=cos(a)*dx+sin(a)*dy;nearA=max(nearA,tex2D(_MainTex,i.uv+d*1.7).a);farA=max(farA,tex2D(_MainTex,i.uv+d*4).a);}
  float glow=saturate(nearA-c.a)*.52+saturate(farA-nearA)*.16;
  float alpha=c.a+glow*(1-c.a);clip(alpha-.025);
  float3 rgb=lerp(float3(.92,.97,1),c.rgb*_Color.rgb,c.a/max(alpha,.001));
  return float4(rgb,alpha);
 }
 ENDCG}
 }
}

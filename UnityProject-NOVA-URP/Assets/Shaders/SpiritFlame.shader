Shader "LightFeast/SpiritFlame" {
 Properties {_Color("Flame tint",Color)=(1,1,1,1)}
 SubShader {Tags {"Queue"="Transparent+15" "RenderType"="Transparent"} Blend SrcAlpha One ZWrite Off Cull Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _Color;
 struct a {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
 struct v {float4 pos:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
 v vert(a i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.uv=i.uv;o.color=i.color;return o;}
 fixed4 frag(v i):SV_Target{
  float y=i.uv.y;float x=i.uv.x-.5-sin(y*6+_Time.y*4)*.075*y;float width=lerp(.37,.025,pow(y,1.1));float edge=saturate(1-abs(x)/width);
  float mask=pow(edge,1.6)*smoothstep(0,.14,y)*(1-smoothstep(.86,1,y));float3 color=lerp(float3(1,.55,.13),float3(1,.97,.75),pow(edge,3));
  return float4(color*i.color.rgb*_Color.rgb,mask*i.color.a*_Color.a);
 }
 ENDCG}
 }
}

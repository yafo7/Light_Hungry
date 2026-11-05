Shader "LightFeast/ReservoirWater" {
 Properties { _MainTex("Scene water source",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) _WavePixels("Ripple amplitude",Range(0,3))=0.85 }
 SubShader { Tags {"Queue"="Transparent-20" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex; float4 _Color; float _WavePixels;
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord;return o;}
 float box(float2 p,float4 r,float f){return smoothstep(r.x,r.x+f,p.x)*(1-smoothstep(r.z-f,r.z,p.x))*smoothstep(r.y,r.y+f,p.y)*(1-smoothstep(r.w-f,r.w,p.y));}
 fixed4 frag(v2f i):SV_Target {
  float2 p=float2(i.uv.x*1859,(1-i.uv.y)*846);float t=_Time.y;
  // Only source water pixels are animated; bank and root silhouettes stay fixed.
  float pool=max(box(p,float4(373,584,641,796),8),box(p,float4(1249,584,1414,796),8));
  float falls=max(box(p,float4(68,536,113,572),4),box(p,float4(1768,540,1805,705),6));
  float mask=max(pool,falls);clip(mask-.005);
  float2 q=i.uv;
  q.x+=pool*_WavePixels*(sin(p.y*.11+t*1.4)+.35*sin(p.y*.27-t*.9))/1859;
  q.y+=pool*_WavePixels*(sin(p.x*.045-t*1.1)+.3*sin(p.x*.11+t*.6))/846;
  float phase=frac(t*.6),phase2=frac(phase+.5),blend=abs(phase*2-1);
  float3 baseColor=tex2D(_MainTex,q).rgb;
  float3 stream=lerp(tex2D(_MainTex,i.uv+float2(0,phase*.016)).rgb,tex2D(_MainTex,i.uv+float2(0,phase2*.016)).rgb,blend);
  float glimmer=sin(p.x*.16+p.y*.25-t*2)*sin(p.x*.043-p.y*.18+t*.9);
  float3 water=baseColor*(1+.055*glimmer);
  water=lerp(water,lerp(baseColor,stream,.5)*(1+.08*sin(p.y*.29-t*9)),falls);
  return fixed4(water*_Color.rgb,mask*_Color.a);
 }
 ENDCG }
 }
}

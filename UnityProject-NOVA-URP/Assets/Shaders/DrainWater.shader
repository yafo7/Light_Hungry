Shader "LightFeast/DrainWater" {
 Properties {
  _MainTex("Painted water",2D)="white"{}
  _Color("Tint",Color)=(1,1,1,1)
  _WavePixels("Surface wave amplitude in source pixels",Range(0,5))=2.2
  _FlowSpeed("Downstream waterfall speed",Range(0,3))=1
 }
 SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _Color;float _WavePixels,_FlowSpeed;
 struct v {float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
 v vert(appdata_base a){v o;o.p=UnityObjectToClipPos(a.vertex);o.uv=a.texcoord;return o;}
 float ripple(float2 pixel,float cx,float t){
  float2 d=float2((pixel.x-cx)/1.8,(pixel.y-541)*2.5);
  float r=length(d);float fade=exp(-r*.026)*smoothstep(534,548,pixel.y);
  return pow(saturate(sin(r*.27-t*4.5)),7)*fade;
 }
 fixed4 frag(v i):SV_Target{
  float t=_Time.y;float2 uv=i.uv;float2 pixel=float2(uv.x*2170,(1-uv.y)*725);
  float surface=smoothstep(504,544,pixel.y);
  float2 q=uv;
  // Oscillate the painted reflections in two directions without moving the banks.
  q.x+=(sin(uv.y*155+t*1.7)+.45*sin(uv.y*317-t*1.1))*_WavePixels/2170*surface;
  q.y+=(sin(uv.x*95-t*1.9)+.35*sin(uv.x*213+t*.9))*_WavePixels/725*surface;
  fixed4 baseWater=tex2D(_MainTex,q);
  fixed4 original=tex2D(_MainTex,uv);
  // Advect two offset samples downward. Cross-fading the reset keeps the fall continuous.
  // Keep the original alpha silhouette: mouths and impact points cannot drift up/down.
  float phase=frac(t*.8*_FlowSpeed),phaseB=frac(phase+.5);
  float crossfade=abs(phase*2-1);
  float2 flowUV=uv;
  flowUV.x+=sin(pixel.y*.11-t*3.4)*.6/2170;
  fixed4 fallA=tex2D(_MainTex,flowUV+float2(0,phase*.055));
  fixed4 fallB=tex2D(_MainTex,flowUV+float2(0,phaseB*.055));
  float3 falling=lerp(lerp(original.rgb,fallA.rgb,smoothstep(.25,.8,fallA.a)),lerp(original.rgb,fallB.rgb,smoothstep(.25,.8,fallB.a)),crossfade);
  float stream=.5+.5*sin(pixel.y*.15-t*10*_FlowSpeed+sin(pixel.x*.18)*2);
  float strands=pow(saturate(sin(pixel.x*.7+sin(pixel.y*.017)*1.5)),5);
  float3 fallColor=lerp(original.rgb,falling,.45)*(.84+.24*stream);
  fallColor+=float3(.10,.19,.22)*strands*pow(stream,4);
  float shimmer=sin(uv.x*180+uv.y*310-t*2.1)*sin(uv.x*71-uv.y*190+t*1.3);
  float rings=ripple(pixel,150,t)+ripple(pixel,302,t+.6)+ripple(pixel,1048,t+1.4)+ripple(pixel,1525,t+.3)+ripple(pixel,1655,t+.9)+ripple(pixel,2136,t+1.8);
  float3 surfaceColor=baseWater.rgb*(1+.12*shimmer)+float3(.12,.24,.27)*rings;
  fixed4 c=fixed4(lerp(fallColor,surfaceColor,surface),lerp(original.a,baseWater.a,surface))*_Color;
  clip(c.a-.025);return c;
 }
 ENDCG }
 }
}

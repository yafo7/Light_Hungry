Shader "LightHungry/LightColumn" {
 Properties { _Tint("Tint",Color)=(.84,.94,1,.75) _Length("Length",Float)=10 }
 SubShader { Tags { "Queue"="Transparent+18" "RenderType"="Transparent" } Blend SrcAlpha OneMinusSrcAlpha Cull Back ZWrite Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;};
 struct v2f {float4 pos:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float2 uv:TEXCOORD2;};float4 _Tint;float _Length;
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.uv=v.uv;return o;}
 fixed4 frag(v2f i):SV_Target {
  float facing=saturate(dot(normalize(i.normal),normalize(_WorldSpaceCameraPos-i.world)));
  float soft=pow(facing,1.25);float z=i.uv.y*_Length;
  float flow=.8+.12*sin(z*3-_Time.y*2)+.08*sin(z*9+sin(i.uv.x*31)-_Time.y*4);
  float wisps=pow(saturate(sin(i.uv.x*57+sin(z*1.5-_Time.y)*1.2)),14)*.16;
  float endFade=saturate(i.uv.y*_Length*14)*saturate((1-i.uv.y)*_Length*14);
  float alpha=saturate(_Tint.a*(soft*.8+pow(facing,10)*.5+wisps*soft)*flow*endFade);
  float3 color=lerp(_Tint.rgb,float3(1,1,1),pow(facing,6)*.9);
  return fixed4(color,alpha);
 }
 ENDCG }
 }
}

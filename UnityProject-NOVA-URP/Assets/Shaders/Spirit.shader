Shader "LightFeast/Spirit" {
 Properties {_MainTex("Watercolour pigment",2D)="white"{} _Stride("Stride",Float)=0 _Move("Moving",Float)=0 _LightCharge("Light form charge",Range(0,1))=0}
 CGINCLUDE
 #include "UnityCG.cginc"
 float4x4 _SpiritWorldToLocal;
 sampler2D _MainTex;float _Stride,_Move,_LightCharge;
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 normal:TEXCOORD1;float3 view:TEXCOORD2;float3 root:TEXCOORD3;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.root=mul(_SpiritWorldToLocal,mul(unity_ObjectToWorld,v.vertex)).xyz;o.uv=v.texcoord;o.normal=UnityObjectToWorldNormal(v.normal);o.view=WorldSpaceViewDir(v.vertex);return o;}
 v2f outline(appdata_base v,float pixels){v2f o=vert(v);float2 n=mul((float3x3)UNITY_MATRIX_V,o.normal).xy;n/=max(length(n),.001);o.pos.xy+=n*pixels*2/_ScreenParams.xy*o.pos.w;return o;}
 v2f wide(appdata_base v){return outline(v,7+_LightCharge*5);}
 v2f medium(appdata_base v){return outline(v,3.2+_LightCharge*3);}
 v2f fine(appdata_base v){return outline(v,1.05+_LightCharge);}
 float hairAlpha(v2f i){float rear=-i.root.x*.90+(i.root.y-.50)*.45;return 1-smoothstep(.28,.48,rear)*smoothstep(.58,.70,i.root.y);}
 fixed4 haloWide(v2f i):SV_Target{clip(hairAlpha(i)-.999);return float4(1,.67,.22,(.065+_LightCharge*.06)*hairAlpha(i));}
 fixed4 haloMedium(v2f i):SV_Target{clip(hairAlpha(i)-.999);return float4(1,.78,.38,(.15+_LightCharge*.08)*hairAlpha(i));}
 fixed4 outlineColor(v2f i):SV_Target{clip(hairAlpha(i)-.999);return float4(1,.96,.76,.84*hairAlpha(i));}
 fixed4 pigment(v2f i):SV_Target{
  float3 ink=tex2D(_MainTex,i.uv).rgb;float lum=dot(ink,float3(.299,.587,.114));float paper=smoothstep(.18,.4,lum);
  float3 wash=pow(max(ink,.001),.72)*float3(1.06,1.025,1);
  float light=dot(normalize(i.normal),normalize(float3(-.35,.85,-.5)));
  wash*=lerp(float3(.90,.94,.98),float3(1,1,1),smoothstep(.12,.22,light));
  float3 c=lerp(ink,wash,paper);float rim=pow(1-saturate(dot(normalize(i.normal),normalize(i.view))),5);
  c+=rim*float3(.15,.11,.045)*paper;float gold=smoothstep(.15,.3,ink.r-ink.b)*smoothstep(.3,.65,lum);c+=gold*float3(.13,.09,.015);
  c=lerp(c,float3(1,.97,.79),pow(_LightCharge,2)*.6);c+=rim*_LightCharge*float3(.7,.48,.18);return float4(c,1);
 }
 fixed4 core(v2f i):SV_Target{clip(hairAlpha(i)-.999);return pigment(i);}
 fixed4 tip(v2f i):SV_Target{float a=hairAlpha(i);clip(.999-a);clip(a-.005);float4 c=pigment(i);c.rgb=lerp(c.rgb,float3(1,.94,.72),(1-a)*.32);c.a=a;return c;}
 ENDCG
 SubShader {Tags {"RenderType"="Opaque"}
 Pass {Cull Back ZWrite On Blend Off CGPROGRAM
 #pragma vertex vert
 #pragma fragment core
 ENDCG}
 Pass {Cull Front ZWrite Off Blend SrcAlpha One CGPROGRAM
 #pragma vertex wide
 #pragma fragment haloWide
 ENDCG}
 Pass {Cull Front ZWrite Off Blend SrcAlpha One CGPROGRAM
 #pragma vertex medium
 #pragma fragment haloMedium
 ENDCG}
 Pass {Cull Front ZWrite Off Blend SrcAlpha One CGPROGRAM
 #pragma vertex fine
 #pragma fragment outlineColor
 ENDCG}

 Pass {Cull Back ZWrite Off Blend SrcAlpha OneMinusSrcAlpha CGPROGRAM
 #pragma vertex vert
 #pragma fragment tip
 ENDCG}

 }
}


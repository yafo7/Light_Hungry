Shader "LightFeast/SoftLight" {
 Properties {_Color("Light colour",Color)=(1,0.68,0.2,0.5) _Radial("Radial falloff",Float)=0}
 SubShader { Tags {"Queue"="Transparent+20" "RenderType"="Transparent"} Blend SrcAlpha One ZWrite Off Cull Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _Color;float _Radial;
 struct a {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
 struct v {float4 pos:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
 v vert(a i){v o;o.pos=UnityObjectToClipPos(i.vertex);o.uv=i.uv;o.color=i.color;return o;}
 fixed4 frag(v i):SV_Target {float f=1;if(_Radial>0.5)f=pow(saturate(1-length(i.uv*2-1)),2);else f=pow(saturate(1-abs(i.uv.y*2-1)),1.6);return float4(_Color.rgb*i.color.rgb,_Color.a*i.color.a*f);}
 ENDCG }
 }
}

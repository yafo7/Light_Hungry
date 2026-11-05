Shader "LightFeast/PassageShadow" {
 Properties {_Color("Shadow",Color)=(0.015,0.022,0.018,1)}
 SubShader {Tags {"Queue"="Transparent+40" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _Color;struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord;return o;}
 fixed4 frag(v2f i):SV_Target {float2 p=i.uv*2-1;float a=atan2(p.y,p.x);float edge=.05*sin(a*7+_Time.y*.28)+.025*sin(a*13-_Time.y*.2);float d=length(p)+edge;float alpha=(1-smoothstep(.62,1.0,d))*_Color.a;return float4(_Color.rgb,alpha);}
 ENDCG }
 }
}

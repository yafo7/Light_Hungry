Shader "LightHungry/WatercolorBeam" {
 Properties { _Color("Tint",Color)=(1,1,1,1) }
 SubShader { Tags { "Queue"="Transparent+25" "RenderType"="Transparent" } Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};float4 _Color;
 v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
 fixed4 frag(v2f i):SV_Target {float edge=pow(saturate(1-abs(i.uv.y*2-1)),1.8);float wash=.85+.1*sin(i.uv.x*21-_Time.y*2)+.05*sin(i.uv.x*77+_Time.y);return fixed4(i.color.rgb,i.color.a*edge*wash);}
 ENDCG }
 } 
}

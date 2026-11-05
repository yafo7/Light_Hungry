Shader "LightFeast/ReservoirScale" {
 Properties { _MainTex("Painted scale",2D)="white"{} _Exposure("Received light",Range(0,1))=0 }
 SubShader {Tags {"Queue"="AlphaTest+15" "RenderType"="TransparentCutout"} Cull Off ZWrite On Blend SrcAlpha OneMinusSrcAlpha
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float _Exposure;
 struct v {float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
 v vert(appdata_base a){v o;o.p=UnityObjectToClipPos(a.vertex);o.uv=a.texcoord;return o;}
 fixed4 frag(v i):SV_Target {fixed4 c=tex2D(_MainTex,i.uv);clip(c.a-.025);float lum=dot(c.rgb,float3(.299,.587,.114));float3 gray=lerp(float3(.20,.23,.25),float3(.58,.61,.61),lum);float3 awake=lerp(float3(.82,.53,.16),float3(1,.94,.64),lum);return float4(lerp(gray,awake,_Exposure),c.a);}
 ENDCG }
 }
}

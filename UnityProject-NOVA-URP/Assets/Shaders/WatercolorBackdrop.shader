Shader "LightFeast/WatercolorBackdrop" {
 Properties {
  _MainTex("Pale watercolor background",2D)="white"{}
  _OriginalTex("Original background - retained for comparison",2D)="white"{}
  _Color("Tint",Color)=(1,1,1,1)
  _Cutoff("Cutout",Range(0,1))=0.025
  _StyleBlend("Watercolor strength",Range(0,1))=0.8
  _Softness("Background softness (source pixels)",Range(0,3))=0.65
 }
 SubShader {
  Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"}
  Cull Off ZWrite On
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex, _OriginalTex;
   float4 _MainTex_ST, _MainTex_TexelSize, _Color;
   float _Cutoff, _StyleBlend, _Softness;
   struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
   v2f vert(appdata_base v) {
    v2f o; o.pos=UnityObjectToClipPos(v.vertex);
    o.uv=TRANSFORM_TEX(v.texcoord,_MainTex); return o;
   }
   fixed4 frag(v2f i):SV_Target {
    fixed4 original=tex2D(_OriginalTex,i.uv);
    float2 d=_MainTex_TexelSize.xy*_Softness;
    // Gentle nine-tap smoothing is limited to this backdrop material.
    // At strength zero the exact old texture is returned, with no grading or blur.
    fixed4 wash=tex2D(_MainTex,i.uv)*4;
    wash+=2*(tex2D(_MainTex,i.uv+float2(d.x,0))+tex2D(_MainTex,i.uv-float2(d.x,0))
             +tex2D(_MainTex,i.uv+float2(0,d.y))+tex2D(_MainTex,i.uv-float2(0,d.y)));
    wash+=tex2D(_MainTex,i.uv+d)+tex2D(_MainTex,i.uv-d)
          +tex2D(_MainTex,i.uv+float2(d.x,-d.y))+tex2D(_MainTex,i.uv+float2(-d.x,d.y));
    fixed4 c=lerp(original,wash/16,saturate(_StyleBlend))*_Color;
    clip(c.a-_Cutoff); return c;
   }
   ENDCG
  }
 }
}

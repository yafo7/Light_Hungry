Shader "LightHungry/SpatialActor" {
    Properties {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OccludedColor ("Occluded silhouette", Color) = (.55,.88,.84,.65)
    }
    SubShader {
        Tags { "Queue"="Transparent+20" "RenderType"="TransparentCutout" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        CGINCLUDE
        #include "UnityCG.cginc"
        struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
        struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
        sampler2D _MainTex; fixed4 _Color, _OccludedColor;
        v2f vert(appdata v) { v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o; }
        fixed4 normal(v2f i):SV_Target { fixed4 c=tex2D(_MainTex,i.uv)*i.color;clip(c.a-.035);return c; }
        fixed4 silhouette(v2f i):SV_Target { fixed4 c=tex2D(_MainTex,i.uv)*i.color;clip(c.a-.035);return fixed4(lerp(c.rgb,_OccludedColor.rgb,.8),c.a*_OccludedColor.a); }
        ENDCG
        Pass {
            ZWrite On ZTest LEqual
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment normal
            ENDCG
        }
        Pass {
            ZWrite Off ZTest Greater
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment silhouette
            ENDCG
        }
    }
}

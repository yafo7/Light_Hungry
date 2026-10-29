Shader "LightHungry/SpatialSolid" {
    Properties { _Color ("Color", Color) = (.3,.35,.36,1) }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; };
            struct v2f { float4 vertex:SV_POSITION; float light:TEXCOORD0; };
            fixed4 _Color;
            v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex);float3 n=UnityObjectToWorldNormal(v.normal);o.light=.72+.28*saturate(dot(n,normalize(float3(-.4,1,-.6))));return o; }
            fixed4 frag(v2f i):SV_Target { return fixed4(_Color.rgb*i.light,1); }
            ENDCG
        }
    }
}

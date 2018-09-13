Shader "Custom/Additive Surface Shader" {
    Properties{
        _MainTex( "Texture", 2D ) = "white" {}
        _Color( "_Color", Color ) = (1.0, 0.6, 0.6, 1.0)
    }
    SubShader{
        Tags{ "Queue"="Transparent" "RenderType" = "Transparent" }
        ZWrite Off
        Blend SrcAlpha One // additive One One; soft additive would be OneMinusDstColor One; very soft additive is SrcAlpha One
        CGPROGRAM
        #pragma surface surf Lambert finalcolor:mycolor keepalpha //alpha
        struct Input {
            float2 uv_MainTex;
        };
        fixed4 _Color;
    
        void mycolor( Input IN, SurfaceOutput o, inout fixed4 color )
        {
            color *= _Color;
        }
        sampler2D _MainTex;
        void surf( Input IN, inout SurfaceOutput o ) {
            o.Albedo = tex2D( _MainTex, IN.uv_MainTex ).rgb;
            o.Alpha = tex2D( _MainTex, IN.uv_MainTex ).a;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
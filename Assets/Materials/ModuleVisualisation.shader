Shader "Custom/ModuleVisualisation"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf SimpleUnlit

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        half4 LightingSimpleUnlit(SurfaceOutput s, half3 lightDir, half atten)
        {
            return half4(s.Albedo, 1.0);
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = IN.worldNormal * 0.5 + 0.5;
            // Metallic and smoothness come from slider variables
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

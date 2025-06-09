Shader "Custom/MobileEmissionShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}                 // Texture chính
        _Color("Base Color", Color) = (1,1,1,1)              // Màu cơ bản
        _EmissionColor("Emission Color", Color) = (0,0,0,0)  // Màu phát sáng
        _EmissionIntensity("Emission Intensity", Float) = 1.0 // Độ sáng phát sáng
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 100

            CGPROGRAM
            #pragma surface surf Lambert

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _EmissionColor;
            float _EmissionIntensity;

            struct Input
            {
                float2 uv_MainTex;
            };

            void surf(Input IN, inout SurfaceOutput o)
            {
                fixed4 tex = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = tex.rgb;

                // Thêm emission với độ sáng điều chỉnh
                o.Emission = _EmissionColor.rgb * _EmissionColor.a * _EmissionIntensity;
            }
            ENDCG
        }
            FallBack "Diffuse"
}

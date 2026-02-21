Shader "BattleBG/CharacterImage"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Brightness("Brightness",Range(0,1)) = 1.0
        _Cutoff("Alpha cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="TransperentCutoff" "Queue"="AlphaTest" }
        
        CGPROGRAM
        #pragma surface surf Lambert alphatest:_Cutoff
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        float  _Brightness;

        struct Input
        {
           float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb * _Brightness ;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Legacy Shaders/Transparent/Cutout/VertexLit"
}


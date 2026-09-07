Shader "Exit7/Printed type" {
Properties { _MainTex("Font",2D)="white"{} _Color("Color",Color)=(1,1,1,1) _Cutoff("Cutoff",Range(0,1))=.4 }
SubShader { Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"} Cull Off
CGPROGRAM
#pragma surface surf Lambert alphatest:_Cutoff
sampler2D _MainTex;fixed4 _Color;
struct Input {float2 uv_MainTex;float4 color:COLOR;};
void surf(Input IN,inout SurfaceOutput o){o.Albedo=_Color.rgb*IN.color.rgb;o.Alpha=tex2D(_MainTex,IN.uv_MainTex).a*IN.color.a;}
ENDCG
} }

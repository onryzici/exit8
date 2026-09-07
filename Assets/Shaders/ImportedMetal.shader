Shader "Exit7/Imported painted metal" {
 Properties {
 _MainTex("Color",2D)="white"{} _BumpMap("Normal",2D)="bump"{} _Roughness("Roughness or specular",2D)="gray"{} _MetalMap("Metal",2D)="white"{}
 _Tint("Tint",Color)=(1,1,1,1) _Saturation("Saturation",Range(0,1))=1 _Lift("Paint finish",Range(0,1))=0 _Gloss("Gloss multiplier",Range(0,1))=.7 _IsSpec("Specular workflow",Float)=0 _Metallic("Metallic",Range(0,1))=.8
 }
 SubShader {Tags{"RenderType"="Opaque"} LOD 300
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 sampler2D _MainTex,_BumpMap,_Roughness,_MetalMap; half4 _Tint;half _Saturation,_Lift,_Gloss,_IsSpec,_Metallic;
 struct Input{float2 uv_MainTex;};
 void surf(Input IN,inout SurfaceOutputStandard o){float2 uv=IN.uv_MainTex;float3 c=tex2D(_MainTex,uv).rgb;float l=dot(c,float3(.2126,.7152,.0722));c=lerp(l.xxx,c,_Saturation);o.Albedo=lerp(c,float3(.5,.51,.5),_Lift)*_Tint.rgb;o.Normal=UnpackNormal(tex2D(_BumpMap,uv));float r=tex2D(_Roughness,uv).r;o.Smoothness=lerp(1-r,r,_IsSpec)*_Gloss;o.Metallic=tex2D(_MetalMap,uv).r*_Metallic;}
 ENDCG
 } FallBack "Standard"
}

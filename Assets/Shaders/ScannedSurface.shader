Shader "Exit7/Scanned surface" {
Properties {
 _MainTex("Scanned color",2D)="white"{} _BumpMap("Surface normal",2D)="bump"{}
 _Roughness("Roughness",2D)="white"{} _Height("Joint depth",2D)="white"{}
 _Color("Tint",Color)=(1,1,1,1) _Flatness("Reduce stone contrast",Range(0,1))=0
 _Gloss("Glaze adjustment",Range(0,1))=.7 _NormalStrength("Relief",Range(0,2))=1
 _Grout("Joint darkening",Range(0,1))=.4
 _MicroNormal("Ceramic glaze micro normal",2D)="bump"{} _Finish("Surface finish variation",2D)="gray"{}
 _MicroStrength("Glaze unevenness",Range(0,1))=.25 _MicroScale("Detail repeat",Float)=8
 _RoughnessWeight("Roughness response",Range(0,1))=.3
}
SubShader { Tags {"RenderType"="Opaque"} LOD 350
CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
#include "UnityStandardUtils.cginc"
sampler2D _MainTex,_BumpMap,_Roughness,_Height; fixed4 _Color; half _Gloss,_NormalStrength,_Flatness,_Grout;
sampler2D _MicroNormal,_Finish; half _MicroStrength,_MicroScale,_RoughnessWeight;
struct Input { float2 uv_MainTex; };
void surf(Input IN,inout SurfaceOutputStandard o){
 float2 uv=IN.uv_MainTex; float3 c=tex2D(_MainTex,uv).rgb;
 float h=tex2D(_Height,uv).r; float joint=1-smoothstep(.10,.36,h);
 c=lerp(c,float3(.58,.59,.58),_Flatness); c*=1-joint*_Grout;
 float finish=tex2D(_Finish,uv*_MicroScale*.25).r;
 o.Albedo=c*_Color.rgb*(.985+finish*.03);
 o.Normal=BlendNormals(UnpackScaleNormal(tex2D(_BumpMap,uv),_NormalStrength),UnpackScaleNormal(tex2D(_MicroNormal,uv*_MicroScale),_MicroStrength));
 float rough=tex2D(_Roughness,uv).r;
 o.Smoothness=saturate(_Gloss-rough*_RoughnessWeight-joint*.36-(finish-.5)*.10); o.Metallic=0; o.Occlusion=1-joint*.3;
}
ENDCG
} FallBack "Standard" }

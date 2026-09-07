Shader "Station/AnomalyWater" {
 Properties {_Color("Water absorption",Color)=(.035,.075,.058,1) _FrontZ("Leading edge",Float)=25.2 _Rise("Water depth",Float)=1.25 _Crest("Surge crest",Float)=.85 _FlowAxis("Flow axis",Vector)=(0,1,0,0)}
 SubShader {Tags{"Queue"="Transparent" "RenderType"="Transparent"} LOD 300
 GrabPass {"_FloodRefraction"}
 CGPROGRAM
 #pragma surface surf Standard alpha:premul vertex:vert
 #pragma target 3.0
 #include "UnityCG.cginc"
 fixed4 _Color;float _FrontZ,_Rise,_Crest;float4 _FlowAxis;sampler2D _FloodRefraction;UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
 struct Input {float3 worldPos;float4 screenPos;};
 float hash21(float2 p){p=frac(p*float2(123.34,456.21));p+=dot(p,p+45.32);return frac(p.x*p.y);}
 float noise21(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash21(i),hash21(i+float2(1,0)),f.x),lerp(hash21(i+float2(0,1)),hash21(i+1),f.x),f.y);}
 float fbm(float2 p){return noise21(p)*.57+noise21(p*2.07+13.4)*.28+noise21(p*4.13+5.7)*.15;}
 float edgeOffset(float x){return (noise21(float2(x*2.8,_Time.y*.35))-.5)*.20;}
 void vert(inout appdata_full v){float3 p=mul(unity_ObjectToWorld,v.vertex).xyz;float d=max(0,dot(p.xz,_FlowAxis.xy)-_FrontZ-edgeOffset(p.x));float edge=1-exp(-d*3.2);float crest=exp(-pow((d-.7)*1.9,2))*_Crest*(.65+.55*fbm(float2(p.x*2.1,_Time.y*.7)));float ripple=sin(p.z*11+_Time.y*3.5)*.009+sin(p.x*17-p.z*13+_Time.y*2)*.006+(fbm(p.xz*12+_Time.y)-.5)*.09;v.vertex.y+=edge*(_Rise+crest+ripple);}
 void surf(Input IN,inout SurfaceOutputStandard o){if(_FlowAxis.x>.5)clip(IN.worldPos.x-2.23);float d=max(0,dot(IN.worldPos.xz,_FlowAxis.xy)-_FrontZ-edgeOffset(IN.worldPos.x));float2 uv=IN.screenPos.xy/IN.screenPos.w;float2 n=float2(sin(IN.worldPos.x*9+IN.worldPos.z*4-_Time.y*4),cos(IN.worldPos.z*8+_Time.y*3+sin(IN.worldPos.x*4)))*.12;
 float sceneDepth=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD(IN.screenPos)));float depth=max(0,sceneDepth-IN.screenPos.w);float2 distortion=n*.005*saturate(depth*4);float3 refraction=tex2D(_FloodRefraction,uv+distortion).rgb;
 float breakup=fbm(IN.worldPos.xz*32+float2(_Time.y*.9,-_Time.y*2.1));float band=exp(-pow((d-.7)*1.15,2));float foam=band*smoothstep(.49,.73,breakup)*.85;float contact=(1-saturate(depth*20))*saturate(d*5)*smoothstep(.52,.75,breakup)*.18;foam=saturate(foam+contact);
 o.Albedo=lerp(_Color.rgb,float3(.72,.77,.73),foam);o.Emission=refraction*exp(-depth*float3(1.4,.85,1.1))*.55*(1-foam);o.Metallic=0;o.Smoothness=lerp(.94,.32,foam);o.Normal=normalize(float3(n,1));o.Alpha=saturate(.78+depth*.35+foam*.4)*saturate(d*15);}
 ENDCG
 } FallBack "Standard"
}
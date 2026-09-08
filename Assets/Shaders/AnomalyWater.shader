Shader "Station/AnomalyWater"
{
 Properties
 {
  _Color("Water absorption",Color)=(.045,.095,.105,1)
  _FrontZ("Leading edge",Float)=25.2
  _Rise("Water depth",Float)=1.05
  _Crest("Surge crest",Float)=.85
  _FlowAxis("Flow axis",Vector)=(0,1,0,0)
 }
 SubShader
 {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" }
  LOD 300
  Cull Off
  GrabPass { "_FloodRefraction" }
  CGPROGRAM
  #pragma surface surf Standard alpha:premul vertex:vert
  #pragma target 3.0
  #include "UnityCG.cginc"
  fixed4 _Color;
  float _FrontZ,_Rise,_Crest;
  float4 _FlowAxis;
  sampler2D _FloodRefraction;
  UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
  struct Input { float3 worldPos; float4 screenPos; };
  float hash21(float2 p) { p=frac(p*float2(123.34,456.21));p+=dot(p,p+45.32);return frac(p.x*p.y); }
  float noise21(float2 p)
  {
   float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);
   return lerp(lerp(hash21(i),hash21(i+float2(1,0)),f.x),lerp(hash21(i+float2(0,1)),hash21(i+1),f.x),f.y);
  }
  float fbm(float2 p) { return noise21(p)*.57+noise21(p*2.07+13.4)*.28+noise21(p*4.13+5.7)*.15; }
  float across(float2 p) { return dot(p,float2(_FlowAxis.y,-_FlowAxis.x)); }
  float edgeDistance(float2 p)
  {
   float x=across(p);
   float irregular=.17*sin(x*2.4+_Time.y*2.1)+.11*sin(x*5.8-_Time.y*3.3);
   return dot(p,_FlowAxis.xy)-_FrontZ-.32-irregular;
  }
  float heightAt(float2 p)
  {
   float d=max(0,edgeDistance(p)),x=across(p),t=_Time.y;
   float wet=smoothstep(0,2.3,d);
   // A steep aerated face, rolling shoulder, then a lower turbulent wake.
   float shoulder=exp(-pow((d-2.15)/.95,2));
   float broad=sin(x*2.9+t*2.8)*sin(d*3.8-t*4.2)*.08;
   float chop=sin(x*10+d*8-t*9)*.009+sin(x*17-d*13+t*6)*.006;
   float wake=sin(d*4-x*1.7-t*6)*.035+sin(d*7+x*3+t*4)*.02;
   return wet*(_Rise*.58+_Crest*shoulder*(.94+.06*sin(x*3.2+t*3))+broad*shoulder+chop+wake*saturate(d-1));
  }
  void vert(inout appdata_full v)
  {
   float3 p=mul(unity_ObjectToWorld,v.vertex).xyz;
   float h=heightAt(p.xz),e=.025;
   float dx=(heightAt(p.xz+float2(e,0))-heightAt(p.xz-float2(e,0)))/(2*e);
   float dz=(heightAt(p.xz+float2(0,e))-heightAt(p.xz-float2(0,e)))/(2*e);
   p.y+=h;
   v.vertex=mul(unity_WorldToObject,float4(p,1));
   // Recompute lighting normals for the displaced wave, including nonuniform mesh scale.
   float3 n=normalize(float3(-dx,1,-dz));
   v.normal=normalize(mul(n,(float3x3)unity_ObjectToWorld));
   v.tangent.xyz=normalize(mul((float3x3)unity_WorldToObject,float3(1,dx,0)));
   v.tangent.w=-1;
  }
  void surf(Input IN,inout SurfaceOutputStandard o)
  {
   if(_FlowAxis.x>.5)clip(IN.worldPos.x-2.23);
   float2 p=IN.worldPos.xz;
   float d=edgeDistance(p);
   clip(d-.015);
   float t=_Time.y;
   float2 surface=float2(across(p),d+IN.worldPos.y*1.5);
   float2 flow=surface*float2(2.7,3.9)+float2(.15,t*2.2);
   float n1=fbm(flow),n2=fbm(flow*1.83+float2(7,-t*.4));
   float2 micro=float2(n1-.5,n2-.5)*.2;
   float sceneDepth=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture,UNITY_PROJ_COORD(IN.screenPos)));
   float depth=max(0,sceneDepth-IN.screenPos.w);
   float2 uv=IN.screenPos.xy/IN.screenPos.w;
   float3 refracted=tex2D(_FloodRefraction,uv+micro*.018*saturate(depth)).rgb;
   float3 transmission=exp(-min(max(depth,IN.worldPos.y*1.6),5)*float3(.8,.52,.45));
   float crest=exp(-pow((d-1.8)/1.2,2));
   float cells=fbm(surface*17+float2(t*.7,t*3));
   float veins=1-abs(noise21(flow*5)*2-1);
   float foam=saturate(crest*smoothstep(.40,.69,cells)*.85);
   foam=max(foam,smoothstep(.79,.94,veins)*saturate(n1-.47)*1.2);
   foam=max(foam,(1-saturate(depth*5))*smoothstep(.3,.65,cells)*.7);
   o.Albedo=lerp(_Color.rgb*.8,float3(.53,.59,.59),foam);
   o.Emission=refracted*transmission*.45*(1-foam);
   o.Metallic=0;
   o.Smoothness=lerp(.84,.24,foam);
   o.Normal=normalize(float3(micro,1));
   o.Alpha=lerp(.72,1,saturate(d*2+foam));
  }
  ENDCG
 }
 FallBack "Standard"
}

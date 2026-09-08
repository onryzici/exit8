Shader "Station/VentBlood" {
 Properties {_Progress("Flow",Range(0,1))=.2 _Puddle("Puddle",Float)=0 _Color("Blood",Color)=(.16,.006,.009,1)}
 SubShader {
 Tags{"Queue"="Transparent+5" "RenderType"="Transparent"} Cull Off ZWrite Off Offset -1,-1
 Blend SrcAlpha OneMinusSrcAlpha
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float _Progress,_Puddle;fixed4 _Color;
 struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 ambient:TEXCOORD1;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord;o.ambient=ShadeSH9(float4(UnityObjectToWorldNormal(v.normal),1));return o;}
 float randValue(float x){return frac(sin(x*127.1)*43758.54);}
 fixed4 frag(v2f i):SV_Target {
  float2 uv=i.uv;float coverage=0;
  if(_Puddle>.5){
   float2 p=(uv-.5)*2;float angle=atan2(p.y,p.x);
   float edge=.72+.08*sin(angle*7)+.045*sin(angle*13);
   coverage=saturate((edge*sqrt(max(.001,_Progress))-length(p))*140);
  }else{
   float channel=floor(uv.x*17);float offset=frac(uv.x*17)-.5;
   offset+=sin(uv.y*23+channel*5)*.08;
   float width=lerp(.065,.32,randValue(channel+3));
   float reach=_Progress*lerp(.30,1.15,randValue(channel+1));
   coverage=saturate((width-abs(offset))*50)*saturate((reach-(1-uv.y))*120);
   coverage=max(coverage,step(1-(.012+.025*randValue(channel+8)),uv.y)*step(abs(uv.x-.5),.46)*saturate(_Progress*4));
  }
  clip(coverage-.01);
  float wet=sin(uv.y*65-_Time.y*3+uv.x*12)*.5+.5;
  float3 color=_Color.rgb*lerp(.65,1.15,wet)*max(i.ambient,float3(.4,.4,.4));
  color+=pow(wet,24)*.015;
  return fixed4(color,coverage*.97);
 }
 ENDCG
 }
 } FallBack Off
}

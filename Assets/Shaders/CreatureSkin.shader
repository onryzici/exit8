Shader "Station/CreatureSkin" {
 Properties { _Color("Skin",Color)=(.38,.34,.29,1) }
 SubShader { Tags{"RenderType"="Opaque"}
 CGPROGRAM
 #pragma surface surf Standard
 #pragma target 3.0
 fixed4 _Color;
 struct Input {float3 worldPos;};
 float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
 void surf(Input IN,inout SurfaceOutputStandard o){
  float3 p=IN.worldPos;
  float mottling=sin(p.x*21+sin(p.y*13))*sin(p.z*29+sin(p.y*17));
  float veins=pow(saturate(1-abs(sin(p.y*37+p.x*19+sin(p.z*42)*2))),18);
  float pores=hash(floor(p*550));
  o.Albedo=_Color.rgb*lerp(.7,1.12,mottling*.5+.5);
  o.Albedo=lerp(o.Albedo,float3(.13,.085,.09),veins*.20);
  o.Normal=normalize(float3((pores-.5)*.08,(hash(floor(p*470)+7)-.5)*.08,1));
  o.Smoothness=.29+pores*.08;o.Metallic=0;
 }
 ENDCG
 } FallBack "Standard"
}

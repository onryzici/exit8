Shader "Exit7/Ceramic" {
Properties { _Color("Ceramic",Color)=(.73,.76,.73,1) _Size("Tile size",Float)=.3 _Gloss("Glaze",Range(0,1))=.55 }
SubShader { Tags {"RenderType"="Opaque"} LOD 300
CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
struct Input { float3 worldPos; float3 worldNormal; };
fixed4 _Color; float _Size; half _Gloss;
float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
void surf(Input IN,inout SurfaceOutputStandard o) {
float3 n=abs(IN.worldNormal); float2 p=n.y>.6?IN.worldPos.xz:(n.x>.6?IN.worldPos.zy:IN.worldPos.xy);
float2 uv=p/_Size; float2 f=frac(uv); float2 edge=min(f,1-f)*_Size;
float d=min(edge.x,edge.y); float grout=1-smoothstep(.0015,.004,d);
float grain=hash(floor(p*1800)); float tile=hash(floor(uv));
float dirt=pow(1-saturate(d/.035),3)*.12;
o.Albedo=lerp(_Color.rgb*(.94+tile*.09)+(grain-.5)*.035-dirt,float3(.19,.205,.20),grout);
o.Smoothness=lerp(_Gloss+(grain-.5)*.12,.12,grout); o.Metallic=0;
o.Occlusion=lerp(1,.52,grout);
}
ENDCG
} FallBack "Standard" }

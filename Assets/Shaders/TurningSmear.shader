Shader "Hidden/Station/TurningSmear" {
 Properties {_MainTex ("Source",2D)="white"{}}
 SubShader {Cull Off ZWrite Off ZTest Always Pass {
 CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _Smear;
 half4 frag(v2f_img i):SV_Target {
  half4 c=0;float total=0;
  [unroll]for(int j=-6;j<=6;j++){float w=7-abs(j);c+=tex2D(_MainTex,saturate(i.uv+_Smear.xy*j/6.0))*w;total+=w;}
  return c/total;
 }
 ENDCG
 }}
}

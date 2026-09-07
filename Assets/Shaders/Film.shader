Shader "Exit7/Film" { Properties { _MainTex("Image",2D)="white"{} } SubShader { Cull Off ZWrite Off ZTest Always Pass { CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"
sampler2D _MainTex;
fixed4 frag(v2f_img i):SV_Target { float2 p=i.uv-.5; float3 c=tex2D(_MainTex,i.uv).rgb; c=(c-.5)*1.04+.5; c*=1-dot(p,p)*.30; float noise=frac(sin(dot(i.uv+_Time.x,float2(12.9898,78.233)))*43758.5453)-.5; return float4(c+noise*.009,1); }
ENDCG } } }

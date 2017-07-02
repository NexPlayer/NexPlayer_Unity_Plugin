// Based on Unlit shader, but culls the front faces instead of the back

Shader "InsideVisible-3D-RIGHT" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	Tags { "NexPlayer"="Transparent" }
	Cull front
	LOD 100
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				v.texcoord.x = 1 - v.texcoord.x;
				v.texcoord.y = 1 - v.texcoord.y;				
			
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				i.texcoord.x = ( i.texcoord.x / 2 ) + 0.5;
				float4 col = tex2D(_MainTex, i.texcoord);
				return col;
			}
		ENDCG
	}
}

}
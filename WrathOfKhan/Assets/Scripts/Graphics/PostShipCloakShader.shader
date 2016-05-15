Shader "Hidden/PostShipCloakShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_DistortTex ("_DistortTex", 2D) = "white" {}
		//lol nope _CloakPoints ("CloakPoints0", Float3[])
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				//float2 worldPos : TEXCOORD1;
				float2 tl : TEXCOORD2;
				float2 tr : TEXCOORD3;
				float2 bl : TEXCOORD4;
				float2 br : TEXCOORD5;

				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;

				//using _Object2World doesnt work because we're in an Image Effect.  sigh... 
				float2 camPos = _WorldSpaceCameraPos.xy;
				o.tl = camPos + float2(-960, 540);
				o.tr = camPos + float2(960, 540);
				o.bl = camPos + float2(-960, -540);
				o.br = camPos + float2(960, -540);

				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _DistortTex;
			float3 _CloakFields[6];

			fixed4 frag (v2f i) : SV_Target
			{
				//lerp out our own fucking world pos here (gawwwd)
				float2 worldPos = lerp( lerp(i.tl, i.tr, i.uv.x), lerp(i.bl, i.br, i.uv.x), i.uv.y);

				//sum the field contributions from all players
				float field = 0.0;
				for (int j = 0; j < 6; ++j)
				{
					float pointGradient = 1.0 - min(distance(_CloakFields[j].xy, worldPos) / 76.0, 1.0);
					field += pointGradient * _CloakFields[j].z; //.z gives them control to trn on / off
				}

				const float scale = 1.2; //nice, a big wobble every 2 ship lengths or so
				const float limit = 0.3; //a clamp to keep the big cuts down
				const float strength = 0.1; //distortion power once on

				//float2 offsets = tex2D(_DistortTex, i.uv * scale).rg;
				//fixed4 col = tex2D(_MainTex, i.uv + offsets * strength);
				float distortion = tex2D(_DistortTex, i.uv * scale).r;
				distortion = min(distortion, limit);

				fixed4 col = tex2D(_MainTex, i.uv + distortion * strength * field);

				return col;
			}
			ENDCG
		}
	}
}

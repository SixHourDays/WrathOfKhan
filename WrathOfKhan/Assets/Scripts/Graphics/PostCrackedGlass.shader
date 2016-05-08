Shader "Custom/PostCrackedGlass"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_CrackedGlass ("Cracked Glass", 2D) = "white" {}
		_NoiseScale("Noise Scale", Float) = 0.2
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
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			uniform sampler2D _CrackedGlass;
			uniform float _NoiseScale;
			uint rng_state;

			float hash( float2 p ) 
			{
				float h = dot(p,float2(127.1,311.7));	
				return frac(sin(h)*43758.5453123);
			}

		/*	float noise( float2 p )
			{
				float2 i = floor( p );
				float2 f = frac( p );	
				float2 u = f*f*(3.0-2.0*f);
				
				return -1.0+2.0*lerp( lerp( hash( i + float2(0.0,0.0) ), 
					    hash( i + float2(1.0,0.0) ), u.x),
						lerp( hash( i + float2(0.0,1.0) ), 
						hash( i + float2(1.0,1.0) ), u.x), u.y);
			}*/


			uint rand_xorshift()
			{
				// Xorshift algorithm from George Marsaglia's paper
				rng_state ^= (rng_state << 13);
				rng_state ^= (rng_state >> 17);
				rng_state ^= (rng_state << 5);
				return rng_state;
			}

			uint wang_hash(uint seed)
			{
				seed = (seed ^ 61) ^ (seed >> 16);
				seed *= 9;
				seed = seed ^ (seed >> 4);
				seed *= 0x27d4eb2d;
				seed = seed ^ (seed >> 15);
				return seed;
			}

			void seed( float2 p )
			{
				float2 i = floor( p );
				float2 f = frac( p );	
				float2 u = f*f*(3.0-2.0*f);

				uint seed = (u.x+(u.y*2048.0))*_Time.w;
				rng_state = wang_hash(seed);

				rng_state = rand_xorshift();
				rng_state = rand_xorshift();
			}

			float2 noise()
			{
				float f0 = float(rand_xorshift()) * (1.0 / 4294967296.0);
				float f1 = float(rand_xorshift()) * (1.0 / 4294967296.0);

				float2 n = float2(f0,f1);
				n = 2.0 * (n - 0.5);

				return n;
			}

			sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target
			{
				float3 crackStrength = tex2D(_CrackedGlass, i.uv).rgb;

				seed( 1000.0*i.uv );
				float scale = crackStrength.r*_NoiseScale;
				float2 n0 = 0.75*scale*noise();
				float2 n1 = 0.5*scale*noise();
				float2 n2 = scale*noise();

				float r = tex2D(_MainTex, i.uv + n0).r;
				float g = tex2D(_MainTex, i.uv + n1).g;
				float b = tex2D(_MainTex, i.uv + n2).b;

				float2 n3 = 0.002*_NoiseScale*noise();
				float3 ghost = 0.5 * tex2D(_MainTex, i.uv+n3).rgb;

				float3 n = float3(r,g,b);

				return fixed4(ghost+n,1.0);
				//return fixed4(n3,0,1.0);
			}
			ENDCG
		}
	}
}

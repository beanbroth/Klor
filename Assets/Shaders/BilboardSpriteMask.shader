Shader "Custom/BillboardedSpriteMask"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
	}
		SubShader
		{
			Tags {"Queue" = "Transparent-1" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout"}
			ColorMask 0
			ZWrite Off
			Cull Off

			Stencil
			{
				Ref[_Stencil]
				Comp Always
				Pass Replace
				WriteMask[_StencilWriteMask]
			}

			CGPROGRAM
			#pragma surface surf NoLighting alpha:fade keepalpha noforwardadd nolightmap noambient novertexlights noshadow
			#pragma vertex vert

			sampler2D _MainTex;
			float _Cutoff;

			struct Input
			{
				float2 uv_MainTex;
			};

			void vert(inout appdata_full v)
			{
				float3 worldPos = unity_ObjectToWorld._m03_m13_m23;
				float3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				float3 upDir = float3(0, 1, 0);
				float3 rightDir = normalize(cross(upDir, viewDir));
				upDir = normalize(cross(viewDir, rightDir));

				float3 centerOffset = v.vertex.xyz;
				float3 localPos = rightDir * centerOffset.x + upDir * centerOffset.y;

				v.vertex.xyz = localPos;
			}

			void surf(Input IN, inout SurfaceOutput o)
			{
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
				o.Alpha = c.a;
				clip(o.Alpha - _Cutoff);
			}

			fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
			{
				return fixed4(0,0,0,s.Alpha);
			}
			ENDCG
		}
}
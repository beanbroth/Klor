Shader "Custom/BillboardSprite"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float extractYRotation(float4x4 mat)
            {
                return atan2(mat._m02, mat._m22);
            }

            float extractZRotation(float4x4 mat)
            {
                return atan2(mat._m01, mat._m00);
            }

            v2f vert(appdata v)
            {
                v2f o;
                float3 scale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
                float worldYRotation = extractYRotation(unity_ObjectToWorld);
                float localZRotation = extractZRotation(unity_ObjectToWorld);
                float cosZ = cos(localZRotation);
                float sinZ = sin(localZRotation);
                float3x3 localRotationMatrix = float3x3(
                    cosZ, -sinZ, 0,
                    sinZ, cosZ, 0,
                    0, 0, 1
                );

                float3 worldPos = unity_ObjectToWorld._m03_m13_m23;
                float3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                float3 upDir = float3(0, 1, 0);
                float3 rightDir = normalize(cross(upDir, viewDir));

                worldYRotation = fmod(worldYRotation + 2 * UNITY_PI, 2 * UNITY_PI);
                if (worldYRotation > UNITY_PI / 2 && worldYRotation < 3 * UNITY_PI / 2)
                {
                    rightDir = -rightDir;
                }
                upDir = normalize(cross(viewDir, rightDir));

                float3 vertexOffset = mul(localRotationMatrix, v.vertex.xyz * scale);
                float3 worldOffset = rightDir * vertexOffset.x + upDir * vertexOffset.y + viewDir * vertexOffset.z;
                float3 billboardPos = worldPos + worldOffset;

                o.vertex = UnityWorldToClipPos(billboardPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
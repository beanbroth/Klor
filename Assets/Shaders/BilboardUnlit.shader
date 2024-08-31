Shader "Custom/BillboardSpriteWithMultipleDamage"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _DamageTex("Damage Texture", 2D) = "white" {}
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

            struct DamageInfo
            {
                float4 positionAndType; // xy = position, z = damage type, w = rotation
                float scale;
            };

            sampler2D _MainTex;
            sampler2D _DamageTex;
            float4 _MainTex_ST;

            // Constants for three damage instances
            static const DamageInfo DAMAGE_INSTANCES[3] = {
                { float4(0.3, 0.3, 0, 0), 0.15 },  // Bottom-left
                { float4(0.7, 0.7, 0, 0.785), 0.2 },  // Top-right, rotated 45 degrees
                { float4(0.5, 0.5, 0, 1.57), 0.25 }   // Center, rotated 90 degrees
            };

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

            float2 rotateUV(float2 uv, float rotation)
            {
                float s = sin(rotation);
                float c = cos(rotation);
                float2 pivot = float2(0.5, 0.5);
                return float2(
                    c * (uv.x - pivot.x) - s * (uv.y - pivot.y) + pivot.x,
                    s * (uv.x - pivot.x) + c * (uv.y - pivot.y) + pivot.y
                );
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

            // Apply damage for each instance
            for (int j = 0; j < 3; j++)
            {
                DamageInfo damage = DAMAGE_INSTANCES[j];
                float2 damagePos = damage.positionAndType.xy;
                float damageRotation = damage.positionAndType.w;
                float damageScale = damage.scale;

                float2 damageUV = (i.uv - damagePos) / damageScale;
                damageUV = rotateUV(damageUV, damageRotation);

                if (all(damageUV >= 0 && damageUV <= 1))
                {
                    fixed4 damageTexture = tex2D(_DamageTex, damageUV);
                    col.rgb = lerp(col.rgb, damageTexture.rgb, damageTexture.a);
                    col.a = lerp(col.a, 0, damageTexture.a); // Reduce alpha where damage is applied
                }
            }

            return col;
        }
        ENDCG
    }
    }
}
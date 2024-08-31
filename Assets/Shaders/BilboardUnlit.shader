Shader "Custom/BillboardSpriteWithMultipleDamageTypesAndFlash"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _DamageTex1("Damage Texture 1", 2D) = "white" {}
        _DamageTex2("Damage Texture 2", 2D) = "white" {}
        _DamageTex3("Damage Texture 3", 2D) = "white" {}
        _EnemyID("Enemy ID", Float) = 0
        _FlashColor("Flash Color", Color) = (1,0,0,1)
        _FlashIntensity("Flash Intensity", Range(0,1)) = 0
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
                    float damage;
                };

                StructuredBuffer<DamageInfo> _DamageBuffer;
                int _MaxDamageInstancesPerEnemy;

                sampler2D _MainTex;
                sampler2D _DamageTex1;
                sampler2D _DamageTex2;
                sampler2D _DamageTex3;
                float4 _MainTex_ST;
                float _EnemyID;
                float4 _FlashColor;
                float _FlashIntensity;

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
                    float finalAlpha = col.a;

                    int startIndex = _EnemyID * _MaxDamageInstancesPerEnemy;
                    for (int j = 0; j < _MaxDamageInstancesPerEnemy; j++)
                    {
                        DamageInfo damage = _DamageBuffer[startIndex + j];
                        if (damage.damage > 0)
                        {
                            float2 damagePos = damage.positionAndType.xy;
                            float damageType = damage.positionAndType.z;
                            float damageRotation = damage.positionAndType.w;
                            float damageScale = damage.scale;

                            float2 damageUV = (i.uv - damagePos) / damageScale;
                            damageUV = rotateUV(damageUV, damageRotation);

                            if (all(damageUV >= 0 && damageUV <= 1))
                            {
                                float damageAlpha;
                                if (damageType == 0) damageAlpha = tex2D(_DamageTex1, damageUV).a;
                                else if (damageType == 1) damageAlpha = tex2D(_DamageTex2, damageUV).a;
                                else damageAlpha = tex2D(_DamageTex3, damageUV).a;

                                // Create a complete hole where damage is applied
                                finalAlpha = min(finalAlpha, 1 - damageAlpha);
                            }
                        }
                    }

                    // Apply flash effect
                    fixed3 flashedColor = lerp(col.rgb, _FlashColor.rgb, _FlashIntensity);

                    return fixed4(flashedColor, finalAlpha);
                }
                ENDCG
            }
        }
}
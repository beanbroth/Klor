Shader "Custom/BillboardSpriteWithMultipleDamageTypesAndFlash"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _DamageTex1("Damage Texture 1", 2D) = "white" {}
        _DamageTex2("Damage Texture 2", 2D) = "white" {}
        _DamageTex3("Damage Texture 3", 2D) = "white" {}
        _FlashColor("Flash Color", Color) = (1,0,0,1)
    }
        SubShader
        {
            Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline"}

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            Pass
            {
                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_instancing
                #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                struct Attributes
                {
                    float4 positionOS : POSITION;
                    float2 uv : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct Varyings
                {
                    float2 uv : TEXCOORD0;
                    float4 positionCS : SV_POSITION;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                TEXTURE2D(_MainTex);
                TEXTURE2D(_DamageTex1);
                TEXTURE2D(_DamageTex2);
                TEXTURE2D(_DamageTex3);
                SAMPLER(sampler_MainTex);

                CBUFFER_START(UnityPerMaterial)
                    float4 _MainTex_ST;
                    float4 _FlashColor;
                CBUFFER_END

                #define MAX_DAMAGE_INSTANCES 3

                UNITY_INSTANCING_BUFFER_START(Props)
                    UNITY_DEFINE_INSTANCED_PROP(float, _FlashIntensity)
                    UNITY_DEFINE_INSTANCED_PROP(float4, _DamagePositionsAndTypes[MAX_DAMAGE_INSTANCES])
                    UNITY_DEFINE_INSTANCED_PROP(float2, _DamageScalesAndRotations[MAX_DAMAGE_INSTANCES])
                UNITY_INSTANCING_BUFFER_END(Props)

                void ConfigureProcedural()
                {
                    // This function is needed for the instancing_options procedural
                    // It's empty because we're not doing any procedural generation
                }

              
                float extractYRotation(float4x4 mat)
                {
                    return atan2(mat._m02, mat._m22);
                }

                float extractZRotation(float4x4 mat)
                {
                    return atan2(mat._m01, mat._m00);
                }

                Varyings vert(Attributes input)
                {
                    Varyings output;
                    UNITY_SETUP_INSTANCE_ID(input);
                    UNITY_TRANSFER_INSTANCE_ID(input, output);

                    float4x4 objectToWorld = GetObjectToWorldMatrix();
                    float3 scale = float3(
                        length(objectToWorld._m00_m10_m20),
                        length(objectToWorld._m01_m11_m21),
                        length(objectToWorld._m02_m12_m22)
                    );
                    float worldYRotation = extractYRotation(objectToWorld);
                    float localZRotation = extractZRotation(objectToWorld);
                    float cosZ = cos(localZRotation);
                    float sinZ = sin(localZRotation);
                    float3x3 localRotationMatrix = float3x3(
                        cosZ, -sinZ, 0,
                        sinZ, cosZ, 0,
                        0, 0, 1
                    );

                    float3 worldPos = objectToWorld._m03_m13_m23;
                    float3 viewDir = normalize(GetCameraPositionWS() - worldPos);
                    float3 upDir = float3(0, 1, 0);
                    float3 rightDir = normalize(cross(upDir, viewDir));

                    worldYRotation = fmod(worldYRotation + 2 * PI, 2 * PI);
                    if (worldYRotation > PI / 2 && worldYRotation < 3 * PI / 2)
                    {
                        rightDir = -rightDir;
                    }
                    upDir = normalize(cross(viewDir, rightDir));

                    float3 vertexOffset = mul(localRotationMatrix, input.positionOS.xyz * scale);
                    float3 worldOffset = rightDir * vertexOffset.x + upDir * vertexOffset.y + viewDir * vertexOffset.z;
                    float3 billboardPos = worldPos + worldOffset;

                    output.positionCS = mul(GetWorldToHClipMatrix(), float4(billboardPos, 1.0));
                    output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                    return output;
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

                half4 frag(Varyings input) : SV_Target
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                    float finalAlpha = col.a;

                    for (int j = 0; j < MAX_DAMAGE_INSTANCES; j++)
                    {
                        float4 damagePositionAndType = UNITY_ACCESS_INSTANCED_PROP(Props, _DamagePositionsAndTypes[j]);
                        float2 damageScaleAndRotation = UNITY_ACCESS_INSTANCED_PROP(Props, _DamageScalesAndRotations[j]);

                        float2 damagePos = damagePositionAndType.xy;
                        float damageType = damagePositionAndType.z;
                        float damageActive = damagePositionAndType.w;
                        float damageRotation = damageScaleAndRotation.y;
                        float damageScale = damageScaleAndRotation.x;

                        if (damageActive > 0)
                        {
                            float2 damageUV = (input.uv - damagePos) / damageScale;
                            damageUV = rotateUV(damageUV, damageRotation);

                            if (all(damageUV >= 0 && damageUV <= 1))
                            {
                                float damageAlpha;
                                if (damageType == 0) damageAlpha = SAMPLE_TEXTURE2D(_DamageTex1, sampler_MainTex, damageUV).a;
                                else if (damageType == 1) damageAlpha = SAMPLE_TEXTURE2D(_DamageTex2, sampler_MainTex, damageUV).a;
                                else damageAlpha = SAMPLE_TEXTURE2D(_DamageTex3, sampler_MainTex, damageUV).a;

                                finalAlpha = min(finalAlpha, 1 - damageAlpha);
                            }
                        }
                    }

                    float flashIntensity = UNITY_ACCESS_INSTANCED_PROP(Props, _FlashIntensity);
                    half3 flashedColor = lerp(col.rgb, _FlashColor.rgb, flashIntensity);

                    return half4(flashedColor, finalAlpha);
                }
                ENDHLSL
            }
        }
}
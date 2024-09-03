Shader "Custom/3LayeredBillboardSpriteWithMultipleDamageTypesAndFlash_Instanced"
{
    Properties
    {
        _MainTex ("Top Layer Texture", 2D) = "white" {}
        _MidLayerTex ("Middle Layer Texture", 2D) = "white" {}
        _BottomLayerTex ("Bottom Layer Texture", 2D) = "white" {}
        _DamageTex1 ("Damage Texture 1", 2D) = "white" {}
        _DamageTex2 ("Damage Texture 2", 2D) = "white" {}
        _DamageTex3 ("Damage Texture 3", 2D) = "white" {}
        _FlashColor ("Flash Color", Color) = (0,0,0,0)
        //decrese bullet effects per layer, so gross layers underneath show.
        _DamageLayerScale ("Damage Layer Scale", Vector) = (1, 0.9, 0.8, 0)
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"
        }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog  // Add fog support
            #include "UnityCG.cginc"

            // Input structure for vertex shader
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Output structure for vertex shader / input for fragment shader
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)  //  fog support
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct DamageInfo
            {
                float4 positionAndType; // xy = position, z = damage type, w = rotation
                float scale;
                float damage;
            };

            // Damage information buffer
            StructuredBuffer<DamageInfo> _DamageBuffer;
            int _MaxDamageInstancesPerEnemy;

            // Textures and related properties
            sampler2D _MainTex;
            sampler2D _MidLayerTex;
            sampler2D _BottomLayerTex;
            sampler2D _DamageTex1;
            sampler2D _DamageTex2;
            sampler2D _DamageTex3;
            float4 _MainTex_ST;
            float4 _DamageLayerScale;

            // Instance properties
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _EnemyID)
                UNITY_DEFINE_INSTANCED_PROP(float, _FlashIntensity)
            UNITY_INSTANCING_BUFFER_END(Props)

            float4 _FlashColor;
            
            float extractYRotation(float4x4 mat)
            {
                return atan2(mat._m02, mat._m22);
            }
            
            float extractZRotation(float4x4 mat)
            {
                return atan2(mat._m01, mat._m00);
            }

            // Vertex 
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                // Extract scale and rotations from object-to-world matrix
                float3 scale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );
                float worldYRotation = extractYRotation(unity_ObjectToWorld);
                float localZRotation = extractZRotation(unity_ObjectToWorld);

                // Create local rotation matrix
                float cosZ = cos(localZRotation);
                float sinZ = sin(localZRotation);
                float3x3 localRotationMatrix = float3x3(
                    cosZ, -sinZ, 0,
                    sinZ, cosZ, 0,
                    0, 0, 1
                );

                // Calculate billboard orientation
                float3 worldPos = unity_ObjectToWorld._m03_m13_m23;
                float3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                float3 upDir = float3(0, 1, 0);
                float3 rightDir = normalize(cross(upDir, viewDir));

                // Adjust billboard orientation to reverse funky Y rotation effects. This sucks.
                worldYRotation = fmod(worldYRotation + 2 * UNITY_PI, 2 * UNITY_PI);
                if (worldYRotation > UNITY_PI / 2 && worldYRotation < 3 * UNITY_PI / 2)
                {
                    rightDir = -rightDir;
                }
                upDir = normalize(cross(viewDir, rightDir));

                // Calculate final vertex position
                float3 vertexOffset = mul(localRotationMatrix, v.vertex.xyz * scale);
                float3 worldOffset = rightDir * vertexOffset.x + upDir * vertexOffset.y + viewDir * vertexOffset.z;
                float3 billboardPos = worldPos + worldOffset;

                o.vertex = UnityWorldToClipPos(billboardPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);  // Calculate fog factor
                return o;
            }

            // Rotate UV coordinates
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

            // Fragment
            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                
                // Sample textures for each flesh layer
                fixed4 topLayer = tex2D(_MainTex, i.uv);
                fixed4 midLayer = tex2D(_MidLayerTex, i.uv);
                fixed4 bottomLayer = tex2D(_BottomLayerTex, i.uv);

                float3 layerAlpha = float3(topLayer.a, midLayer.a, bottomLayer.a);

                // Process damage instances frim buffer
                float enemyID = UNITY_ACCESS_INSTANCED_PROP(Props, _EnemyID);
                int startIndex = enemyID * _MaxDamageInstancesPerEnemy;
                for (int j = 0; j < _MaxDamageInstancesPerEnemy; j++)
                {
                    DamageInfo damage = _DamageBuffer[startIndex + j];

                    float2 damagePos = damage.positionAndType.xy;
                    float damageType = damage.positionAndType.z;
                    float damageRotation = damage.positionAndType.w;
                    float damageScale = damage.scale;

                    // Apply damage to each layer
                    for (int layer = 0; layer < 3; layer++)
                    {
                        float layerDamageScale = damageScale * _DamageLayerScale[layer];
                        float2 damageUV = (i.uv - damagePos) / layerDamageScale;
                        damageUV = rotateUV(damageUV, damageRotation);

                        if (all(damageUV >= 0 && damageUV <= 1))
                        {
                            float damageAlpha;
                            if (damageType == 0) damageAlpha = tex2D(_DamageTex1, damageUV).a;
                            else if (damageType == 1) damageAlpha = tex2D(_DamageTex2, damageUV).a;
                            else damageAlpha = tex2D(_DamageTex3, damageUV).a;

                            // Create a hole where damage is applied, with decreasing effect on lower layers
                            layerAlpha[layer] = min(layerAlpha[layer], 1 - damageAlpha);
                        }
                    }
                }

                // combine layers
                fixed3 finalColor = bottomLayer.rgb;
                finalColor = lerp(finalColor, midLayer.rgb, layerAlpha[1]);
                finalColor = lerp(finalColor, topLayer.rgb, layerAlpha[0]);

                float finalAlpha = max(max(layerAlpha[0], layerAlpha[1]), layerAlpha[2]);

                // Apply flash effect
                float flashIntensity = UNITY_ACCESS_INSTANCED_PROP(Props, _FlashIntensity);
                fixed3 flashedColor = lerp(finalColor, _FlashColor.rgb, flashIntensity);

                fixed4 col = fixed4(flashedColor, finalAlpha);
                
                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
}
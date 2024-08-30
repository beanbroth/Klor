Shader "Custom/BillboardWithUprightSprite"
{
    Properties
    {
       _MainTex("Texture Image", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off // Disable culling to render both sides
        Pass
        {
            CGPROGRAM
            #pragma vertex vert  
            #pragma fragment frag
            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;

            struct vertexInput
            {
                float4 vertex : POSITION;
                float4 tex : TEXCOORD0;
            };
            struct vertexOutput
            {
                float4 pos : SV_POSITION;
                float4 tex : TEXCOORD0;
            };

            float extractYRotation(float4x4 mat)
            {
                // Extract Y rotation from the matrix
                return atan2(mat._m02, mat._m22);
            }

            float extractZRotation(float4x4 mat)
            {
                // Extract only the Z rotation, ignoring X and Y
                return atan2(mat._m01, mat._m00);
            }

            vertexOutput vert(vertexInput input)
            {
                vertexOutput output;

                // Extract local scale from object-to-world matrix
                float3 scale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );

                // Extract Y rotation
                float worldYRotation = extractYRotation(unity_ObjectToWorld);

                // Extract only Z rotation
                float localZRotation = extractZRotation(unity_ObjectToWorld);

                // Create rotation matrix for local Z rotation only
                float cosZ = cos(localZRotation);
                float sinZ = sin(localZRotation);
                float3x3 localRotationMatrix = float3x3(
                    cosZ, -sinZ, 0,
                    sinZ, cosZ, 0,
                    0, 0, 1
                );

                // Billboard calculation
                float3 worldPos = unity_ObjectToWorld._m03_m13_m23;
                float3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                float3 upDir = float3(0, 1, 0);
                float3 rightDir = normalize(cross(upDir, viewDir));

                // Normalize the Y rotation to be between 0 and 2π
                worldYRotation = fmod(worldYRotation + 2 * UNITY_PI, 2 * UNITY_PI);

                // Check if the world Y rotation is between 90 and 270 degrees
                if (worldYRotation > UNITY_PI / 2 && worldYRotation < 3 * UNITY_PI / 2)
                {
                    // Flip the right direction
                    rightDir = -rightDir;
                }

                upDir = normalize(cross(viewDir, rightDir));

                // Apply scale and Z rotation to local vertex position
                float3 vertexOffset = mul(localRotationMatrix, input.vertex.xyz * scale);

                // Apply billboard orientation
                float3 worldOffset = rightDir * vertexOffset.x + upDir * vertexOffset.y + viewDir * vertexOffset.z;

                float3 billboardPos = worldPos + worldOffset;
                output.pos = UnityWorldToClipPos(billboardPos);
                output.tex = input.tex;

                // Remove the UV flipping
                // if (worldYRotation > UNITY_PI / 2 && worldYRotation < 3 * UNITY_PI / 2)
                // {
                //     output.tex.x = 1 - output.tex.x;
                // }

                return output;
            }

            float4 frag(vertexOutput input) : COLOR
            {
                return tex2D(_MainTex, float2(input.tex.xy));
            }
            ENDCG
        }
    }
}
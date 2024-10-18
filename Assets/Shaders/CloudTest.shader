Shader "Custom/CloudTest"
{
    Properties
    {
        _CloudCube ("Cloud Cubemap", CUBE) = "white" {}
        _RotationAngle("Rotation Angle", Float) = 0.0
        _Color ("Base Color", Color) = (1,1,1,1)
        _DuskColor ("Dusk Color", Color) = (1,0.5,0,1)
        _NightColor ("Night Color", Color) = (0,0,0,1)
        _LightDirection("Light Direction", Vector) = (0, 1, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 lightDir : TEXCOORD1;
            };

            samplerCUBE _CloudCube;
            float _RotationAngle;
            float4 _Color;
            float4 _DuskColor;
            float4 _NightColor;
            float4 _LightDirection;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                
                // Calculate world light direction
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.lightDir = normalize(_WorldSpaceLightPos0.xyz - worldPos);
                
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float cosGreen = cos(_RotationAngle);
                float sinGreen = sin(_RotationAngle);
                float cosBlue = cos(-0.5 * _RotationAngle);
                float sinBlue = sin(-0.5 * _RotationAngle);

                fixed4 color = texCUBE(_CloudCube, i.worldNormal);
                fixed colorR = color.r;
                fixed colorG = texCUBE(_CloudCube, float3(
                    cosGreen * i.worldNormal.x + sinGreen * i.worldNormal.z,
                    i.worldNormal.y,
                    -sinGreen * i.worldNormal.x + cosGreen * i.worldNormal.z
                )).g;
                fixed colorB = texCUBE(_CloudCube, float3(
                    cosBlue * i.worldNormal.x + sinBlue * i.worldNormal.z,
                    i.worldNormal.y,
                    -sinBlue * i.worldNormal.x + cosBlue * i.worldNormal.z
                )).b;

                fixed cloudIntensity = saturate(1000 * colorR * colorG * colorB);
                
                // Compute the dot product between the normal and light direction
                float dotProduct = dot(i.worldNormal, normalize(_LightDirection.xyz));
                float4 finalColor = lerp(_NightColor, _Color, saturate(dotProduct));
                finalColor = lerp(_DuskColor, finalColor, saturate(0.9 + abs(dotProduct)));
                
                return fixed4(finalColor.rgb, cloudIntensity);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
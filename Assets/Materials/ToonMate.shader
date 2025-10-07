Shader "Custom/ToonSimple"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _ShadowSteps("Shadow Steps", Range(1,5)) = 3
        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimPower("Rim Power", Range(0.5,8)) = 3

        _LightPos("Light Position", Vector) = (0,1,0,1)
        _LightColor("Light Color", Color) = (1,1,1,1)
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
            LOD 200

            ZWrite On

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normalDir : TEXCOORD1;
                    float3 viewDir : TEXCOORD2;
                    float3 worldPos : TEXCOORD3;
                };

                sampler2D _MainTex;
                float4 _BaseColor;
                int _ShadowSteps;
                float4 _RimColor;
                float _RimPower;

                float4 _LightPos;
                float4 _LightColor;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    o.normalDir = normalize(UnityObjectToWorldNormal(v.normal));
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Luz desde posición custom
                    float3 lightDir = normalize(_LightPos.xyz - i.worldPos);
                    float NdotL = saturate(dot(i.normalDir, lightDir));

                    // Toon shading con steps
                    float stepShade = ceil(NdotL * _ShadowSteps) / _ShadowSteps;
                    stepShade = saturate(stepShade);

                    // Base
                    fixed4 tex = tex2D(_MainTex, i.uv) * _BaseColor;
                    tex.rgb *= stepShade * _LightColor.rgb;

                    // Rim light
                    float rim = pow(1.0 - max(0, dot(i.viewDir, i.normalDir)), _RimPower);
                    tex.rgb += _RimColor.rgb * rim;

                    tex.a = 1.0;
                    return tex;
                }
                ENDCG
            }
        }
}

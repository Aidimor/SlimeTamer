Shader "Custom/ToonTransparentSlimeSmooth"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Transparency("Transparency", Range(0,1)) = 0
        _ShadowSteps("Shadow Steps", Range(1,5)) = 3
        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimPower("Rim Power", Range(0.5,8)) = 3
    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
            LOD 200

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

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
                };

                sampler2D _MainTex;
                float4 _Color;
                float _Transparency; // 0 = fully visible, 1 = fully invisible
                int _ShadowSteps;
                float4 _RimColor;
                float _RimPower;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    o.normalDir = normalize(UnityObjectToWorldNormal(v.normal));
                    float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                    float NdotL = dot(i.normalDir, lightDir);

                    // Toon shading
                    float stepShade = ceil(NdotL * _ShadowSteps) / _ShadowSteps;
                    stepShade = saturate(stepShade);

                    // Sample texture
                    fixed4 tex = tex2D(_MainTex, i.uv) * _Color;

                    // Bright toon effect
                    tex.rgb *= stepShade * 1.5;

                    // Rim light
                    float rim = pow(1.0 - max(0, dot(i.viewDir, i.normalDir)), _RimPower);
                    tex.rgb += _RimColor.rgb * rim;

                    // Smooth transparency control
                    tex.a *= 1.0 - _Transparency; // 0 = fully visible, 1 = fully invisible

                    return tex;
                }
                ENDCG
            }
        }
}

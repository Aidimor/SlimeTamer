Shader "Custom/ToonTripleFillGlass"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _FillColorA("Fill Color A", Color) = (0,1,0,0.8)
        _FillColorB("Fill Color B", Color) = (1,0,0,0.8)
        _FillAmount("Fill Amount", Range(0,1)) = 0.5
        _FillEdge("Fill Edge Smoothness", Range(0,0.2)) = 0.05
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
                float4 _BaseColor;
                float4 _FillColorA;
                float4 _FillColorB;
                float _FillAmount;
                float _FillEdge;
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

                    // Base texture
                    fixed4 tex = tex2D(_MainTex, i.uv) * _BaseColor;

                    // --- FILL COLORS ---
                    fixed4 fill = tex;

                    // Fill Color A (0 - 0.5)
                    float tA = smoothstep(0.0, 0.5, i.uv.y) * smoothstep(0.0, _FillEdge, _FillAmount - i.uv.y);
                    tA = saturate(tA * (_FillAmount <= 0.5 ? _FillAmount / 0.5 : 1.0));
                    fill.rgb = lerp(fill.rgb, _FillColorA.rgb, tA * _FillColorA.a);
                    fill.a = lerp(fill.a, _FillColorA.a, tA);

                    // Fill Color B (0.5 - 1)
                    if (_FillAmount > 0.5)
                    {
                        float tB = smoothstep(0.5, 1.0, i.uv.y) * smoothstep(0.0, _FillEdge, _FillAmount - 0.5);
                        tB = saturate(tB * ((_FillAmount - 0.5) / 0.5));
                        fill.rgb = lerp(fill.rgb, _FillColorB.rgb, tB * _FillColorB.a);
                        fill.a = lerp(fill.a, _FillColorB.a, tB);
                    }

                    // Toon brightness
                    fill.rgb *= stepShade * 1.5;

                    // Rim light
                    float rim = pow(1.0 - max(0, dot(i.viewDir, i.normalDir)), _RimPower);
                    fill.rgb += _RimColor.rgb * rim;

                    return fill;
                }
                ENDCG
            }
        }
}

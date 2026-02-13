Shader "Custom/ToonGlassSimple"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _ShadowSteps("Shadow Steps", Range(1,5)) = 3

        _LightPos("Light Position", Vector) = (0,1,0,1)
        _LightColor("Light Color", Color) = (1,1,1,1)
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
                    float3 worldPos : TEXCOORD2;
                };

                sampler2D _MainTex;
                float4 _BaseColor;
                int _ShadowSteps;

                float4 _LightPos;
                float4 _LightColor;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    o.normalDir = normalize(UnityObjectToWorldNormal(v.normal));
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Dirección de luz custom
                    float3 lightDir = normalize(_LightPos.xyz - i.worldPos);
                    float NdotL = saturate(dot(i.normalDir, lightDir));

                    // Toon shading
                    float stepShade = ceil(NdotL * _ShadowSteps) / _ShadowSteps;
                    stepShade = saturate(stepShade);

                    // Base texture
                    fixed4 col = tex2D(_MainTex, i.uv) * _BaseColor;

                    // Aplicar iluminación toon
                    col.rgb *= stepShade * _LightColor.rgb;

                    return col;
                }
                ENDCG
            }
        }
}

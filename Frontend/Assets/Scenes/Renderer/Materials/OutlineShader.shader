Shader "Custom/RT_Outline"
{
    Properties
    {
        _MainTex ("Render Texture", 2D) = "white" {}
        _OutlineThickness ("Outline Thickness (px)", Float) = 1
    }

    SubShader
    {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Tags { "LightMode"="UniversalForward" }

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            // URP includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            float4 _EdgeColor;
            float _OutlineThickness;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag (Varyings i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // Draw normal pixels (alpha > threshold)
                if (col.a > 0.01)
                    return col;

                // Outline sampling offsets
                float2 px = _OutlineThickness * _MainTex_TexelSize.xy;

                float alphaN =
                      SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(px.x, 0)).a
                    + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-px.x, 0)).a
                    + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, px.y)).a
                    + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0,-px.y)).a;

                // If any neighbour is opaque → outline
                if (alphaN > 0.01)
                    return _EdgeColor;

                // Otherwise transparent
                return float4(0,0,0,0);
            }

            ENDHLSL
        }
    }
}

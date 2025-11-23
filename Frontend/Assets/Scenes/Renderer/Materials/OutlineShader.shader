Shader "Custom/RT_Outline"
{
    Properties
    {
        _MainTex ("Render Texture", 2D) = "white" {}
        _OutlineThickness ("Outline Thickness (px)", Float) = 1

        // --- Ripple effect properties ---
        _RippleCenter ("Ripple Center (0-1)", Vector) = (0.5, 0.5, 0, 0)
        _RippleTime ("Ripple Time (0-1)", Float) = 0
        _RippleStrength ("Ripple Strength", Float) = 0.03
        _RippleRadius ("Ripple Radius", Float) = 0.25
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

            // --- Ripple uniforms ---
            float2 _RippleCenter;
            float _RippleTime;
            float _RippleStrength;
            float _RippleRadius;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag (Varyings i) : SV_Target
            {
                float2 uv = i.uv;

                // ====================================================
                //    EXPLOSIVE RIPPLE DISTORTION
                // ====================================================
                if (_RippleTime > 0.001)
                {
                    float dist = distance(uv, _RippleCenter);

                    // Explosion wave - starts strong, fades quickly
                    float explosionRadius = (1.0 - _RippleTime) * _RippleRadius * 3.0; // Expands outward as _RippleTime goes 1->0
                    float wave = (explosionRadius - dist) * 20.0; // Higher frequency for sharper effect

                    // Multiple ripples for explosion effect
                    float ripple = sin(wave) * sin(wave * 0.3) * _RippleStrength;

                    // Sharp falloff - explosion effect concentrated near blast center
                    float explosionFade = 1.0 - smoothstep(0.0, explosionRadius * 1.5, dist);
                    float timeFade = _RippleTime; // Fades as time goes 1->0

                    uv += normalize(uv - _RippleCenter) * ripple * explosionFade * timeFade;
                }

                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // Normal alpha → return original pixel
                if (col.a > 0.01)
                    return col;

                // Outline sampling offsets
                float2 px = _OutlineThickness * _MainTex_TexelSize.xy;

                float alphaN =
                      SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(px.x, 0)).a
                    + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-px.x, 0)).a
                    + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, px.y)).a
                    + SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0,-px.y)).a;

                if (alphaN > 0.01)
                    return _EdgeColor;

                return float4(0,0,0,0);
            }

            ENDHLSL
        }
    }
}

Shader "Custom/EdgeDetection"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
        _Threshold ("Threshold", Range(0, 1)) = 0.01
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
        }
        
        Pass
        {
            Name "EdgeDetection"
            
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha  // ← Critical for transparency
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            
            half4 _EdgeColor;
            float _Threshold;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }
            
            bool ColorsMatch(half4 a, half4 b, float threshold)
            {
                half diff = abs(a.r - b.r) + abs(a.g - b.g) + abs(a.b - b.b);
                return diff < threshold;
            }
            
            half Luminance(half3 color)
            {
                return dot(color, half3(0.299, 0.587, 0.114));
            }
            
            half4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 texelSize = _MainTex_TexelSize.xy;
                
                half4 center = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                
                // Skip transparent pixels (no walls here)
                if (center.a < 0.01)
                    return center;
                
                half4 top    = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( 0, -1) * texelSize);
                half4 left   = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-1,  0) * texelSize);
                half4 right  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( 1,  0) * texelSize);
                half4 bottom = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( 0,  1) * texelSize);
                
                bool isEdge = !ColorsMatch(center, top, _Threshold) ||
                              !ColorsMatch(center, left, _Threshold) ||
                              !ColorsMatch(center, right, _Threshold) ||
                              !ColorsMatch(center, bottom, _Threshold);
                
                if (!isEdge)
                    return center;
                
                half centerLum = Luminance(center.rgb);
                half avgNeighborLum = (Luminance(top.rgb) + Luminance(left.rgb) + 
                                       Luminance(right.rgb) + Luminance(bottom.rgb)) * 0.25;
                
                if (centerLum > avgNeighborLum)
                    return center;
                
                return _EdgeColor;
            }
            ENDHLSL
        }
    }
}
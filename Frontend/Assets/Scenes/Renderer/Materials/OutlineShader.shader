Shader "Sludge/OutlineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform half4 _EdgeColor;
            uniform half4 _WallColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 _MainTex_TexelSize;

            fixed4 frag(v2f i) : SV_Target
            {
                half4 col0 = tex2D(_MainTex, i.uv);
                half4 colL = tex2D(_MainTex, i.uv + float2(-_MainTex_TexelSize.x, 0));
                half4 colR = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0));
                half4 colU = tex2D(_MainTex, i.uv + float2(0, -_MainTex_TexelSize.y));
                half4 colD = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y));
                // Sampling diagonal neighbors
                half4 colUL = tex2D(_MainTex, i.uv + float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y));
                half4 colUR = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y));
                half4 colDL = tex2D(_MainTex, i.uv + float2(-_MainTex_TexelSize.x, _MainTex_TexelSize.y));
                half4 colDR = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y));

                half col0Sum = col0.r + col0.g + col0.b;
                half isBackground = step(0.01, col0Sum);

                // Enhanced edge detection with diagonals
                half4 diff = abs(col0.g - colL.g) + abs(col0.g - colR.g) +
                             abs(col0.g - colU.g) + abs(col0.g - colD.g) +
                             abs(col0.g - colUL.g) + abs(col0.g - colUR.g) +
                             abs(col0.g - colDL.g) + abs(col0.g - colDR.g);
                half edgeStrength = smoothstep(0.0, 0.4, diff); // Adjust range as needed

                half4 color = (col0 * isBackground) + (_WallColor * (1 - isBackground));

                // Smooth blending of edge color
                half4 edgeColor = _EdgeColor * edgeStrength * 3;
                half4 result = lerp(color, edgeColor, edgeStrength); // Linear interpolation for smooth transition

                return result;
            }

            ENDCG
        }
    }
}

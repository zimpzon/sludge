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

            fixed4 frag (v2f i) : SV_Target
            {
                half4 col0 = tex2D(_MainTex, i.uv);
                half4 colL = tex2D(_MainTex, i.uv + float2(-_MainTex_TexelSize.x, 0));
                half4 colD = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y));

                half col0Sum = col0.r + col0.g + col0.b;
                half isBackground = step(0.01, col0Sum);

                half4 diff = abs(col0.g - colL.g) + abs(col0.g - colD.g);
                half isEdge = step(0.01, diff);
                half4 color = (col0 * isBackground) + (_WallColor * (1 - isBackground));
                half4 result = (_EdgeColor * isEdge * 3) + (color * (1 - isEdge));
                return result;
            }
            ENDCG
        }
    }
}

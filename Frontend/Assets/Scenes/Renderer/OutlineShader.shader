Shader "Sludge/OutlineShader"
{
    Properties
    {
        _Color ("EdgeColor", COLOR) = (1,1,1,1)
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
            uniform float4 _EdgeColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 _MainTex_TexelSize;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col0 = tex2D(_MainTex, i.uv);
                fixed4 colL = tex2D(_MainTex, i.uv + float2(-_MainTex_TexelSize.x, 0));
                fixed4 colR = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0));
                fixed4 colU = tex2D(_MainTex, i.uv + float2(0, -_MainTex_TexelSize.y));
                fixed4 colD = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y));
                fixed4 diff = abs(col0 - colL) + abs(col0 - colR) + abs(col0 - colU) + abs(col0 - colD);
                fixed lum = step(0.01, (diff.r + diff.g + diff.b));
                fixed4 result = _EdgeColor * lum + col0 * (1 - lum);
                return result;
            }
            ENDCG
        }
    }
}

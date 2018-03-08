Shader "Hidden/Macaron/UVViewer/Editor/DrawLine"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Thickness ("Thickness", Float) = 1
    }

    SubShader
    {
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geo
            #pragma fragment frag
            #pragma target 4.0
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _Thickness;

            appdata_t vert(appdata_t v)
            {
                appdata_t o;
                o.vertex = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            [maxvertexcount(4)]
            void geo(line appdata_t p[2], inout TriangleStream<g2f> stream)
            {
                float2 vec = p[1].vertex.xy - p[0].vertex.xy;
                float2 offset = normalize(float2(-vec.y, vec.x)) * _Thickness * 0.5;

                float3 worldPos = p[0].vertex;
                worldPos.xy += offset;

                g2f v0;
                v0.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
                stream.Append(v0);

                worldPos = p[1].vertex;
                worldPos.xy += offset;

                g2f v1;
                v1.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
                stream.Append(v1);

                worldPos = p[0].vertex;
                worldPos.xy -= offset;

                g2f v2;
                v2.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
                stream.Append(v2);

                worldPos = p[1].vertex;
                worldPos.xy -= offset;

                g2f v3;
                v3.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
                stream.Append(v3);
            }

            fixed4 frag(g2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }

    SubShader
    {
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float4 _Color;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}

Shader "Hidden/Macaron/UVViewer/Editor/DrawVertex"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _VertexColorRatio ("Vertex Color Ratio", Range (0,1)) = 1
        _Radius ("Radius", Float) = 1
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
            #pragma geometry geo
            #pragma fragment frag
            #pragma target 4.0
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            float4 _Color;
            float _VertexColorRatio;
            float _Radius;

            appdata_t vert(appdata_t v)
            {
                appdata_t o;
                o.vertex = mul(unity_ObjectToWorld, v.vertex);
                o.color = lerp(_Color, v.color, _VertexColorRatio);
                return o;
            }

            [maxvertexcount(4)]
            void geo(point appdata_t p[1], inout TriangleStream<g2f> stream)
            {
                float2 offset = normalize(mul(unity_ObjectToWorld, float4(-1, 1, 0, 0)).xy);
                float3 worldPos = p[0].vertex;
                worldPos.xy += offset * _Radius;

                g2f v0;
                v0.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
                v0.color = p[0].color;
                stream.Append(v0);

                offset = normalize(mul(unity_ObjectToWorld, float4(1, 1, 0, 0)).xy);
                worldPos = p[0].vertex;
                worldPos.xy += offset * _Radius;

                g2f v1;
                v1.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
                v1.color = p[0].color;
                stream.Append(v1);

                offset = normalize(mul(unity_ObjectToWorld, float4(-1, -1, 0, 0)).xy);
                worldPos = p[0].vertex;
                worldPos.xy += offset * _Radius;

                g2f v2;
                v2.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
                v2.color = p[0].color;
                stream.Append(v2);

                offset = normalize(mul(unity_ObjectToWorld, float4(1, -1, 0, 0)).xy);
                worldPos = p[0].vertex;
                worldPos.xy += offset * _Radius;

                g2f v3;
                v3.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
                v3.color = p[0].color;
                stream.Append(v3);
            }

            float4 frag(g2f i) : SV_Target
            {
                return i.color;
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
                float3 normal : NORMAL;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            float4 _Color;
            float _VertexColorRatio;
            float _Radius;

            v2f vert(appdata_t v)
            {
                float2 offset = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0)).xy);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                worldPos.xy += offset * _Radius;

                v2f o;
                o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
                o.color = lerp(_Color, v.color, _VertexColorRatio);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }

    }
}

Shader "Hidden/Macaron/UVViewer/Editor/DrawTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorMaskR ("Color Mask R", Vector) = (1,0,0,0)
        _ColorMaskG ("Color Mask G", Vector) = (0,1,0,0)
        _ColorMaskB ("Color Mask B", Vector) = (0,0,1,0)
        _ColorMaskA ("Color Mask A", Vector) = (0,0,0,1)
        _AdditiveAlpha ("Additive Alpha", Float) = 0
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
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            half4 _ColorMaskR;
            half4 _ColorMaskG;
            half4 _ColorMaskB;
            half4 _ColorMaskA;
            half _AdditiveAlpha;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half4 color = tex2D(_MainTex, i.texcoord);
                fixed4 r = color.r * _ColorMaskR;
                fixed4 g = color.g * _ColorMaskG;
                fixed4 b = color.b * _ColorMaskB;
                fixed4 a = (color.a * _ColorMaskA) + fixed4(0.0, 0.0, 0.0, _AdditiveAlpha);

                return (r + g + b + a) * i.color;
            }
            ENDCG
        }
    }
}

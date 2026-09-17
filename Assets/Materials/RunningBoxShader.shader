Shader "BS/BS_ImageEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Direction("Moving Direction", Vector) = (1, 0, 0, 0) // x 값만 필요하지만 유연성을 위해 vector로 받음
        _Amount("Decresing y amount", float) = 0.5
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed3 _Direction;
            float _Amount;

            v2f vert (appdata v)
            {
                float pi = 3.141592 * 0.5;
                float uvX = v.uv.x * pi;
                float x_dir = cos(uvX) * saturate(_Direction.x) + sin(uvX) * saturate(-_Direction.x);
                float y = v.vertex.y - _Amount * sin(v.uv.y * pi) * x_dir;
                v.vertex.y = y;

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}

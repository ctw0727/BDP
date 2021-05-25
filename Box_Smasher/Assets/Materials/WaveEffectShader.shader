Shader "Effects/WaveEffect"
{
	Properties
    {
        [PerRendererData]_MainTex ("Texture", 2D) = "white" {}
        _NoiseTex("Texture (R,G=X,Y Distortion; B=Mask; A=Unused)", 2D) = "white" {}
        _Intensity("Intensity", Float) = 0.1
    }
    SubShader
    {
        Tags
        { 
            "RenderType" = "Transparent" "Queue" = "Transparent" 
        }

        GrabPass
        {
            "_BackgroundTexture"
        }

		Cull Off ZWrite Off AlphaTest Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 grabPos : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float _Strength;

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);

                return o;
            }

            sampler2D _BackgroundTexture;
            sampler2D _NoiseTex;
            float _Intensity;

            fixed4 frag (v2f i) : SV_Target
            {
                float4 p = i.grabPos;

                half4 d = tex2D(_NoiseTex, i.grabPos);
                p += (d * _Intensity);

                half4 bgcolor = tex2Dproj(_BackgroundTexture, p);

                return bgcolor;
            }
            ENDCG
        }
    }
}
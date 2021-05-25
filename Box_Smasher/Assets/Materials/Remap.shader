Shader "Effects/Remap"
{
	Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _PaletteMap("PaletteMap", 2D) = "white" {}
        _Fade("Fade", Range(0.0, 1.0)) = 0
        _Amount("Amount of gray", Range(0.0, 10.0)) = 0
    }
    SubShader
    {
        Tags
        { 
            "RenderType" = "Transparent" "Queue" = "Transparent" 
        }

        Pass
        {
            Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
		    ZWrite Off 
            Blend SrcAlpha OneMinusSrcAlpha 
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define mod(x, y) x-y*floor(x/y)

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

            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                o.vertex = UnityObjectToClipPos(v.vertex);

                return o;
            }

            sampler2D _NoiseTex;
            sampler2D _PaletteMap;
            float _Fade;
            float _Amount;

            float rand(float2 co){
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 noisePixel = tex2D(_NoiseTex, i.uv);

                // get the displaced pixel
                float4 pixel = tex2D(_MainTex, i.uv);

                // grayscale conversion
                float gray = Luminance(pixel.rgb);
                float noise = step(_Fade, noisePixel);

                gray = mod(gray + _Amount, 1);
                float4 palettePixel = tex2D(_PaletteMap, float2(gray, 1));
                
                float4 fragColor = pixel + (palettePixel * noisePixel.a);

                return fragColor;
            }
            ENDCG
        }
    }
}
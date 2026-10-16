Shader "Custom/VHS_Mobile_BlackFisheye_URP"
{
    Properties
    {
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _BleedAmount("Bleed Amount", Float) = 0.005
        _NoiseAmount("Noise Amount", Float) = 0.05
        _FisheyeBend("Fisheye Bend", Float) = 0.2
        _TimeSpeed("Time Speed", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "VHS"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            float _BleedAmount;
            float _NoiseAmount;
            float _FisheyeBend;
            float _TimeSpeed;

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                float2 uv;

                if (input.vertexID == 0)
                {
                    uv = float2(0.0, 0.0);
                }
                else if (input.vertexID == 1)
                {
                    uv = float2(2.0, 0.0);
                }
                else
                {
                    uv = float2(0.0, 2.0);
                }

                output.uv = float2(uv.x, 1.0 - uv.y);

                output.positionCS = float4(
                    uv * 2.0 - 1.0,
                    0.0,
                    1.0
                );

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                // FISHEYE

                float2 centered = uv - 0.5;

                float len = length(centered);

                uv = 0.5 + centered *
                    (1.0 + _FisheyeBend * len * len);


                // BLACK BORDER

                if (uv.x < 0.0 ||
                    uv.x > 1.0 ||
                    uv.y < 0.0 ||
                    uv.y > 1.0)
                {
                    return half4(0, 0, 0, 1);
                }


                // CHROMATIC BLEED

                float2 rUV = uv + float2(_BleedAmount, 0.0);
                float2 gUV = uv;
                float2 bUV = uv - float2(_BleedAmount, 0.0);

                half r = SAMPLE_TEXTURE2D(
                    _BlitTexture,
                    sampler_BlitTexture,
                    rUV
                ).r;

                half g = SAMPLE_TEXTURE2D(
                    _BlitTexture,
                    sampler_BlitTexture,
                    gUV
                ).g;

                half b = SAMPLE_TEXTURE2D(
                    _BlitTexture,
                    sampler_BlitTexture,
                    bUV
                ).b;

                half3 color = half3(r, g, b);


                // NOISE

                float2 noiseUV = uv * 0.25;

                half noise = SAMPLE_TEXTURE2D(
                    _NoiseTex,
                    sampler_NoiseTex,
                    noiseUV + _TimeParameters.x * _TimeSpeed
                ).r;

                color += (noise - 0.5) * _NoiseAmount;


                return half4(color, 1.0);
            }

            ENDHLSL
        }
    }
}
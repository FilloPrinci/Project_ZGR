Shader "Custom/URP_HeatDistortionPlane"
{
    Properties
    {
        [NoScaleOffset] _NoiseTex ("Noise Texture (tileable, grayscale)", 2D) = "gray" {}
        _Tiling1 ("Tiling 1", Vector) = (2, 4, 0, 0)
        _Speed1 ("Speed 1 (u,v)", Vector) = (0.10, 0.40, 0, 0)
        _Tiling2 ("Tiling 2", Vector) = (5, 3, 0, 0)
        _Speed2 ("Speed 2 (u,v)", Vector) = (-0.05, 0.60, 0, 0)
        _DistortionStrength ("Distortion Strength", Range(0, 0.2)) = 0.03
        _PatchThreshold ("Patch Threshold", Range(0, 1)) = 0.40
        _PatchSoftness ("Patch Softness", Range(0.01, 0.5)) = 0.20
        _EdgeFadeBottom ("Edge Fade Bottom", Range(0, 1)) = 0.15
        _EdgeFadeTop ("Edge Fade Top", Range(0, 1)) = 0.25
        _Opacity ("Opacity", Range(0, 1)) = 1.0
    }

    SubShader
    {
        // Deve stare in coda Transparent e girare DOPO gli oggetti opachi,
        // cosi' _CameraOpaqueTexture contiene gia' la scena da distorcere.
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
            Name "HeatDistortion"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 screenPos   : TEXCOORD1; // NDC, come Screen Position (Default) in Shader Graph
            };

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float2 _Tiling1;
                float2 _Speed1;
                float2 _Tiling2;
                float2 _Speed2;
                float  _DistortionStrength;
                float  _PatchThreshold;
                float  _PatchSoftness;
                float  _EdgeFadeBottom;
                float  _EdgeFadeTop;
                float  _Opacity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = posInputs.positionCS;
                OUT.screenPos   = posInputs.positionNDC;
                OUT.uv          = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float  t  = _Time.y;

                // --- Layer di noise per l'INTENSITA' della turbolenza (crea le "chiazze") ---
                float2 uv1 = uv * _Tiling1 + t * _Speed1;
                float2 uv2 = uv * _Tiling2 + t * _Speed2;

                float noiseA = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv1).r;
                float noiseB = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv2).r;
                float combined = (noiseA + noiseB) * 0.5;

                // --- Secondo campo di noise (offsettato) per la DIREZIONE della distorsione ---
                float2 uv1b = uv1 + float2(0.37, 0.17);
                float2 uv2b = uv2 + float2(0.61, 0.29);
                float noiseAb = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv1b).r;
                float noiseBb = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, uv2b).r;
                float combinedB = (noiseAb + noiseBb) * 0.5;

                float2 distortionDir = float2(combined, combinedB) * 2.0 - 1.0; // -1..1
                float2 offset = distortionDir * _DistortionStrength;

                // --- Distorci la scena dietro al piano ---
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float2 distortedUV = screenUV + offset;
                half3 sceneColor = SampleSceneColor(distortedUV);

                // --- Alpha "a chiazze": simula tante piccole sacche di calore ---
                float patchAlpha = smoothstep(_PatchThreshold, _PatchThreshold + _PatchSoftness, combined);

                // --- Fade verticale (attacca al suolo, dissolve in alto) ---
                float bottomFade = smoothstep(0.0, max(_EdgeFadeBottom, 1e-4), uv.y);
                float topFade    = 1.0 - smoothstep(1.0 - max(_EdgeFadeTop, 1e-4), 1.0, uv.y);
                float verticalMask = bottomFade * topFade;

                float alpha = patchAlpha * verticalMask * _Opacity;

                return half4(sceneColor, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}

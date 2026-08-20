// SOLO PER DIAGNOSTICA: identico a URP_UnderwaterGlassTube.shader ma in coda
// Opaque invece di Transparent, per capire se e' proprio la coda Transparent a
// non ricevere il dato del reflection probe. Assegna questo materiale al tubo
// al posto dell'altro, attiva il debug e controlla: se qui il probe si vede
// correttamente, il problema e' isolato alla coda Transparent.
Shader "Custom/URP_UnderwaterGlassTube_DEBUG_OPAQUE"
{
    Properties
    {
        _Smoothness ("Smoothness (nitidezza riflesso)", Range(0, 1)) = 0.9
        [Toggle] _DebugReflectionRaw ("DEBUG: mostra solo il probe grezzo", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ReflectionProbeDebugOpaque"
            Tags { "LightMode"="UniversalForward" }

            ZWrite On
            ZTest LEqual
            Cull [_CullMode]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #if (UNITY_VERSION >= 60010000)
                #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
                #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #endif

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Smoothness;
                float _DebugReflectionRaw;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS  = posInputs.positionWS;
                OUT.normalWS    = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN, bool frontFace : SV_IsFrontFace) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                N = frontFace ? N : -N;
                float3 V = normalize(_WorldSpaceCameraPos - IN.positionWS);

                float4 positionCS = TransformWorldToHClip(IN.positionWS);
                float2 screenUV = (positionCS.xy / positionCS.w) * 0.5 + 0.5;
                #if UNITY_UV_STARTS_AT_TOP
                    screenUV.y = 1.0 - screenUV.y;
                #endif

                float3 reflectVector = reflect(-V, N);
                half perceptualRoughness = 1.0 - _Smoothness;
                half3 envReflection = GlossyEnvironmentReflection(reflectVector, IN.positionWS, perceptualRoughness, 1.0, screenUV);

                return half4(envReflection, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}

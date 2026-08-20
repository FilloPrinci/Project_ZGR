Shader "Custom/URP_UnderwaterGlassTube"
{
    Properties
    {
        [NoScaleOffset] _NoiseTex ("Noise Texture (tileable, grayscale)", 2D) = "gray" {}

        [Header(Screen Space Distortion Acqua Sullo Sfondo)]
        _ScreenDistortionTiling1 ("Screen Distortion Tiling 1", Vector) = (4, 4, 0, 0)
        _ScreenDistortionSpeed1 ("Screen Distortion Speed 1 (u,v)", Vector) = (0.03, 0.06, 0, 0)
        _ScreenDistortionTiling2 ("Screen Distortion Tiling 2", Vector) = (7, 5, 0, 0)
        _ScreenDistortionSpeed2 ("Screen Distortion Speed 2 (u,v)", Vector) = (-0.05, 0.04, 0, 0)
        _DistortionStrength ("Distortion Strength", Range(0, 0.15)) = 0.02

        [Header(UV Space Surface Increspature Del Vetro Statiche)]
        _SurfaceRippleTiling ("Surface Ripple Tiling", Vector) = (2, 4, 0, 0)
        _SurfaceRippleStrength ("Surface Ripple Strength", Range(0, 0.5)) = 0.08
        _RefractionAmount ("Refraction (bend at edges)", Range(0, 0.3)) = 0.05

        [Header(Glass Look)]
        _GlassTint ("Glass Tint", Color) = (0.6, 0.85, 0.9, 1)
        _GlassOpacity ("Glass Opacity", Range(0, 1)) = 1.0
        _FresnelColor ("Fresnel Rim Color", Color) = (0.7, 0.95, 1, 1)
        _FresnelPower ("Fresnel Power", Range(0.5, 8)) = 3.0
        _FresnelIntensity ("Fresnel Intensity", Range(0, 3)) = 1.2
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularPower ("Specular Power", Range(8, 256)) = 64
        _SpecularIntensity ("Specular Intensity", Range(0, 3)) = 0.6

        [Header(Reflection Probe)]
        // Stesso meccanismo automatico dello shader Lit standard (unity_SpecCube0):
        // nessuno script necessario, il binding e' fatto da Unity per-renderer.
        _Smoothness ("Smoothness (nitidezza riflesso)", Range(0, 1)) = 0.9
        _ReflectionIntensity ("Reflection Intensity", Range(0, 3)) = 1.0
        _BaseReflectivity ("Base Reflectivity (visibile anche di fronte)", Range(0, 1)) = 0.08
        [Toggle] _DebugReflectionRaw ("DEBUG: mostra solo il probe grezzo", Float) = 0

        [Header(Rendering)]
        // Camera dentro al tubo -> di default va vista la faccia "interna" (Front = 1).
        // Se il tubo viene visto anche dall'esterno, o le normali della mesh sono invertite,
        // cambia questo valore dall'Inspector senza toccare lo shader.
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
            Name "UnderwaterGlass"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull [_CullMode]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Keyword richieste da GlossyEnvironmentReflection per funzionare come nello shader
            // Lit standard. Senza queste, in molte versioni URP (Unity 6+) la funzione ripiega
            // silenziosamente sul singolo unity_SpecCube0 "grezzo", che risulta sempre la skybox
            // di default invece del probe vero assegnato all'oggetto.
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            // Unity 6.1+: nuovo sistema "Reflection Probe Atlas". Se il progetto lo usa e queste
            // keyword mancano, GlossyEnvironmentReflection non legge l'atlas e torna alla skybox.
            #if (UNITY_VERSION >= 60010000)
                #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
                #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #endif

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 screenPos   : TEXCOORD1; // NDC
                float3 normalWS    : TEXCOORD2;
                float3 positionWS  : TEXCOORD3;
            };

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float2 _ScreenDistortionTiling1;
                float2 _ScreenDistortionSpeed1;
                float2 _ScreenDistortionTiling2;
                float2 _ScreenDistortionSpeed2;
                float  _DistortionStrength;

                float2 _SurfaceRippleTiling;
                float  _SurfaceRippleStrength;
                float  _RefractionAmount;

                half4  _GlassTint;
                float  _GlassOpacity;
                half4  _FresnelColor;
                float  _FresnelPower;
                float  _FresnelIntensity;
                half4  _SpecularColor;
                float  _SpecularPower;
                float  _SpecularIntensity;

                float  _Smoothness;
                float  _ReflectionIntensity;
                float  _BaseReflectivity;
                float  _DebugReflectionRaw;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = posInputs.positionCS;
                OUT.screenPos   = posInputs.positionNDC;
                OUT.positionWS  = posInputs.positionWS;
                OUT.normalWS    = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv          = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN, bool frontFace : SV_IsFrontFace) : SV_Target
            {
                // Con Cull Front la camera dentro al tubo vede le back-face:
                // riallinea la normale cosi' che punti verso la camera.
                float3 N = normalize(IN.normalWS);
                N = frontFace ? N : -N;

                float3 V = normalize(_WorldSpaceCameraPos - IN.positionWS);
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

                // --- Distorsione dello sfondo (acqua): campionata in SCREEN-SPACE ---
                // Usa le coordinate schermo, non le UV del tubo: cosi' l'ondulazione
                // resta coerente sull'intera view e non si deforma/ripete con l'UV mapping
                // del cilindro (ne' si "attacca" alla geometria quando la camera si muove).
                float2 suvA = screenUV * _ScreenDistortionTiling1 + _Time.y * _ScreenDistortionSpeed1;
                float2 suvB = screenUV * _ScreenDistortionTiling2 + _Time.y * _ScreenDistortionSpeed2;
                float snA = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, suvA).r;
                float snB = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, suvB).r;
                float2 screenWaveDir = float2(snA, snB) * 2.0 - 1.0; // -1..1

                // --- Superficie del vetro (increspature/imperfezioni): campionata in UV-SPACE, STATICA ---
                // Legata alla mesh: si muove/ruota insieme al tubo come una vera proprieta'
                // fisica del vetro (influenza normale, fresnel, specular e piega ai bordi),
                // ma non e' animata nel tempo: e' un pattern fisso della superficie.
                float2 guvA = IN.uv * _SurfaceRippleTiling;
                float2 guvB = IN.uv * _SurfaceRippleTiling * 1.7 + float2(0.5, 0.2);
                float gnA = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, guvA).r;
                float gnB = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, guvB).r;
                float2 surfaceWaveDir = float2(gnA, gnB) * 2.0 - 1.0;

                // Perturba la normale con le increspature UV-space del vetro.
                // IMPORTANTE: il rumore e' un valore 2D arbitrario (UV-space), quindi va
                // proiettato nel piano TANGENTE alla normale, non sommato direttamente alle
                // componenti X/Y in world space (su un cilindro le normali puntano in ogni
                // direzione attorno alla circonferenza: sommare in world space distorce la
                // normale in modo incoerente e manda il riflesso a campionare punti sbagliati
                // della cubemap, es. sempre verso il "cielo").
                float3 upRef = (abs(N.y) < 0.99) ? float3(0, 1, 0) : float3(1, 0, 0);
                float3 tangent = normalize(cross(upRef, N));
                float3 bitangent = cross(N, tangent);
                N = normalize(N + (tangent * surfaceWaveDir.x + bitangent * surfaceWaveDir.y) * _SurfaceRippleStrength);

                float NdotV = saturate(abs(dot(N, V)));
                float fresnel = pow(1.0 - NdotV, _FresnelPower) * _FresnelIntensity;

                // Rifrazione extra ai bordi: dipende dalla normale del vetro (UV-space),
                // ma l'offset finale viene comunque applicato in screen-space sullo sfondo.
                float2 normalOffset = N.xy * _RefractionAmount * (1.0 - NdotV);

                float2 distortedUV = screenUV + screenWaveDir * _DistortionStrength + normalOffset;

                half3 sceneColor = SampleSceneColor(distortedUV);
                sceneColor *= _GlassTint.rgb;

                // --- Specular semplice (Blinn-Phong, senza ombre: economico) ---
                Light mainLight = GetMainLight();
                float3 H = normalize(V + mainLight.direction);
                float spec = pow(saturate(dot(N, H)), _SpecularPower) * _SpecularIntensity;

                // --- Riflesso dal reflection probe: stesso meccanismo automatico di Lit ---
                // GlossyEnvironmentReflection (firma URP 12+/Unity 6) richiede anche le UV di
                // schermo normalizzate: le usa per il blending tra probe vicini / l'atlas.
                // Senza questo 5o parametro (e le keyword sopra) la funzione ripiega su un
                // singolo campionamento "flat" che di fatto mostra solo la skybox di default.
                float3 reflectVector = reflect(-V, N);
                half perceptualRoughness = 1.0 - _Smoothness;
                half3 envReflection = GlossyEnvironmentReflection(reflectVector, IN.positionWS, perceptualRoughness, 1.0, screenUV);

                if (_DebugReflectionRaw > 0.5)
                {
                    return half4(envReflection, 1.0);
                }

                // Il probe e' HDR (cielo/sole possono valere ben oltre 1.0): senza un tonemap
                // quei valori vengono tagliati a bianco piatto in output. Applico un Reinhard
                // morbido cosi' le zone luminose restano leggibili invece di "bruciare".
                envReflection *= _ReflectionIntensity;
                envReflection = envReflection / (1.0 + envReflection);

                // fresnel puro arriva a 0 quando si guarda il vetro di fronte: aggiungo una
                // riflettivita' minima (come il vetro reale, ~4-8%) cosi' il probe si vede sempre.
                float reflectFactor = saturate(fresnel + _BaseReflectivity);
                half3 reflectionTerm = envReflection * _FresnelColor.rgb * reflectFactor;

                half3 finalColor = sceneColor
                                  + fresnel * _FresnelColor.rgb
                                  + reflectionTerm
                                  + spec * _SpecularColor.rgb * mainLight.color;

                return half4(finalColor, _GlassOpacity);
            }
            ENDHLSL
        }
    }

    FallBack Off
}

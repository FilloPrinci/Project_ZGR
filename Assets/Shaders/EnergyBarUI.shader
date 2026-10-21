// Luminous "energy" UI shader: two samples of the same Perlin noise texture, at different tile
// sizes and scrolling in opposite directions, blended together (Multiply by default, Add/Screen
// also available) - the classic trick for a living, organic glow instead of a sliding static
// pattern (a single noise layer, or two layers at the same scale, just looks like a texture
// sliding past; different scales + opposite scroll directions is what breaks up the repetition
// and reads as genuinely organic).
//
// Also doubles as the "halo" layer around the bar (a second Image instance of this same shader,
// larger and behind the crisp bar - see RectTransformHaloFollower.cs) via _EdgeFeather, which
// fades alpha to 0 near this element's own rect edges - the glow source's own semi-transparency
// comes from the Image's ordinary Color alpha, already respected like any other UI shader.
//
// Built on Unity's standard UI/Default.shader structure (stencil/clip-rect/alpha-clip support)
// rather than a Shader Graph: Shader Graph assets are opaque, hand-rolled JSON with internal
// node GUIDs - authoring one correctly by hand (not through the graph editor) is fragile to the
// point of likely producing a broken graph. A plain HLSL shader achieves the identical visual
// result and is something that can actually be authored and verified reliably as text.
Shader "ZGR/UI/EnergyBar"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint (multiplies the Image's own color - leave white to let Image.color drive it)", Color) = (1, 1, 1, 1)

        [Header(Noise)]
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        [KeywordEnum(Multiply, Add, Screen)] _BlendMode ("Noise Blend Mode", Float) = 0
        // Local/canvas units per tile - independent of the bar's own width (see frag()). Two
        // different sizes, one coarser and one finer, is what the "one texture bigger, one
        // smaller" look actually needs - two layers at the same scale just reinforce each other
        // instead of adding detail.
        _NoiseTileSizeA ("Noise Tile Size A (coarser)", Float) = 150
        _NoiseTileSizeB ("Noise Tile Size B (finer)", Float) = 60
        _ScrollSpeedA ("Scroll Speed A (local units/sec)", Vector) = (70, 35, 0, 0)
        _ScrollSpeedB ("Scroll Speed B (local units/sec)", Vector) = (-55, -28, 0, 0)

        [Header(Look)]
        _Brightness ("Brightness", Float) = 1.6
        _Contrast ("Contrast", Float) = 1.3

        // Pseudo-emission: no real Bloom here (Overlay canvas can't trigger it, see frag()).
        [Header(PseudoEmission)]
        // Dim Floor: base dimness where the noise is dark - 0 = fully black base, 1 = no darkening at all.
        _DimFloor ("Dim Floor", Range(0, 1)) = 0.35
        // Glow Power: how sharply the bright spots stand out from the rest - higher = fewer, punchier hotspots.
        _GlowPower ("Glow Power", Float) = 3
        // Glow Boost: extra additive brightness at the hotspots, on top of the base color - this is what reads as "emissive" without actually being HDR/Bloom.
        _GlowBoost ("Glow Boost", Float) = 1.8

        [Header(Halo)]
        // Edge Feather: 0 = hard-edged (use for the bar itself). On a halo instance (a larger,
        // semi-transparent copy sitting behind the bar - see RectTransformHaloFollower.cs), a
        // value around 0.3-0.5 fades it to nothing near its own rect edges instead of cutting
        // off sharply, so it reads as a soft bloom bleeding outward rather than a second bar.
        _EdgeFeather ("Edge Feather", Range(0, 0.5)) = 0

        // UI internals below (stencil/clip support) - leave at their defaults.
        [Header(UI Internals)]
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "CanCullOverride"="False"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma shader_feature_local _BLENDMODE_MULTIPLY _BLENDMODE_ADD _BLENDMODE_SCREEN

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _TextureSampleAdd;
            float4 _MainTex_ST;
            fixed4 _Color;
            float4 _ClipRect;

            sampler2D _NoiseTex;
            float _NoiseTileSizeA;
            float _NoiseTileSizeB;
            float4 _ScrollSpeedA;
            float4 _ScrollSpeedB;
            float _Brightness;
            float _Contrast;
            float _DimFloor;
            float _GlowPower;
            float _GlowBoost;
            float _EdgeFeather;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // IN.worldPosition.xy is the RectTransform's own LOCAL-space vertex position (set
                // in vert() before the clip-space transform), in local/Canvas units - NOT the
                // normalized 0..1 texcoord. Dividing by a fixed tile size gives a tile that stays
                // a constant size in those units regardless of how wide the bar's rect currently
                // is (anchors stretching it just reveals more or fewer tiles, the same way a
                // world-space texture doesn't restretch when the mesh it's on resizes) - using
                // texcoord instead (the original approach) re-stretched the same fixed repeat
                // count across whatever the current width was, and on this bar's actual ~16px
                // tall strip, squashed the pattern into single-digit-pixel bands that read as
                // static dithering instead of visible flowing motion.
                float2 uvA = IN.worldPosition.xy / _NoiseTileSizeA + _Time.y * _ScrollSpeedA.xy / _NoiseTileSizeA;
                float2 uvB = IN.worldPosition.xy / _NoiseTileSizeB + _Time.y * _ScrollSpeedB.xy / _NoiseTileSizeB;

                fixed noiseA = tex2D(_NoiseTex, uvA).r;
                fixed noiseB = tex2D(_NoiseTex, uvB).r;

                #if defined(_BLENDMODE_ADD)
                    fixed rawCombined = saturate(noiseA + noiseB - 1.0);
                #elif defined(_BLENDMODE_SCREEN)
                    fixed rawCombined = 1.0 - (1.0 - noiseA) * (1.0 - noiseB);
                #else // Multiply (default) - two opposite-scrolling, differently-scaled layers
                      // multiplied together is what breaks up each one's own repetition and
                      // reads as organic instead of a sliding texture.
                    fixed rawCombined = noiseA * noiseB;
                #endif

                // Contrast/Brightness push the result (Multiply skews dark, two 0..1 values
                // multiplied together; Add/Screen skew bright) back into a punchy "energy" look
                // instead of a flat grey/white blend.
                fixed combined = saturate((rawCombined - 0.5) * _Contrast + 0.5) * _Brightness;

                fixed4 mainTexColor = tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd;
                fixed4 color = IN.color * mainTexColor;

                // Pseudo-emission: the Overlay canvas this renders on can never trigger real
                // Bloom (that's a post-process step this draw happens after, confirmed live -
                // no color value, HDR or not, reaches it), so instead of just scaling brightness
                // linearly by the combined noise (which read as a flat, evenly-lit tint, not an
                // "energy" glow), hotspots get pushed additively brighter than the base color
                // while darker noise regions only dim down to _DimFloor rather than to black -
                // the two together are what actually read as "brighter where the noise is
                // lighter, weaker where it's darker" instead of uniform.
                fixed glow = pow(combined, _GlowPower);
                color.rgb *= lerp(_DimFloor, 1.0, combined);
                color.rgb += color.rgb * glow * _GlowBoost;

                if (_EdgeFeather > 0.0)
                {
                    float fadeX = smoothstep(0.0, _EdgeFeather, IN.texcoord.x) * smoothstep(0.0, _EdgeFeather, 1.0 - IN.texcoord.x);
                    float fadeY = smoothstep(0.0, _EdgeFeather, IN.texcoord.y) * smoothstep(0.0, _EdgeFeather, 1.0 - IN.texcoord.y);
                    color.a *= fadeX * fadeY;
                }

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}

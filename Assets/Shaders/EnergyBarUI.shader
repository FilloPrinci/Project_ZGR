// Luminous "energy" UI shader: two samples of the same Perlin noise texture, at different tile
// sizes and scrolling in opposite directions, blended together (Multiply by default, Add/Screen
// also available) - the classic trick for a living, organic glow instead of a sliding static
// pattern (a single noise layer, or two layers at the same scale, just looks like a texture
// sliding past; different scales + opposite scroll directions is what breaks up the repetition
// and reads as genuinely organic).
//
// Also doubles as the "halo" layers around the bar (separate Image instances of this same
// shader, behind the crisp bar - see RectTransformHaloFollower.cs, which builds one piece per
// direction: Up, Down, Right) via the per-side _Feather* properties, which fade alpha to 0 only
// on the one outer side that faces away from the bar - the glow source's own semi-transparency
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
        // Burn Amount: at the hotspots, blend the color toward pure white instead of just scaling
        // it brighter - a real light source overexposes/clips to white at its brightest point
        // rather than staying a saturated, tinted bright version of itself, so this is what makes
        // it read as "emitting light" instead of "a brightly colored texture".
        _BurnAmount ("Burn Amount", Range(0, 2)) = 1.3

        [Header(Halo)]
        // Directional edge feather: each side fades independently (0 = hard edge on that side).
        // Deliberately NOT a single symmetric fadeX*fadeY like a vignette - that fades the
        // corners faster than the edge midpoints, which reads as a rounded/circular blob instead
        // of a clean directional glow. Each halo piece (see RectTransformHaloFollower.cs, which
        // now builds three separate pieces - Up/Down/Right) only feathers the one outer side that
        // actually faces away from the bar, and leaves the other three sides hard (0) so it butts
        // seamlessly against the bar with uniform intensity along that shared edge.
        _FeatherLeft ("Feather Left", Range(0, 1)) = 0
        _FeatherRight ("Feather Right", Range(0, 1)) = 0
        _FeatherTop ("Feather Top", Range(0, 1)) = 0
        _FeatherBottom ("Feather Bottom", Range(0, 1)) = 0

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
            float _BurnAmount;
            float _FeatherLeft;
            float _FeatherRight;
            float _FeatherTop;
            float _FeatherBottom;

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
                // "energy" glow), darker noise regions only dim down to _DimFloor rather than to
                // black, and hotspots burn toward pure white rather than just a brighter tint of
                // the base color - a real light source clips/overexposes to white at its
                // brightest point, it doesn't stay a saturated bright version of its own color,
                // so blending toward white (instead of additively scaling it) is what actually
                // reads as "emitting light" rather than "a brightly colored texture".
                fixed glow = pow(combined, _GlowPower);
                color.rgb *= lerp(_DimFloor, 1.0, combined);
                color.rgb = lerp(color.rgb, fixed3(1.0, 1.0, 1.0), saturate(glow * _BurnAmount));

                float fadeLeft = _FeatherLeft > 0.0 ? smoothstep(0.0, _FeatherLeft, IN.texcoord.x) : 1.0;
                float fadeRight = _FeatherRight > 0.0 ? smoothstep(0.0, _FeatherRight, 1.0 - IN.texcoord.x) : 1.0;
                float fadeBottom = _FeatherBottom > 0.0 ? smoothstep(0.0, _FeatherBottom, IN.texcoord.y) : 1.0;
                float fadeTop = _FeatherTop > 0.0 ? smoothstep(0.0, _FeatherTop, 1.0 - IN.texcoord.y) : 1.0;
                color.a *= fadeLeft * fadeRight * fadeBottom * fadeTop;

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

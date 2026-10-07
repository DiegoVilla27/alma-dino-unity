// Sprite shader for resizable liquids (lava, toxic lake, mud). Built-in pipeline, mobile friendly:
// one texture sample and a handful of ALU ops per pixel, no extra textures or grab passes.
//  - Pulse: the brightest parts of the art (lava veins) brighten and dim slowly, with a phase that
//    travels across the liquid so it never pulses all at once.
//  - Sway: a tiny horizontal ripple of the texture; the art tiles horizontally, so it needs the
//    texture's wrap mode U set to Repeat to stay seamless at tile edges.
// Works with SpriteRenderer Tiled/Sliced draw modes and Flip X/Y.
Shader "AlmaGame/LiquidSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Pulse)]
        _GlowColor ("Glow Color", Color) = (1, 0.75, 0.25, 1)
        _GlowThreshold ("Bright Threshold", Range(0, 1)) = 0.55
        _PulseStrength ("Pulse Strength", Range(0, 1)) = 0.35
        _PulseSpeed ("Pulse Speed", Float) = 0.6
        _PulseScale ("Pulse Wave Scale (per unit)", Float) = 0.45

        [Header(Sway)]
        _SwayAmount ("Sway Amount (UV)", Range(0, 0.02)) = 0.004
        _SwaySpeed ("Sway Speed", Float) = 0.8
        _SwayScale ("Sway Wave Scale (per unit)", Float) = 2.2

        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
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
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #include "UnitySprites.cginc"

            fixed4 _GlowColor;
            half _GlowThreshold;
            half _PulseStrength;
            float _PulseSpeed;
            float _PulseScale;
            float _SwayAmount;
            float _SwaySpeed;
            float _SwayScale;

            struct v2fLiquid
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 world : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Same as SpriteVert (flip, tint, pixel snap) plus the world position.
            v2fLiquid vert(appdata_t v)
            {
                v2fLiquid o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float4 local = UnityFlipSprite(v.vertex, _Flip);
                o.vertex = UnityObjectToClipPos(local);
                #ifdef PIXELSNAP_ON
                o.vertex = UnityPixelSnap(o.vertex);
                #endif
                o.texcoord = v.texcoord;
                o.color = v.color * _Color * _RendererColor;
                // World position drives both waves, so neighbouring tiles stay in phase.
                o.world = mul(unity_ObjectToWorld, local).xy;
                return o;
            }

            fixed4 frag(v2fLiquid i) : SV_Target
            {
                float t = _Time.y;
                float2 uv = i.texcoord;
                uv.x += sin(i.world.y * _SwayScale + t * _SwaySpeed) * _SwayAmount;

                fixed4 c = SampleSpriteTexture(uv) * i.color;

                // Only the bright veins pulse; dark crusts stay as they are.
                half lum = dot(c.rgb, half3(0.299, 0.587, 0.114));
                half mask = saturate((lum - _GlowThreshold) / (1.0 - _GlowThreshold));
                float phase = t * _PulseSpeed + (i.world.x + i.world.y * 0.6) * _PulseScale;
                half pulse = 0.5 + 0.5 * sin(phase) * sin(phase * 0.37 + 1.7);
                c.rgb += _GlowColor.rgb * (mask * pulse * _PulseStrength);

                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}

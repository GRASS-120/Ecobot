Shader "UI/LineFlowChaos"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [MainColor]   _Color   ("Tint", Color) = (1,1,1,1)

        // Основной «перелив» (аддитивно поверх базового)
        _FlowColor ("Flow Color", Color) = (1,1,1,1)
        _Intensity ("Flow Intensity", Range(0,3)) = 1

        // Движение вдоль линии
        _Speed     ("Speed (along U)", Range(0,10)) = 1.2
        _Frequency ("Wave Frequency",  Range(0,30)) = 10
        _BandWidth ("Band Width",      Range(0.02,0.6)) = 0.2

        // Шум для хаотичности
        _NoiseScale  ("Noise Scale",  Range(0.1,10)) = 2.0
        _NoiseAmp    ("Noise Amount", Range(0,1))    = 0.35
        _NoiseSpeed  ("Noise Speed",  Range(0,5))    = 0.7
        _Octaves     ("Noise Octaves (1..5)", Range(1,5)) = 3

        // UI стандарт
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil     ("Stencil ID", Float) = 0
        _StencilOp   ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        _ClipRect   ("Clip Rect", Vector) = (-10000, -10000, 10000, 10000)
        _UIMaskSoftnessX ("Mask Softness X", Float) = 0
        _UIMaskSoftnessY ("Mask Softness Y", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
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
            Name "UILINEFLOW"
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target 2.0

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0; // от UIBezierConnection: u вдоль, v поперёк
                fixed4 color    : COLOR;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                fixed4 color    : COLOR;
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            fixed4 _FlowColor;
            float  _Intensity;
            float  _Speed;
            float  _Frequency;
            float  _BandWidth;

            float  _NoiseScale;
            float  _NoiseAmp;
            float  _NoiseSpeed;
            float  _Octaves;

            float4 _ClipRect;
            float  _UIMaskSoftnessX;
            float  _UIMaskSoftnessY;

            // --- простые хэш и value-noise ---
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 78.233);
                return frac(p.x * p.y);
            }

            float vnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash21(i + float2(0,0));
                float b = hash21(i + float2(1,0));
                float c = hash21(i + float2(0,1));
                float d = hash21(i + float2(1,1));

                float2 u = f*f*(3.0 - 2.0*f);
                return lerp(lerp(a,b,u.x), lerp(c,d,u.x), u.y);
            }

            float fbm(float2 p, int oct)
            {
                float a = 0.0;
                float w = 0.5;
                for (int i=0;i<5;i++)
                {
                    if (i >= oct) break;
                    a += vnoise(p) * w;
                    p *= 2.02;
                    w *= 0.5;
                }
                return a;
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.uv       = v.texcoord; // u вдоль кривой, v поперёк
                o.color    = v.color * _Color;
                o.worldPos = v.vertex;
                return o;
            }

            // узкая плавная «полоса» вокруг пика синуса (без резких клипов)
            float band(float x, float width01)
            {
                float s = 0.5 + 0.5 * sin(x);
                float edge = 1.0 - saturate(width01);
                return smoothstep(edge, 1.0, s);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv) * i.color;

                // u — вдоль, v — поперёк
                float u = saturate(i.uv.x);
                float v = saturate(i.uv.y);

                // чуть «сожмём» влияние на краях, чтобы серединка была ярче
                float across = smoothstep(0.0, 0.15, v) * (1.0 - smoothstep(0.85, 1.0, v));

                // хаотичный оффсет фазы из fBm (uv + анимированная добавка)
                float2 pNoise = float2(u, v) * _NoiseScale + float2(0.0, _Time.y * _NoiseSpeed);
                float n = fbm(pNoise, (int)_Octaves);          // 0..~1
                float phase = _Time.y * _Speed * 6.2831853;    // 2π * speed * t

                // волна вдоль U с шумовым «дрожанием» частоты/фазы
                float wave = band(u * _Frequency + phase + (n * 3.14159), _BandWidth);

                // добавляем ещё лёгкую стохастичность амплитуды
                float ampJitter = lerp(0.85, 1.15, vnoise(pNoise + 13.37));

                // итоговая аддитивная подсветка
                float bloom = wave * across * _Intensity * ampJitter;
                fixed3 rgb = baseCol.rgb + _FlowColor.rgb * bloom;

                // мягкое маскирование для RectMask2D
                #ifdef UNITY_UI_CLIP_RECT
                float2 pixelSize = 1.0 / float2(
                    abs(ddx(i.pos.x)) + 1e-6,
                    abs(ddy(i.pos.y)) + 1e-6
                );
                float2 softness = float2(_UIMaskSoftnessX, _UIMaskSoftnessY) * pixelSize;
                float alphaMask = UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                alphaMask = UnityUIApplySoftness(alphaMask, softness);
                baseCol.a *= alphaMask;
                #endif

                fixed4 finalCol = fixed4(rgb, baseCol.a);

                #ifdef UNITY_UI_ALPHACLIP
                clip(finalCol.a - 0.001);
                #endif

                return finalCol;
            }
            ENDCG
        }
    }
}

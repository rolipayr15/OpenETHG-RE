// School lesson/break clock overlay. BreakLessonClock paints the dial mesh's vertex colours
// (lesson/break segments, alpha 0.3); this blends them over the clock with the game's
// distance falloff so the overlay fades into the dark like the level around it.
// The original shader was in the School asset bundle and is not in the shipped build.
Shader "Falloff/LessonBreakClockTransparent" {
	Properties {
		_Color ("Tint", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Cull Back
			Offset -1, -1
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float3 _FalloffColor;
			float _FalloffOffset;
			float _FalloffScale;
			float _ShaderBrightness;

			struct appdata {
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float4 color : COLOR;
				float falloff : TEXCOORD0;
			};

			v2f vert(appdata v) {
				v2f o;
				float3 world = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.falloff = distance(world, _WorldSpaceCameraPos.xyz) * _FalloffScale + _FalloffOffset;
				o.color = v.color;
				return o;
			}

			float4 frag(v2f i) : SV_Target {
				float3 c = i.color.rgb * (_ShaderBrightness + 1.0);
				c = lerp(c, _FalloffColor, saturate(i.falloff));
				return float4(c, i.color.a);
			}
			ENDCG
		}
	}
}

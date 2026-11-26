Shader "Eyes/UI/WinShareFog" {
	Properties {
		_MainTex ("Fog Texture", 2D) = "white" {}
		_ClearThreshold ("Clear Threshold", Range(0.009999999776482582, 1.0)) = 0.6499999761581421
		_EdgeFeather ("Edge Feather", Range(0.0010000000474974513, 0.25)) = 0.029999999329447746
		_FogUVScale ("Fog UV Scale", Vector) = (1.0,1.0,0.0,0.0)
		_FogTint ("Fog Tint (RGB) / Strength (A)", Color) = (0.0,0.0,0.0,0.0)
		_BlackCoreStrength ("Black Core Strength", Range(0.0, 1.0)) = 0.0
		_BlackCoreCenter ("Black Core Center", Vector) = (0.5,0.4399999976158142,0.0,0.0)
		_BlackCoreRadii ("Black Core Radii", Vector) = (0.30000001192092896,0.46000000834465027,0.0,0.0)
		_BlackCoreFeather ("Black Core Feather", Range(0.009999999776482582, 1.0)) = 0.20000000298023224
		_BlackCoreIrregularity ("Black Core Irregularity", Range(0.0, 1.0)) = 0.0
		_StencilComp ("Stencil Comparison", Float) = 8.0
		_Stencil ("Stencil ID", Float) = 0.0
		_StencilOp ("Stencil Operation", Float) = 0.0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255.0
		_StencilReadMask ("Stencil Read Mask", Float) = 255.0
		_ColorMask ("Color Mask", Float) = 15.0
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0.0
	}
	SubShader {
		Tags { "CanUseSpriteAtlas"="False" "IgnoreProjector"="true" "PreviewType"="Plane" "Queue"="Transparent" "RenderType"="Transparent" }
		Pass {
			Name "Default"
			Tags { "CanUseSpriteAtlas"="False" "IgnoreProjector"="true" "PreviewType"="Plane" "Queue"="Transparent" "RenderType"="Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask [_ColorMask]
			ZWrite Off
			ZTest [unity_GUIZTestMode]
			Cull Off
			Stencil {
				Ref [_Stencil]
				ReadMask [_StencilReadMask]
				WriteMask [_StencilWriteMask]
				Comp [_StencilComp]
				Pass [_StencilOp]
				Fail Keep
				ZFail Keep
			}
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#pragma multi_compile _ UNITY_UI_ALPHACLIP
#pragma multi_compile _ UNITY_UI_CLIP_RECT
#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc"

#define MCOL(M,i) float4(M[0][i],M[1][i],M[2][i],M[3][i])
#define GL_TEX2DLOD(S,uv,lod) tex2Dlod(S,float4((uv).xy,0,lod))
#define GL_CUBELOD(S,uv,lod) texCUBElod(S,float4((uv).xyz,lod))
#define GL_TEX3DLOD(S,uv,lod) tex3Dlod(S,float4((uv).xyz,lod))
#define GL_CUBE_LOD(S,uv,lod) UNITY_SAMPLE_TEXCUBE_LOD(S,uv,lod)
#define GL_CUBE_S(S,uv) UNITY_SAMPLE_TEXCUBE(S,uv)
#define GEQ(a,b) ((a)>=(b))
#define LEQ(a,b) ((a)<=(b))
#define GT(a,b) ((a)>(b))
#define LT(a,b) ((a)<(b))
#define EQ(a,b) ((a)==(b))
#define NE(a,b) ((a)!=(b))
float glmod(float a, float b) { return a - b * floor(a / b); }
float2 glmod(float2 a, float2 b) { return a - b * floor(a / b); }
float3 glmod(float3 a, float3 b) { return a - b * floor(a / b); }
float4 glmod(float4 a, float4 b) { return a - b * floor(a / b); }

sampler2D _MainTex;
float4 _BlackCoreCenter;
float _BlackCoreFeather;
float _BlackCoreIrregularity;
float4 _BlackCoreRadii;
float _BlackCoreStrength;
float _ClearThreshold;
float4 _ClipRect;
float _EdgeFeather;
float4 _FogTint;
float4 _FogUVScale;
#if defined(UNITY_UI_ALPHACLIP) && defined(UNITY_UI_CLIP_RECT)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_COLOR0 : COLOR0;
	float2 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_COLOR0 : COLOR0;
	float2 vs_TEXCOORD0 : TEXCOORD0;
	float4 vs_TEXCOORD1 : TEXCOORD1;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_COLOR0 = genIn.in_COLOR0;
	float2 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_COLOR0 = (float4)0;
	float2 vs_TEXCOORD0 = (float2)0;
	float4 vs_TEXCOORD1 = (float4)0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat1 = MCOL(unity_MatrixVP,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_MatrixVP,2) * u_xlat0.zzzz + u_xlat1;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat0.wwww + u_xlat1;
    vs_COLOR0 = in_COLOR0;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy;
    vs_TEXCOORD1 = in_POSITION0;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_COLOR0 = vs_COLOR0;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float2 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float3 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat16_1;
	float4 u_xlat2;
	bool2 u_xlatb2;
	float4 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat5;
	bool3 u_xlatb5;
	float3 u_xlat6;
	float2 u_xlat10;
	float2 u_xlat11;
	float u_xlat15;
	float4 SV_Target0 = 0;

    u_xlat0.xy = vs_TEXCOORD0.xy * float2(13.0, 13.0) + float2(4.69999981, 9.19999981);
    u_xlat10.xy = floor(u_xlat0.xy);
    u_xlat0.xy = frac(u_xlat0.xy);
    u_xlat1.xy = u_xlat10.xy + float2(1.0, 1.0);
    u_xlat1.x = dot(u_xlat1.xy, float2(127.099998, 311.700012));
    u_xlat1.w = sin(u_xlat1.x);
    u_xlat2.x = dot(u_xlat10.xy, float2(127.099998, 311.700012));
    u_xlat3 = u_xlat10.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlat1.x = sin(u_xlat2.x);
    u_xlat10.x = dot(u_xlat3.xy, float2(127.099998, 311.700012));
    u_xlat10.y = dot(u_xlat3.zw, float2(127.099998, 311.700012));
    u_xlat1.yz = sin(u_xlat10.xy);
    u_xlat1 = u_xlat1 * float4(43758.5469, 43758.5469, 43758.5469, 43758.5469);
    u_xlat1 = frac(u_xlat1);
    u_xlat10.xy = (-u_xlat1.xz) + u_xlat1.yw;
    u_xlat6.xz = u_xlat0.xy * u_xlat0.xy;
    u_xlat0.xy = (-u_xlat0.xy) * float2(2.0, 2.0) + float2(3.0, 3.0);
    u_xlat0.xy = u_xlat0.xy * u_xlat6.xz;
    u_xlat0.xz = u_xlat0.xx * u_xlat10.xy + u_xlat1.xz;
    u_xlat10.x = (-u_xlat0.x) + u_xlat0.z;
    u_xlat0.x = u_xlat0.y * u_xlat10.x + u_xlat0.x;
    u_xlat0.x = u_xlat0.x * 0.349999994;
    u_xlat5.xy = vs_TEXCOORD0.xy * float2(7.0, 7.0);
    u_xlat1.xy = floor(u_xlat5.xy);
    u_xlat5.xy = frac(u_xlat5.xy);
    u_xlat11.xy = u_xlat1.xy + float2(1.0, 1.0);
    u_xlat15 = dot(u_xlat11.xy, float2(127.099998, 311.700012));
    u_xlat2.w = sin(u_xlat15);
    u_xlat15 = dot(u_xlat1.xy, float2(127.099998, 311.700012));
    u_xlat1 = u_xlat1.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlat2.x = sin(u_xlat15);
    u_xlat15 = dot(u_xlat1.xy, float2(127.099998, 311.700012));
    u_xlat1.x = dot(u_xlat1.zw, float2(127.099998, 311.700012));
    u_xlat2.z = sin(u_xlat1.x);
    u_xlat2.y = sin(u_xlat15);
    u_xlat1 = u_xlat2 * float4(43758.5469, 43758.5469, 43758.5469, 43758.5469);
    u_xlat1 = frac(u_xlat1);
    u_xlat6.xz = (-u_xlat1.xz) + u_xlat1.yw;
    u_xlat2.xy = u_xlat5.xy * u_xlat5.xy;
    u_xlat5.xy = (-u_xlat5.xy) * float2(2.0, 2.0) + float2(3.0, 3.0);
    u_xlat5.xy = u_xlat5.xy * u_xlat2.xy;
    u_xlat5.xz = u_xlat5.xx * u_xlat6.xz + u_xlat1.xz;
    u_xlat15 = (-u_xlat5.x) + u_xlat5.z;
    u_xlat5.x = u_xlat5.y * u_xlat15 + u_xlat5.x;
    u_xlat0.x = u_xlat5.x * 0.649999976 + u_xlat0.x;
    u_xlat0.x = u_xlat0.x + -0.5;
    u_xlat5.xy = vs_TEXCOORD0.xy + (-_BlackCoreCenter.xy);
    u_xlat1.xy = max(_BlackCoreRadii.xy, float2(9.99999975e-06, 9.99999975e-06));
    u_xlat5.xy = u_xlat5.xy / u_xlat1.xy;
    u_xlat5.x = dot(u_xlat5.xy, u_xlat5.xy);
    u_xlat5.x = sqrt(u_xlat5.x);
    u_xlat0.x = u_xlat0.x * _BlackCoreIrregularity + u_xlat5.x;
    u_xlat0.x = u_xlat0.x + -1.0;
    u_xlat5.x = max(_BlackCoreFeather, 9.99999975e-06);
    u_xlat5.x = float(1.0) / u_xlat5.x;
    u_xlat0.x = u_xlat5.x * u_xlat0.x;
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat5.x = u_xlat0.x * -2.0 + 3.0;
    u_xlat0.x = u_xlat0.x * u_xlat0.x;
    u_xlat0.x = (-u_xlat5.x) * u_xlat0.x + 1.0;
    u_xlat5.x = (-_BlackCoreStrength) * u_xlat0.x + 1.0;
    u_xlat10.xy = vs_TEXCOORD0.xy + float2(-0.5, -0.5);
    u_xlat10.xy = u_xlat10.xy * _FogUVScale.xy + float2(0.5, 0.5);
    u_xlat16_1 = tex2D(_MainTex, u_xlat10.xy);
    u_xlat10.x = max(u_xlat16_1.z, u_xlat16_1.y);
    u_xlat10.x = max(u_xlat10.x, u_xlat16_1.x);
    u_xlat2.xy = max(float2(_ClearThreshold, _EdgeFeather), float2(9.99999975e-06, 9.99999975e-06));
    u_xlat2.xy = float2(1.0, 1.0) / u_xlat2.xy;
    u_xlat10.x = u_xlat10.x * u_xlat2.x;
    u_xlat10.x = clamp(u_xlat10.x, 0.0, 1.0);
    u_xlat15 = u_xlat10.x * -2.0 + 3.0;
    u_xlat10.x = u_xlat10.x * u_xlat10.x;
    u_xlat10.x = (-u_xlat15) * u_xlat10.x + 1.0;
    u_xlat10.x = u_xlat10.x * u_xlat10.x;
    u_xlat10.x = u_xlat10.x * u_xlat16_1.w;
    u_xlat5.x = u_xlat5.x * u_xlat10.x;
    u_xlat0.x = _BlackCoreStrength * u_xlat0.x + u_xlat5.x;
    u_xlat10.xy = (-vs_TEXCOORD0.xy) + float2(1.0, 1.0);
    u_xlat10.xy = min(u_xlat10.xy, vs_TEXCOORD0.xy);
    u_xlat10.x = min(u_xlat10.y, u_xlat10.x);
    u_xlat10.x = u_xlat2.y * u_xlat10.x;
    u_xlat10.x = clamp(u_xlat10.x, 0.0, 1.0);
    u_xlat15 = u_xlat10.x * -2.0 + 3.0;
    u_xlat10.x = u_xlat10.x * u_xlat10.x;
    u_xlat10.x = u_xlat10.x * u_xlat15;
    u_xlat10.x = u_xlat10.x * vs_COLOR0.w;
    u_xlat10.x = u_xlat10.x * u_xlat0.x;
    u_xlat16_4.x = max(u_xlat0.x, 9.99999975e-06);
    u_xlat0.x = u_xlat5.x / u_xlat16_4.x;
    u_xlatb5.xz = GEQ(vs_TEXCOORD1.xxyy, _ClipRect.xxyy).xz;
    u_xlat5.x = u_xlatb5.x ? float(1.0) : 0.0;
    u_xlat5.z = u_xlatb5.z ? float(1.0) : 0.0;
;
    u_xlatb2.xy = GEQ(_ClipRect.zwzz, vs_TEXCOORD1.xyxx).xy;
    u_xlat2.x = u_xlatb2.x ? float(1.0) : 0.0;
    u_xlat2.y = u_xlatb2.y ? float(1.0) : 0.0;
;
    u_xlat5.xz = u_xlat5.xz * u_xlat2.xy;
    u_xlat5.x = u_xlat5.z * u_xlat5.x;
    u_xlat16_4.x = u_xlat10.x * u_xlat5.x + -0.00100000005;
    u_xlat5.x = u_xlat5.x * u_xlat10.x;
    SV_Target0.w = u_xlat5.x;
    u_xlatb5.x = u_xlat16_4.x<0.0;
    if(u_xlatb5.x){discard;}
    u_xlat16_4.xyz = (-u_xlat16_1.xyz) + _FogTint.xyz;
    u_xlat16_4.xyz = _FogTint.www * u_xlat16_4.xyz + u_xlat16_1.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat16_4.xyz;
    SV_Target0.xyz = u_xlat0.xyz * vs_COLOR0.xyz;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(UNITY_UI_ALPHACLIP) && !defined(UNITY_UI_CLIP_RECT)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_COLOR0 : COLOR0;
	float2 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_COLOR0 : COLOR0;
	float2 vs_TEXCOORD0 : TEXCOORD0;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_COLOR0 = genIn.in_COLOR0;
	float2 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_COLOR0 = (float4)0;
	float2 vs_TEXCOORD0 = (float2)0;
	float4 vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat1 = MCOL(unity_MatrixVP,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_MatrixVP,2) * u_xlat0.zzzz + u_xlat1;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat0.wwww + u_xlat1;
    vs_COLOR0 = in_COLOR0;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy;
    vs_TEXCOORD1 = in_POSITION0;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_COLOR0 = vs_COLOR0;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float2 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float3 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat16_1;
	float4 u_xlat2;
	float4 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat5;
	bool u_xlatb5;
	float3 u_xlat6;
	float u_xlat16_9;
	float2 u_xlat10;
	float2 u_xlat11;
	float u_xlat15;
	float4 SV_Target0 = 0;

    u_xlat0.xy = vs_TEXCOORD0.xy * float2(13.0, 13.0) + float2(4.69999981, 9.19999981);
    u_xlat10.xy = floor(u_xlat0.xy);
    u_xlat0.xy = frac(u_xlat0.xy);
    u_xlat1.xy = u_xlat10.xy + float2(1.0, 1.0);
    u_xlat1.x = dot(u_xlat1.xy, float2(127.099998, 311.700012));
    u_xlat1.w = sin(u_xlat1.x);
    u_xlat2.x = dot(u_xlat10.xy, float2(127.099998, 311.700012));
    u_xlat3 = u_xlat10.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlat1.x = sin(u_xlat2.x);
    u_xlat10.x = dot(u_xlat3.xy, float2(127.099998, 311.700012));
    u_xlat10.y = dot(u_xlat3.zw, float2(127.099998, 311.700012));
    u_xlat1.yz = sin(u_xlat10.xy);
    u_xlat1 = u_xlat1 * float4(43758.5469, 43758.5469, 43758.5469, 43758.5469);
    u_xlat1 = frac(u_xlat1);
    u_xlat10.xy = (-u_xlat1.xz) + u_xlat1.yw;
    u_xlat6.xz = u_xlat0.xy * u_xlat0.xy;
    u_xlat0.xy = (-u_xlat0.xy) * float2(2.0, 2.0) + float2(3.0, 3.0);
    u_xlat0.xy = u_xlat0.xy * u_xlat6.xz;
    u_xlat0.xz = u_xlat0.xx * u_xlat10.xy + u_xlat1.xz;
    u_xlat10.x = (-u_xlat0.x) + u_xlat0.z;
    u_xlat0.x = u_xlat0.y * u_xlat10.x + u_xlat0.x;
    u_xlat0.x = u_xlat0.x * 0.349999994;
    u_xlat5.xy = vs_TEXCOORD0.xy * float2(7.0, 7.0);
    u_xlat1.xy = floor(u_xlat5.xy);
    u_xlat5.xy = frac(u_xlat5.xy);
    u_xlat11.xy = u_xlat1.xy + float2(1.0, 1.0);
    u_xlat15 = dot(u_xlat11.xy, float2(127.099998, 311.700012));
    u_xlat2.w = sin(u_xlat15);
    u_xlat15 = dot(u_xlat1.xy, float2(127.099998, 311.700012));
    u_xlat1 = u_xlat1.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlat2.x = sin(u_xlat15);
    u_xlat15 = dot(u_xlat1.xy, float2(127.099998, 311.700012));
    u_xlat1.x = dot(u_xlat1.zw, float2(127.099998, 311.700012));
    u_xlat2.z = sin(u_xlat1.x);
    u_xlat2.y = sin(u_xlat15);
    u_xlat1 = u_xlat2 * float4(43758.5469, 43758.5469, 43758.5469, 43758.5469);
    u_xlat1 = frac(u_xlat1);
    u_xlat6.xz = (-u_xlat1.xz) + u_xlat1.yw;
    u_xlat2.xy = u_xlat5.xy * u_xlat5.xy;
    u_xlat5.xy = (-u_xlat5.xy) * float2(2.0, 2.0) + float2(3.0, 3.0);
    u_xlat5.xy = u_xlat5.xy * u_xlat2.xy;
    u_xlat5.xz = u_xlat5.xx * u_xlat6.xz + u_xlat1.xz;
    u_xlat15 = (-u_xlat5.x) + u_xlat5.z;
    u_xlat5.x = u_xlat5.y * u_xlat15 + u_xlat5.x;
    u_xlat0.x = u_xlat5.x * 0.649999976 + u_xlat0.x;
    u_xlat0.x = u_xlat0.x + -0.5;
    u_xlat5.xy = vs_TEXCOORD0.xy + (-_BlackCoreCenter.xy);
    u_xlat1.xy = max(_BlackCoreRadii.xy, float2(9.99999975e-06, 9.99999975e-06));
    u_xlat5.xy = u_xlat5.xy / u_xlat1.xy;
    u_xlat5.x = dot(u_xlat5.xy, u_xlat5.xy);
    u_xlat5.x = sqrt(u_xlat5.x);
    u_xlat0.x = u_xlat0.x * _BlackCoreIrregularity + u_xlat5.x;
    u_xlat0.x = u_xlat0.x + -1.0;
    u_xlat5.x = max(_BlackCoreFeather, 9.99999975e-06);
    u_xlat5.x = float(1.0) / u_xlat5.x;
    u_xlat0.x = u_xlat5.x * u_xlat0.x;
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat5.x = u_xlat0.x * -2.0 + 3.0;
    u_xlat0.x = u_xlat0.x * u_xlat0.x;
    u_xlat0.x = (-u_xlat5.x) * u_xlat0.x + 1.0;
    u_xlat5.x = (-_BlackCoreStrength) * u_xlat0.x + 1.0;
    u_xlat10.xy = vs_TEXCOORD0.xy + float2(-0.5, -0.5);
    u_xlat10.xy = u_xlat10.xy * _FogUVScale.xy + float2(0.5, 0.5);
    u_xlat16_1 = tex2D(_MainTex, u_xlat10.xy);
    u_xlat10.x = max(u_xlat16_1.z, u_xlat16_1.y);
    u_xlat10.x = max(u_xlat10.x, u_xlat16_1.x);
    u_xlat2.xy = max(float2(_ClearThreshold, _EdgeFeather), float2(9.99999975e-06, 9.99999975e-06));
    u_xlat2.xy = float2(1.0, 1.0) / u_xlat2.xy;
    u_xlat10.x = u_xlat10.x * u_xlat2.x;
    u_xlat10.x = clamp(u_xlat10.x, 0.0, 1.0);
    u_xlat15 = u_xlat10.x * -2.0 + 3.0;
    u_xlat10.x = u_xlat10.x * u_xlat10.x;
    u_xlat10.x = (-u_xlat15) * u_xlat10.x + 1.0;
    u_xlat10.x = u_xlat10.x * u_xlat10.x;
    u_xlat10.x = u_xlat10.x * u_xlat16_1.w;
    u_xlat5.x = u_xlat5.x * u_xlat10.x;
    u_xlat0.x = _BlackCoreStrength * u_xlat0.x + u_xlat5.x;
    u_xlat10.xy = (-vs_TEXCOORD0.xy) + float2(1.0, 1.0);
    u_xlat10.xy = min(u_xlat10.xy, vs_TEXCOORD0.xy);
    u_xlat10.x = min(u_xlat10.y, u_xlat10.x);
    u_xlat10.x = u_xlat2.y * u_xlat10.x;
    u_xlat10.x = clamp(u_xlat10.x, 0.0, 1.0);
    u_xlat15 = u_xlat10.x * -2.0 + 3.0;
    u_xlat10.x = u_xlat10.x * u_xlat10.x;
    u_xlat10.x = u_xlat10.x * u_xlat15;
    u_xlat10.x = u_xlat10.x * vs_COLOR0.w;
    u_xlat16_4.x = u_xlat0.x * u_xlat10.x + -0.00100000005;
    u_xlat10.x = u_xlat10.x * u_xlat0.x;
    u_xlat16_9 = max(u_xlat0.x, 9.99999975e-06);
    u_xlat0.x = u_xlat5.x / u_xlat16_9;
    SV_Target0.w = u_xlat10.x;
    u_xlatb5 = u_xlat16_4.x<0.0;
    if(u_xlatb5){discard;}
    u_xlat16_4.xyz = (-u_xlat16_1.xyz) + _FogTint.xyz;
    u_xlat16_4.xyz = _FogTint.www * u_xlat16_4.xyz + u_xlat16_1.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat16_4.xyz;
    SV_Target0.xyz = u_xlat0.xyz * vs_COLOR0.xyz;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(UNITY_UI_CLIP_RECT) && !defined(UNITY_UI_ALPHACLIP)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_COLOR0 : COLOR0;
	float2 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_COLOR0 : COLOR0;
	float2 vs_TEXCOORD0 : TEXCOORD0;
	float4 vs_TEXCOORD1 : TEXCOORD1;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_COLOR0 = genIn.in_COLOR0;
	float2 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_COLOR0 = (float4)0;
	float2 vs_TEXCOORD0 = (float2)0;
	float4 vs_TEXCOORD1 = (float4)0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat1 = MCOL(unity_MatrixVP,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_MatrixVP,2) * u_xlat0.zzzz + u_xlat1;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat0.wwww + u_xlat1;
    vs_COLOR0 = in_COLOR0;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy;
    vs_TEXCOORD1 = in_POSITION0;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_COLOR0 = vs_COLOR0;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float2 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float3 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat16_1;
	bool2 u_xlatb1;
	float4 u_xlat2;
	float4 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat5;
	bool2 u_xlatb5;
	float3 u_xlat6;
	float2 u_xlat10;
	float2 u_xlat11;
	float u_xlat15;
	float4 SV_Target0 = 0;

    u_xlat0.xy = vs_TEXCOORD0.xy * float2(13.0, 13.0) + float2(4.69999981, 9.19999981);
    u_xlat10.xy = floor(u_xlat0.xy);
    u_xlat0.xy = frac(u_xlat0.xy);
    u_xlat1.xy = u_xlat10.xy + float2(1.0, 1.0);
    u_xlat1.x = dot(u_xlat1.xy, float2(127.099998, 311.700012));
    u_xlat1.w = sin(u_xlat1.x);
    u_xlat2.x = dot(u_xlat10.xy, float2(127.099998, 311.700012));
    u_xlat3 = u_xlat10.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlat1.x = sin(u_xlat2.x);
    u_xlat10.x = dot(u_xlat3.xy, float2(127.099998, 311.700012));
    u_xlat10.y = dot(u_xlat3.zw, float2(127.099998, 311.700012));
    u_xlat1.yz = sin(u_xlat10.xy);
    u_xlat1 = u_xlat1 * float4(43758.5469, 43758.5469, 43758.5469, 43758.5469);
    u_xlat1 = frac(u_xlat1);
    u_xlat10.xy = (-u_xlat1.xz) + u_xlat1.yw;
    u_xlat6.xz = u_xlat0.xy * u_xlat0.xy;
    u_xlat0.xy = (-u_xlat0.xy) * float2(2.0, 2.0) + float2(3.0, 3.0);
    u_xlat0.xy = u_xlat0.xy * u_xlat6.xz;
    u_xlat0.xz = u_xlat0.xx * u_xlat10.xy + u_xlat1.xz;
    u_xlat10.x = (-u_xlat0.x) + u_xlat0.z;
    u_xlat0.x = u_xlat0.y * u_xlat10.x + u_xlat0.x;
    u_xlat0.x = u_xlat0.x * 0.349999994;
    u_xlat5.xy = vs_TEXCOORD0.xy * float2(7.0, 7.0);
    u_xlat1.xy = floor(u_xlat5.xy);
    u_xlat5.xy = frac(u_xlat5.xy);
    u_xlat11.xy = u_xlat1.xy + float2(1.0, 1.0);
    u_xlat15 = dot(u_xlat11.xy, float2(127.099998, 311.700012));
    u_xlat2.w = sin(u_xlat15);
    u_xlat15 = dot(u_xlat1.xy, float2(127.099998, 311.700012));
    u_xlat1 = u_xlat1.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlat2.x = sin(u_xlat15);
    u_xlat15 = dot(u_xlat1.xy, float2(127.099998, 311.700012));
    u_xlat1.x = dot(u_xlat1.zw, float2(127.099998, 311.700012));
    u_xlat2.z = sin(u_xlat1.x);
    u_xlat2.y = sin(u_xlat15);
    u_xlat1 = u_xlat2 * float4(43758.5469, 43758.5469, 43758.5469, 43758.5469);
    u_xlat1 = frac(u_xlat1);
    u_xlat6.xz = (-u_xlat1.xz) + u_xlat1.yw;
    u_xlat2.xy = u_xlat5.xy * u_xlat5.xy;
    u_xlat5.xy = (-u_xlat5.xy) * float2(2.0, 2.0) + float2(3.0, 3.0);
    u_xlat5.xy = u_xlat5.xy * u_xlat2.xy;
    u_xlat5.xz = u_xlat5.xx * u_xlat6.xz + u_xlat1.xz;
    u_xlat15 = (-u_xlat5.x) + u_xlat5.z;
    u_xlat5.x = u_xlat5.y * u_xlat15 + u_xlat5.x;
    u_xlat0.x = u_xlat5.x * 0.649999976 + u_xlat0.x;
    u_xlat0.x = u_xlat0.x + -0.5;
    u_xlat5.xy = vs_TEXCOORD0.xy + (-_BlackCoreCenter.xy);
    u_xlat1.xy = max(_BlackCoreRadii.xy, float2(9.99999975e-06, 9.99999975e-06));
    u_xlat5.xy = u_xlat5.xy / u_xlat1.xy;
    u_xlat5.x = dot(u_xlat5.xy, u_xlat5.xy);
    u_xlat5.x = sqrt(u_xlat5.x);
    u_xlat0.x = u_xlat0.x * _BlackCoreIrregularity + u_xlat5.x;
    u_xlat0.x = u_xlat0.x + -1.0;
    u_xlat5.x = max(_BlackCoreFeather, 9.99999975e-06);
    u_xlat5.x = float(1.0) / u_xlat5.x;
    u_xlat0.x = u_xlat5.x * u_xlat0.x;
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat5.x = u_xlat0.x * -2.0 + 3.0;
    u_xlat0.x = u_xlat0.x * u_xlat0.x;
    u_xlat0.x = (-u_xlat5.x) * u_xlat0.x + 1.0;
    u_xlat5.x = (-_BlackCoreStrength) * u_xlat0.x + 1.0;
    u_xlat10.xy = vs_TEXCOORD0.xy + float2(-0.5, -0.5);
    u_xlat10.xy = u_xlat10.xy * _FogUVScale.xy + float2(0.5, 0.5);
    u_xlat16_1 = tex2D(_MainTex, u_xlat10.xy);
    u_xlat10.x = max(u_xlat16_1.z, u_xlat16_1.y);
    u_xlat10.x = max(u_xlat10.x, u_xlat16_1.x);
    u_xlat2.xy = max(float2(_ClearThreshold, _EdgeFeather), float2(9.99999975e-06, 9.99999975e-06));
    u_xlat2.xy = float2(1.0, 1.0) / u_xlat2.xy;
    u_xlat10.x = u_xlat10.x * u_xlat2.x;
    u_xlat10.x = clamp(u_xlat10.x, 0.0, 1.0);
    u_xlat15 = u_xlat10.x * -2.0 + 3.0;
    u_xlat10.x = u_xlat10.x * u_xlat10.x;
    u_xlat10.x = (-u_xlat15) * u_xlat10.x + 1.0;
    u_xlat10.x = u_xlat10.x * u_xlat10.x;
    u_xlat10.x = u_xlat10.x * u_xlat16_1.w;
    u_xlat5.x = u_xlat5.x * u_xlat10.x;
    u_xlat0.x = _BlackCoreStrength * u_xlat0.x + u_xlat5.x;
    u_xlat16_4.x = max(u_xlat0.x, 9.99999975e-06);
    u_xlat5.x = u_xlat5.x / u_xlat16_4.x;
    u_xlat16_4.xyz = (-u_xlat16_1.xyz) + _FogTint.xyz;
    u_xlat16_4.xyz = _FogTint.www * u_xlat16_4.xyz + u_xlat16_1.xyz;
    u_xlat5.xyz = u_xlat5.xxx * u_xlat16_4.xyz;
    SV_Target0.xyz = u_xlat5.xyz * vs_COLOR0.xyz;
    u_xlat5.xy = (-vs_TEXCOORD0.xy) + float2(1.0, 1.0);
    u_xlat5.xy = min(u_xlat5.xy, vs_TEXCOORD0.xy);
    u_xlat5.x = min(u_xlat5.y, u_xlat5.x);
    u_xlat5.x = u_xlat2.y * u_xlat5.x;
    u_xlat5.x = clamp(u_xlat5.x, 0.0, 1.0);
    u_xlat10.x = u_xlat5.x * -2.0 + 3.0;
    u_xlat5.x = u_xlat5.x * u_xlat5.x;
    u_xlat5.x = u_xlat5.x * u_xlat10.x;
    u_xlat5.x = u_xlat5.x * vs_COLOR0.w;
    u_xlat0.x = u_xlat5.x * u_xlat0.x;
    u_xlatb5.xy = GEQ(vs_TEXCOORD1.xyxx, _ClipRect.xyxx).xy;
    u_xlat5.x = u_xlatb5.x ? float(1.0) : 0.0;
    u_xlat5.y = u_xlatb5.y ? float(1.0) : 0.0;
;
    u_xlatb1.xy = GEQ(_ClipRect.zwzz, vs_TEXCOORD1.xyxx).xy;
    u_xlat1.x = u_xlatb1.x ? float(1.0) : 0.0;
    u_xlat1.y = u_xlatb1.y ? float(1.0) : 0.0;
;
    u_xlat5.xy = u_xlat5.xy * u_xlat1.xy;
    u_xlat5.x = u_xlat5.y * u_xlat5.x;
    u_xlat0.x = u_xlat5.x * u_xlat0.x;
    SV_Target0.w = u_xlat0.x;
    return SV_Target0;

	return SV_Target0;
}
#elif !defined(UNITY_UI_ALPHACLIP) && !defined(UNITY_UI_CLIP_RECT)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_COLOR0 : COLOR0;
	float2 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_COLOR0 : COLOR0;
	float2 vs_TEXCOORD0 : TEXCOORD0;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_COLOR0 = genIn.in_COLOR0;
	float2 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_COLOR0 = (float4)0;
	float2 vs_TEXCOORD0 = (float2)0;
	float4 vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat1 = MCOL(unity_MatrixVP,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_MatrixVP,2) * u_xlat0.zzzz + u_xlat1;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat0.wwww + u_xlat1;
    vs_COLOR0 = in_COLOR0;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy;
    vs_TEXCOORD1 = in_POSITION0;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_COLOR0 = vs_COLOR0;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float2 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float3 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat16_1;
	float4 u_xlat2;
	float4 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat5;
	float3 u_xlat6;
	float2 u_xlat10;
	float2 u_xlat11;
	float u_xlat15;
	float4 SV_Target0 = 0;

    u_xlat0.xy = vs_TEXCOORD0.xy * float2(13.0, 13.0) + float2(4.69999981, 9.19999981);
    u_xlat10.xy = floor(u_xlat0.xy);
    u_xlat0.xy = frac(u_xlat0.xy);
    u_xlat1.xy = u_xlat10.xy + float2(1.0, 1.0);
    u_xlat1.x = dot(u_xlat1.xy, float2(127.099998, 311.700012));
    u_xlat1.w = sin(u_xlat1.x);
    u_xlat2.x = dot(u_xlat10.xy, float2(127.099998, 311.700012));
    u_xlat3 = u_xlat10.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlat1.x = sin(u_xlat2.x);
    u_xlat10.x = dot(u_xlat3.xy, float2(127.099998, 311.700012));
    u_xlat10.y = dot(u_xlat3.zw, float2(127.099998, 311.700012));
    u_xlat1.yz = sin(u_xlat10.xy);
    u_xlat1 = u_xlat1 * float4(43758.5469, 43758.5469, 43758.5469, 43758.5469);
    u_xlat1 = frac(u_xlat1);
    u_xlat10.xy = (-u_xlat1.xz) + u_xlat1.yw;
    u_xlat6.xz = u_xlat0.xy * u_xlat0.xy;
    u_xlat0.xy = (-u_xlat0.xy) * float2(2.0, 2.0) + float2(3.0, 3.0);
    u_xlat0.xy = u_xlat0.xy * u_xlat6.xz;
    u_xlat0.xz = u_xlat0.xx * u_xlat10.xy + u_xlat1.xz;
    u_xlat10.x = (-u_xlat0.x) + u_xlat0.z;
    u_xlat0.x = u_xlat0.y * u_xlat10.x + u_xlat0.x;
    u_xlat0.x = u_xlat0.x * 0.349999994;
    u_xlat5.xy = vs_TEXCOORD0.xy * float2(7.0, 7.0);
    u_xlat1.xy = floor(u_xlat5.xy);
    u_xlat5.xy = frac(u_xlat5.xy);
    u_xlat11.xy = u_xlat1.xy + float2(1.0, 1.0);
    u_xlat15 = dot(u_xlat11.xy, float2(127.099998, 311.700012));
    u_xlat2.w = sin(u_xlat15);
    u_xlat15 = dot(u_xlat1.xy, float2(127.099998, 311.700012));
    u_xlat1 = u_xlat1.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlat2.x = sin(u_xlat15);
    u_xlat15 = dot(u_xlat1.xy, float2(127.099998, 311.700012));
    u_xlat1.x = dot(u_xlat1.zw, float2(127.099998, 311.700012));
    u_xlat2.z = sin(u_xlat1.x);
    u_xlat2.y = sin(u_xlat15);
    u_xlat1 = u_xlat2 * float4(43758.5469, 43758.5469, 43758.5469, 43758.5469);
    u_xlat1 = frac(u_xlat1);
    u_xlat6.xz = (-u_xlat1.xz) + u_xlat1.yw;
    u_xlat2.xy = u_xlat5.xy * u_xlat5.xy;
    u_xlat5.xy = (-u_xlat5.xy) * float2(2.0, 2.0) + float2(3.0, 3.0);
    u_xlat5.xy = u_xlat5.xy * u_xlat2.xy;
    u_xlat5.xz = u_xlat5.xx * u_xlat6.xz + u_xlat1.xz;
    u_xlat15 = (-u_xlat5.x) + u_xlat5.z;
    u_xlat5.x = u_xlat5.y * u_xlat15 + u_xlat5.x;
    u_xlat0.x = u_xlat5.x * 0.649999976 + u_xlat0.x;
    u_xlat0.x = u_xlat0.x + -0.5;
    u_xlat5.xy = vs_TEXCOORD0.xy + (-_BlackCoreCenter.xy);
    u_xlat1.xy = max(_BlackCoreRadii.xy, float2(9.99999975e-06, 9.99999975e-06));
    u_xlat5.xy = u_xlat5.xy / u_xlat1.xy;
    u_xlat5.x = dot(u_xlat5.xy, u_xlat5.xy);
    u_xlat5.x = sqrt(u_xlat5.x);
    u_xlat0.x = u_xlat0.x * _BlackCoreIrregularity + u_xlat5.x;
    u_xlat0.x = u_xlat0.x + -1.0;
    u_xlat5.x = max(_BlackCoreFeather, 9.99999975e-06);
    u_xlat5.x = float(1.0) / u_xlat5.x;
    u_xlat0.x = u_xlat5.x * u_xlat0.x;
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat5.x = u_xlat0.x * -2.0 + 3.0;
    u_xlat0.x = u_xlat0.x * u_xlat0.x;
    u_xlat0.x = (-u_xlat5.x) * u_xlat0.x + 1.0;
    u_xlat5.x = (-_BlackCoreStrength) * u_xlat0.x + 1.0;
    u_xlat10.xy = vs_TEXCOORD0.xy + float2(-0.5, -0.5);
    u_xlat10.xy = u_xlat10.xy * _FogUVScale.xy + float2(0.5, 0.5);
    u_xlat16_1 = tex2D(_MainTex, u_xlat10.xy);
    u_xlat10.x = max(u_xlat16_1.z, u_xlat16_1.y);
    u_xlat10.x = max(u_xlat10.x, u_xlat16_1.x);
    u_xlat2.xy = max(float2(_ClearThreshold, _EdgeFeather), float2(9.99999975e-06, 9.99999975e-06));
    u_xlat2.xy = float2(1.0, 1.0) / u_xlat2.xy;
    u_xlat10.x = u_xlat10.x * u_xlat2.x;
    u_xlat10.x = clamp(u_xlat10.x, 0.0, 1.0);
    u_xlat15 = u_xlat10.x * -2.0 + 3.0;
    u_xlat10.x = u_xlat10.x * u_xlat10.x;
    u_xlat10.x = (-u_xlat15) * u_xlat10.x + 1.0;
    u_xlat10.x = u_xlat10.x * u_xlat10.x;
    u_xlat10.x = u_xlat10.x * u_xlat16_1.w;
    u_xlat5.x = u_xlat5.x * u_xlat10.x;
    u_xlat0.x = _BlackCoreStrength * u_xlat0.x + u_xlat5.x;
    u_xlat16_4.x = max(u_xlat0.x, 9.99999975e-06);
    u_xlat5.x = u_xlat5.x / u_xlat16_4.x;
    u_xlat16_4.xyz = (-u_xlat16_1.xyz) + _FogTint.xyz;
    u_xlat16_4.xyz = _FogTint.www * u_xlat16_4.xyz + u_xlat16_1.xyz;
    u_xlat5.xyz = u_xlat5.xxx * u_xlat16_4.xyz;
    SV_Target0.xyz = u_xlat5.xyz * vs_COLOR0.xyz;
    u_xlat5.xy = (-vs_TEXCOORD0.xy) + float2(1.0, 1.0);
    u_xlat5.xy = min(u_xlat5.xy, vs_TEXCOORD0.xy);
    u_xlat5.x = min(u_xlat5.y, u_xlat5.x);
    u_xlat5.x = u_xlat2.y * u_xlat5.x;
    u_xlat5.x = clamp(u_xlat5.x, 0.0, 1.0);
    u_xlat10.x = u_xlat5.x * -2.0 + 3.0;
    u_xlat5.x = u_xlat5.x * u_xlat5.x;
    u_xlat5.x = u_xlat5.x * u_xlat10.x;
    u_xlat5.x = u_xlat5.x * vs_COLOR0.w;
    u_xlat0.x = u_xlat5.x * u_xlat0.x;
    SV_Target0.w = u_xlat0.x;
    return SV_Target0;

	return SV_Target0;
}
#else
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_COLOR0 : COLOR0;
	float2 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_COLOR0 : COLOR0;
	float2 vs_TEXCOORD0 : TEXCOORD0;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_COLOR0 = genIn.in_COLOR0;
	float2 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_COLOR0 = (float4)0;
	float2 vs_TEXCOORD0 = (float2)0;
	float4 vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat1 = MCOL(unity_MatrixVP,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_MatrixVP,2) * u_xlat0.zzzz + u_xlat1;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat0.wwww + u_xlat1;
    vs_COLOR0 = in_COLOR0;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy;
    vs_TEXCOORD1 = in_POSITION0;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_COLOR0 = vs_COLOR0;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float2 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float3 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat16_1;
	float4 u_xlat2;
	float4 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat5;
	float3 u_xlat6;
	float2 u_xlat10;
	float2 u_xlat11;
	float u_xlat15;
	float4 SV_Target0 = 0;

    u_xlat0.xy = vs_TEXCOORD0.xy * float2(13.0, 13.0) + float2(4.69999981, 9.19999981);
    u_xlat10.xy = floor(u_xlat0.xy);
    u_xlat0.xy = frac(u_xlat0.xy);
    u_xlat1.xy = u_xlat10.xy + float2(1.0, 1.0);
    u_xlat1.x = dot(u_xlat1.xy, float2(127.099998, 311.700012));
    u_xlat1.w = sin(u_xlat1.x);
    u_xlat2.x = dot(u_xlat10.xy, float2(127.099998, 311.700012));
    u_xlat3 = u_xlat10.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlat1.x = sin(u_xlat2.x);
    u_xlat10.x = dot(u_xlat3.xy, float2(127.099998, 311.700012));
    u_xlat10.y = dot(u_xlat3.zw, float2(127.099998, 311.700012));
    u_xlat1.yz = sin(u_xlat10.xy);
    u_xlat1 = u_xlat1 * float4(43758.5469, 43758.5469, 43758.5469, 43758.5469);
    u_xlat1 = frac(u_xlat1);
    u_xlat10.xy = (-u_xlat1.xz) + u_xlat1.yw;
    u_xlat6.xz = u_xlat0.xy * u_xlat0.xy;
    u_xlat0.xy = (-u_xlat0.xy) * float2(2.0, 2.0) + float2(3.0, 3.0);
    u_xlat0.xy = u_xlat0.xy * u_xlat6.xz;
    u_xlat0.xz = u_xlat0.xx * u_xlat10.xy + u_xlat1.xz;
    u_xlat10.x = (-u_xlat0.x) + u_xlat0.z;
    u_xlat0.x = u_xlat0.y * u_xlat10.x + u_xlat0.x;
    u_xlat0.x = u_xlat0.x * 0.349999994;
    u_xlat5.xy = vs_TEXCOORD0.xy * float2(7.0, 7.0);
    u_xlat1.xy = floor(u_xlat5.xy);
    u_xlat5.xy = frac(u_xlat5.xy);
    u_xlat11.xy = u_xlat1.xy + float2(1.0, 1.0);
    u_xlat15 = dot(u_xlat11.xy, float2(127.099998, 311.700012));
    u_xlat2.w = sin(u_xlat15);
    u_xlat15 = dot(u_xlat1.xy, float2(127.099998, 311.700012));
    u_xlat1 = u_xlat1.xyxy + float4(1.0, 0.0, 0.0, 1.0);
    u_xlat2.x = sin(u_xlat15);
    u_xlat15 = dot(u_xlat1.xy, float2(127.099998, 311.700012));
    u_xlat1.x = dot(u_xlat1.zw, float2(127.099998, 311.700012));
    u_xlat2.z = sin(u_xlat1.x);
    u_xlat2.y = sin(u_xlat15);
    u_xlat1 = u_xlat2 * float4(43758.5469, 43758.5469, 43758.5469, 43758.5469);
    u_xlat1 = frac(u_xlat1);
    u_xlat6.xz = (-u_xlat1.xz) + u_xlat1.yw;
    u_xlat2.xy = u_xlat5.xy * u_xlat5.xy;
    u_xlat5.xy = (-u_xlat5.xy) * float2(2.0, 2.0) + float2(3.0, 3.0);
    u_xlat5.xy = u_xlat5.xy * u_xlat2.xy;
    u_xlat5.xz = u_xlat5.xx * u_xlat6.xz + u_xlat1.xz;
    u_xlat15 = (-u_xlat5.x) + u_xlat5.z;
    u_xlat5.x = u_xlat5.y * u_xlat15 + u_xlat5.x;
    u_xlat0.x = u_xlat5.x * 0.649999976 + u_xlat0.x;
    u_xlat0.x = u_xlat0.x + -0.5;
    u_xlat5.xy = vs_TEXCOORD0.xy + (-_BlackCoreCenter.xy);
    u_xlat1.xy = max(_BlackCoreRadii.xy, float2(9.99999975e-06, 9.99999975e-06));
    u_xlat5.xy = u_xlat5.xy / u_xlat1.xy;
    u_xlat5.x = dot(u_xlat5.xy, u_xlat5.xy);
    u_xlat5.x = sqrt(u_xlat5.x);
    u_xlat0.x = u_xlat0.x * _BlackCoreIrregularity + u_xlat5.x;
    u_xlat0.x = u_xlat0.x + -1.0;
    u_xlat5.x = max(_BlackCoreFeather, 9.99999975e-06);
    u_xlat5.x = float(1.0) / u_xlat5.x;
    u_xlat0.x = u_xlat5.x * u_xlat0.x;
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat5.x = u_xlat0.x * -2.0 + 3.0;
    u_xlat0.x = u_xlat0.x * u_xlat0.x;
    u_xlat0.x = (-u_xlat5.x) * u_xlat0.x + 1.0;
    u_xlat5.x = (-_BlackCoreStrength) * u_xlat0.x + 1.0;
    u_xlat10.xy = vs_TEXCOORD0.xy + float2(-0.5, -0.5);
    u_xlat10.xy = u_xlat10.xy * _FogUVScale.xy + float2(0.5, 0.5);
    u_xlat16_1 = tex2D(_MainTex, u_xlat10.xy);
    u_xlat10.x = max(u_xlat16_1.z, u_xlat16_1.y);
    u_xlat10.x = max(u_xlat10.x, u_xlat16_1.x);
    u_xlat2.xy = max(float2(_ClearThreshold, _EdgeFeather), float2(9.99999975e-06, 9.99999975e-06));
    u_xlat2.xy = float2(1.0, 1.0) / u_xlat2.xy;
    u_xlat10.x = u_xlat10.x * u_xlat2.x;
    u_xlat10.x = clamp(u_xlat10.x, 0.0, 1.0);
    u_xlat15 = u_xlat10.x * -2.0 + 3.0;
    u_xlat10.x = u_xlat10.x * u_xlat10.x;
    u_xlat10.x = (-u_xlat15) * u_xlat10.x + 1.0;
    u_xlat10.x = u_xlat10.x * u_xlat10.x;
    u_xlat10.x = u_xlat10.x * u_xlat16_1.w;
    u_xlat5.x = u_xlat5.x * u_xlat10.x;
    u_xlat0.x = _BlackCoreStrength * u_xlat0.x + u_xlat5.x;
    u_xlat16_4.x = max(u_xlat0.x, 9.99999975e-06);
    u_xlat5.x = u_xlat5.x / u_xlat16_4.x;
    u_xlat16_4.xyz = (-u_xlat16_1.xyz) + _FogTint.xyz;
    u_xlat16_4.xyz = _FogTint.www * u_xlat16_4.xyz + u_xlat16_1.xyz;
    u_xlat5.xyz = u_xlat5.xxx * u_xlat16_4.xyz;
    SV_Target0.xyz = u_xlat5.xyz * vs_COLOR0.xyz;
    u_xlat5.xy = (-vs_TEXCOORD0.xy) + float2(1.0, 1.0);
    u_xlat5.xy = min(u_xlat5.xy, vs_TEXCOORD0.xy);
    u_xlat5.x = min(u_xlat5.y, u_xlat5.x);
    u_xlat5.x = u_xlat2.y * u_xlat5.x;
    u_xlat5.x = clamp(u_xlat5.x, 0.0, 1.0);
    u_xlat10.x = u_xlat5.x * -2.0 + 3.0;
    u_xlat5.x = u_xlat5.x * u_xlat5.x;
    u_xlat5.x = u_xlat5.x * u_xlat10.x;
    u_xlat5.x = u_xlat5.x * vs_COLOR0.w;
    u_xlat0.x = u_xlat5.x * u_xlat0.x;
    SV_Target0.w = u_xlat0.x;
    return SV_Target0;

	return SV_Target0;
}
#endif
ENDHLSL
		}
	}
}

Shader "TextMeshPro/Sprite" {
	Properties {
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1.0,1.0,1.0,1.0)
		_StencilComp ("Stencil Comparison", Float) = 8.0
		_Stencil ("Stencil ID", Float) = 0.0
		_StencilOp ("Stencil Operation", Float) = 0.0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255.0
		_StencilReadMask ("Stencil Read Mask", Float) = 255.0
		_CullMode ("Cull Mode", Float) = 0.0
		_ColorMask ("Color Mask", Float) = 15.0
		_ClipRect ("Clip Rect", Vector) = (-32767.0,-32767.0,32767.0,32767.0)
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0.0
	}
	SubShader {
		Tags { "CanUseSpriteAtlas"="true" "IgnoreProjector"="true" "PreviewType"="Plane" "Queue"="Transparent" "RenderType"="Transparent" }
		Pass {
			Name "Default"
			Tags { "CanUseSpriteAtlas"="true" "IgnoreProjector"="true" "PreviewType"="Plane" "Queue"="Transparent" "RenderType"="Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask [_ColorMask]
			ZWrite Off
			ZTest [unity_GUIZTestMode]
			Cull [_CullMode]
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
float4 _ClipRect;
float4 _Color;
float4 _MainTex_ST;
float4 _TextureSampleAdd;
float _UIMaskSoftnessX;
float _UIMaskSoftnessY;
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
	float4 vs_TEXCOORD2 : TEXCOORD2;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_COLOR0 = genIn.in_COLOR0;
	float2 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_COLOR0 = (float4)0;
	float2 vs_TEXCOORD0 = (float2)0;
	float4 vs_TEXCOORD2 = (float4)0;
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
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat0.wwww + u_xlat1;
    gl_Position = u_xlat0;
    u_xlat1 = in_COLOR0 * _Color;
    vs_COLOR0 = u_xlat1;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD1 = in_POSITION0;
    u_xlat0.xy = _ScreenParams.yy * MCOL(glstate_matrix_projection,1).xy;
    u_xlat0.xy = MCOL(glstate_matrix_projection,0).xy * _ScreenParams.xx + u_xlat0.xy;
    u_xlat0.xy = u_xlat0.ww / abs(u_xlat0.xy);
    u_xlat0.xy = float2(_UIMaskSoftnessX, _UIMaskSoftnessY) * float2(0.25, 0.25) + abs(u_xlat0.xy);
    vs_TEXCOORD2.zw = float2(0.25, 0.25) / u_xlat0.xy;
    u_xlat0 = max(_ClipRect, float4(-2e+10, -2e+10, -2e+10, -2e+10));
    u_xlat0 = min(u_xlat0, float4(2e+10, 2e+10, 2e+10, 2e+10));
    u_xlat0.xy = in_POSITION0.xy * float2(2.0, 2.0) + (-u_xlat0.xy);
    vs_TEXCOORD2.xy = (-u_xlat0.zw) + u_xlat0.xy;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_COLOR0 = vs_COLOR0;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float2 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float4 u_xlat0;
	float4 u_xlat16_0;
	float u_xlat16_1;
	bool u_xlatb2;
	float u_xlat16_4;
	float4 SV_Target0 = 0;

    u_xlat0.xy = (-_ClipRect.xy) + _ClipRect.zw;
    u_xlat0.xy = u_xlat0.xy + -abs(vs_TEXCOORD2.xy);
    u_xlat0.xy = u_xlat0.xy * vs_TEXCOORD2.zw;
    u_xlat0.xy = clamp(u_xlat0.xy, 0.0, 1.0);
    u_xlat16_1 = u_xlat0.y * u_xlat0.x;
    u_xlat16_0 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat0 = u_xlat16_0 + _TextureSampleAdd;
    u_xlat0 = u_xlat0 * vs_COLOR0;
    u_xlat16_4 = u_xlat0.w * u_xlat16_1 + -0.00100000005;
    u_xlat16_0 = ((float4)(u_xlat16_1)) * u_xlat0;
    SV_Target0 = u_xlat16_0;
    u_xlatb2 = u_xlat16_4<0.0;
    if(u_xlatb2){discard;}
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
	float4 vs_TEXCOORD2;
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
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat0.wwww + u_xlat1;
    gl_Position = u_xlat0;
    u_xlat1 = in_COLOR0 * _Color;
    vs_COLOR0 = u_xlat1;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD1 = in_POSITION0;
    u_xlat0.xy = _ScreenParams.yy * MCOL(glstate_matrix_projection,1).xy;
    u_xlat0.xy = MCOL(glstate_matrix_projection,0).xy * _ScreenParams.xx + u_xlat0.xy;
    u_xlat0.xy = u_xlat0.ww / abs(u_xlat0.xy);
    u_xlat0.xy = float2(_UIMaskSoftnessX, _UIMaskSoftnessY) * float2(0.25, 0.25) + abs(u_xlat0.xy);
    vs_TEXCOORD2.zw = float2(0.25, 0.25) / u_xlat0.xy;
    u_xlat0 = max(_ClipRect, float4(-2e+10, -2e+10, -2e+10, -2e+10));
    u_xlat0 = min(u_xlat0, float4(2e+10, 2e+10, 2e+10, 2e+10));
    u_xlat0.xy = in_POSITION0.xy * float2(2.0, 2.0) + (-u_xlat0.xy);
    vs_TEXCOORD2.xy = (-u_xlat0.zw) + u_xlat0.xy;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_COLOR0 = vs_COLOR0;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float2 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 u_xlat0;
	float4 u_xlat16_0;
	bool u_xlatb0;
	float u_xlat16_1;
	float4 SV_Target0 = 0;

    u_xlat16_0 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat0 = u_xlat16_0 + _TextureSampleAdd;
    u_xlat16_1 = u_xlat0.w * vs_COLOR0.w + -0.00100000005;
    u_xlat0 = u_xlat0 * vs_COLOR0;
    SV_Target0 = u_xlat0;
    u_xlatb0 = u_xlat16_1<0.0;
    if(u_xlatb0){discard;}
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
	float4 vs_TEXCOORD2 : TEXCOORD2;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_COLOR0 = genIn.in_COLOR0;
	float2 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_COLOR0 = (float4)0;
	float2 vs_TEXCOORD0 = (float2)0;
	float4 vs_TEXCOORD2 = (float4)0;
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
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat0.wwww + u_xlat1;
    gl_Position = u_xlat0;
    u_xlat1 = in_COLOR0 * _Color;
    vs_COLOR0 = u_xlat1;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD1 = in_POSITION0;
    u_xlat0.xy = _ScreenParams.yy * MCOL(glstate_matrix_projection,1).xy;
    u_xlat0.xy = MCOL(glstate_matrix_projection,0).xy * _ScreenParams.xx + u_xlat0.xy;
    u_xlat0.xy = u_xlat0.ww / abs(u_xlat0.xy);
    u_xlat0.xy = float2(_UIMaskSoftnessX, _UIMaskSoftnessY) * float2(0.25, 0.25) + abs(u_xlat0.xy);
    vs_TEXCOORD2.zw = float2(0.25, 0.25) / u_xlat0.xy;
    u_xlat0 = max(_ClipRect, float4(-2e+10, -2e+10, -2e+10, -2e+10));
    u_xlat0 = min(u_xlat0, float4(2e+10, 2e+10, 2e+10, 2e+10));
    u_xlat0.xy = in_POSITION0.xy * float2(2.0, 2.0) + (-u_xlat0.xy);
    vs_TEXCOORD2.xy = (-u_xlat0.zw) + u_xlat0.xy;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_COLOR0 = vs_COLOR0;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float2 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float4 u_xlat0;
	float4 u_xlat16_0;
	float u_xlat16_1;
	float4 SV_Target0 = 0;

    u_xlat0.xy = (-_ClipRect.xy) + _ClipRect.zw;
    u_xlat0.xy = u_xlat0.xy + -abs(vs_TEXCOORD2.xy);
    u_xlat0.xy = u_xlat0.xy * vs_TEXCOORD2.zw;
    u_xlat0.xy = clamp(u_xlat0.xy, 0.0, 1.0);
    u_xlat16_1 = u_xlat0.y * u_xlat0.x;
    u_xlat16_0 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat0 = u_xlat16_0 + _TextureSampleAdd;
    u_xlat0 = u_xlat0 * vs_COLOR0;
    SV_Target0 = ((float4)(u_xlat16_1)) * u_xlat0;
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
	float4 vs_TEXCOORD2;
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
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat0.wwww + u_xlat1;
    gl_Position = u_xlat0;
    u_xlat1 = in_COLOR0 * _Color;
    vs_COLOR0 = u_xlat1;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD1 = in_POSITION0;
    u_xlat0.xy = _ScreenParams.yy * MCOL(glstate_matrix_projection,1).xy;
    u_xlat0.xy = MCOL(glstate_matrix_projection,0).xy * _ScreenParams.xx + u_xlat0.xy;
    u_xlat0.xy = u_xlat0.ww / abs(u_xlat0.xy);
    u_xlat0.xy = float2(_UIMaskSoftnessX, _UIMaskSoftnessY) * float2(0.25, 0.25) + abs(u_xlat0.xy);
    vs_TEXCOORD2.zw = float2(0.25, 0.25) / u_xlat0.xy;
    u_xlat0 = max(_ClipRect, float4(-2e+10, -2e+10, -2e+10, -2e+10));
    u_xlat0 = min(u_xlat0, float4(2e+10, 2e+10, 2e+10, 2e+10));
    u_xlat0.xy = in_POSITION0.xy * float2(2.0, 2.0) + (-u_xlat0.xy);
    vs_TEXCOORD2.xy = (-u_xlat0.zw) + u_xlat0.xy;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_COLOR0 = vs_COLOR0;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float2 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 u_xlat0;
	float4 u_xlat16_0;
	float4 SV_Target0 = 0;

    u_xlat16_0 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat0 = u_xlat16_0 + _TextureSampleAdd;
    u_xlat0 = u_xlat0 * vs_COLOR0;
    SV_Target0 = u_xlat0;
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
	float4 vs_TEXCOORD2;
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
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat0.wwww + u_xlat1;
    gl_Position = u_xlat0;
    u_xlat1 = in_COLOR0 * _Color;
    vs_COLOR0 = u_xlat1;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD1 = in_POSITION0;
    u_xlat0.xy = _ScreenParams.yy * MCOL(glstate_matrix_projection,1).xy;
    u_xlat0.xy = MCOL(glstate_matrix_projection,0).xy * _ScreenParams.xx + u_xlat0.xy;
    u_xlat0.xy = u_xlat0.ww / abs(u_xlat0.xy);
    u_xlat0.xy = float2(_UIMaskSoftnessX, _UIMaskSoftnessY) * float2(0.25, 0.25) + abs(u_xlat0.xy);
    vs_TEXCOORD2.zw = float2(0.25, 0.25) / u_xlat0.xy;
    u_xlat0 = max(_ClipRect, float4(-2e+10, -2e+10, -2e+10, -2e+10));
    u_xlat0 = min(u_xlat0, float4(2e+10, 2e+10, 2e+10, 2e+10));
    u_xlat0.xy = in_POSITION0.xy * float2(2.0, 2.0) + (-u_xlat0.xy);
    vs_TEXCOORD2.xy = (-u_xlat0.zw) + u_xlat0.xy;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_COLOR0 = vs_COLOR0;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float2 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 u_xlat0;
	float4 u_xlat16_0;
	float4 SV_Target0 = 0;

    u_xlat16_0 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat0 = u_xlat16_0 + _TextureSampleAdd;
    u_xlat0 = u_xlat0 * vs_COLOR0;
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#endif
ENDHLSL
		}
	}
}

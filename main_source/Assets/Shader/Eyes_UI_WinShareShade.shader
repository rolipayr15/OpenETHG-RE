Shader "Eyes/UI/WinShareShade" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_FaceOpacity ("Face Shade", Range(0.0, 1.0)) = 0.41999998688697815
		_TextOpacity ("Text Shade", Range(0.0, 1.0)) = 0.8399999737739563
		_TopOpacity ("Logo Shade", Range(0.0, 1.0)) = 0.8999999761581421
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

float4 _ClipRect;
float _FaceOpacity;
float _TextOpacity;
float _TopOpacity;
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
	float u_xlat0;
	bool u_xlatb0;
	float2 u_xlat1;
	bool2 u_xlatb1;
	float u_xlat16_2;
	float2 u_xlat3;
	bool2 u_xlatb3;
	float u_xlat6;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD0.x + -0.5;
    u_xlat0 = abs(u_xlat0) + -0.25999999;
    u_xlat0 = u_xlat0 * 5.88235235;
    u_xlat0 = clamp(u_xlat0, 0.0, 1.0);
    u_xlat3.x = u_xlat0 * -2.0 + 3.0;
    u_xlat0 = u_xlat0 * u_xlat0;
    u_xlat0 = (-u_xlat3.x) * u_xlat0 + 1.0;
    u_xlat3.xy = (-vs_TEXCOORD0.yy) + float2(0.680000007, 0.879999995);
    u_xlat3.xy = u_xlat3.xy * float2(6.25, 5.88235331);
    u_xlat3.xy = clamp(u_xlat3.xy, 0.0, 1.0);
    u_xlat1.xy = u_xlat3.xy * float2(-2.0, -2.0) + float2(3.0, 3.0);
    u_xlat3.xy = u_xlat3.xy * u_xlat3.xy;
    u_xlat3.xy = u_xlat3.xy * u_xlat1.xy;
    u_xlat1.xy = (-float2(_FaceOpacity, _TopOpacity)) + float2(_TextOpacity, _FaceOpacity);
    u_xlat3.xy = u_xlat3.xy * u_xlat1.xy + float2(_FaceOpacity, _TopOpacity);
    u_xlat6 = u_xlat3.y + (-_FaceOpacity);
    u_xlat0 = u_xlat0 * u_xlat6 + _FaceOpacity;
    u_xlat0 = max(u_xlat0, u_xlat3.x);
    u_xlat0 = u_xlat0 * vs_COLOR0.w;
    u_xlatb3.xy = GEQ(vs_TEXCOORD1.xyxx, _ClipRect.xyxx).xy;
    u_xlat3.x = u_xlatb3.x ? float(1.0) : 0.0;
    u_xlat3.y = u_xlatb3.y ? float(1.0) : 0.0;
;
    u_xlatb1.xy = GEQ(_ClipRect.zwzz, vs_TEXCOORD1.xyxx).xy;
    u_xlat1.x = u_xlatb1.x ? float(1.0) : 0.0;
    u_xlat1.y = u_xlatb1.y ? float(1.0) : 0.0;
;
    u_xlat3.xy = u_xlat3.xy * u_xlat1.xy;
    u_xlat3.x = u_xlat3.y * u_xlat3.x;
    u_xlat16_2 = u_xlat0 * u_xlat3.x + -0.00100000005;
    u_xlat0 = u_xlat3.x * u_xlat0;
    SV_Target0.w = u_xlat0;
    u_xlatb0 = u_xlat16_2<0.0;
    if(u_xlatb0){discard;}
    SV_Target0.xyz = vs_COLOR0.xyz;
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
	float u_xlat0;
	bool u_xlatb0;
	float2 u_xlat1;
	float u_xlat16_2;
	float2 u_xlat3;
	float u_xlat6;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD0.x + -0.5;
    u_xlat0 = abs(u_xlat0) + -0.25999999;
    u_xlat0 = u_xlat0 * 5.88235235;
    u_xlat0 = clamp(u_xlat0, 0.0, 1.0);
    u_xlat3.x = u_xlat0 * -2.0 + 3.0;
    u_xlat0 = u_xlat0 * u_xlat0;
    u_xlat0 = (-u_xlat3.x) * u_xlat0 + 1.0;
    u_xlat3.xy = (-vs_TEXCOORD0.yy) + float2(0.680000007, 0.879999995);
    u_xlat3.xy = u_xlat3.xy * float2(6.25, 5.88235331);
    u_xlat3.xy = clamp(u_xlat3.xy, 0.0, 1.0);
    u_xlat1.xy = u_xlat3.xy * float2(-2.0, -2.0) + float2(3.0, 3.0);
    u_xlat3.xy = u_xlat3.xy * u_xlat3.xy;
    u_xlat3.xy = u_xlat3.xy * u_xlat1.xy;
    u_xlat1.xy = (-float2(_FaceOpacity, _TopOpacity)) + float2(_TextOpacity, _FaceOpacity);
    u_xlat3.xy = u_xlat3.xy * u_xlat1.xy + float2(_FaceOpacity, _TopOpacity);
    u_xlat6 = u_xlat3.y + (-_FaceOpacity);
    u_xlat0 = u_xlat0 * u_xlat6 + _FaceOpacity;
    u_xlat0 = max(u_xlat0, u_xlat3.x);
    u_xlat16_2 = u_xlat0 * vs_COLOR0.w + -0.00100000005;
    u_xlat0 = u_xlat0 * vs_COLOR0.w;
    SV_Target0.w = u_xlat0;
    u_xlatb0 = u_xlat16_2<0.0;
    if(u_xlatb0){discard;}
    SV_Target0.xyz = vs_COLOR0.xyz;
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
	float u_xlat0;
	float2 u_xlat1;
	bool2 u_xlatb1;
	float2 u_xlat2;
	bool2 u_xlatb2;
	float u_xlat4;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD0.x + -0.5;
    u_xlat0 = abs(u_xlat0) + -0.25999999;
    u_xlat0 = u_xlat0 * 5.88235235;
    u_xlat0 = clamp(u_xlat0, 0.0, 1.0);
    u_xlat2.x = u_xlat0 * -2.0 + 3.0;
    u_xlat0 = u_xlat0 * u_xlat0;
    u_xlat0 = (-u_xlat2.x) * u_xlat0 + 1.0;
    u_xlat2.xy = (-vs_TEXCOORD0.yy) + float2(0.680000007, 0.879999995);
    u_xlat2.xy = u_xlat2.xy * float2(6.25, 5.88235331);
    u_xlat2.xy = clamp(u_xlat2.xy, 0.0, 1.0);
    u_xlat1.xy = u_xlat2.xy * float2(-2.0, -2.0) + float2(3.0, 3.0);
    u_xlat2.xy = u_xlat2.xy * u_xlat2.xy;
    u_xlat2.xy = u_xlat2.xy * u_xlat1.xy;
    u_xlat1.xy = (-float2(_FaceOpacity, _TopOpacity)) + float2(_TextOpacity, _FaceOpacity);
    u_xlat2.xy = u_xlat2.xy * u_xlat1.xy + float2(_FaceOpacity, _TopOpacity);
    u_xlat4 = u_xlat2.y + (-_FaceOpacity);
    u_xlat0 = u_xlat0 * u_xlat4 + _FaceOpacity;
    u_xlat0 = max(u_xlat0, u_xlat2.x);
    u_xlat0 = u_xlat0 * vs_COLOR0.w;
    u_xlatb2.xy = GEQ(vs_TEXCOORD1.xyxx, _ClipRect.xyxx).xy;
    u_xlat2.x = u_xlatb2.x ? float(1.0) : 0.0;
    u_xlat2.y = u_xlatb2.y ? float(1.0) : 0.0;
;
    u_xlatb1.xy = GEQ(_ClipRect.zwzz, vs_TEXCOORD1.xyxx).xy;
    u_xlat1.x = u_xlatb1.x ? float(1.0) : 0.0;
    u_xlat1.y = u_xlatb1.y ? float(1.0) : 0.0;
;
    u_xlat2.xy = u_xlat2.xy * u_xlat1.xy;
    u_xlat2.x = u_xlat2.y * u_xlat2.x;
    u_xlat0 = u_xlat2.x * u_xlat0;
    SV_Target0.w = u_xlat0;
    SV_Target0.xyz = vs_COLOR0.xyz;
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
	float u_xlat0;
	float2 u_xlat1;
	float2 u_xlat2;
	float u_xlat4;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD0.x + -0.5;
    u_xlat0 = abs(u_xlat0) + -0.25999999;
    u_xlat0 = u_xlat0 * 5.88235235;
    u_xlat0 = clamp(u_xlat0, 0.0, 1.0);
    u_xlat2.x = u_xlat0 * -2.0 + 3.0;
    u_xlat0 = u_xlat0 * u_xlat0;
    u_xlat0 = (-u_xlat2.x) * u_xlat0 + 1.0;
    u_xlat2.xy = (-vs_TEXCOORD0.yy) + float2(0.680000007, 0.879999995);
    u_xlat2.xy = u_xlat2.xy * float2(6.25, 5.88235331);
    u_xlat2.xy = clamp(u_xlat2.xy, 0.0, 1.0);
    u_xlat1.xy = u_xlat2.xy * float2(-2.0, -2.0) + float2(3.0, 3.0);
    u_xlat2.xy = u_xlat2.xy * u_xlat2.xy;
    u_xlat2.xy = u_xlat2.xy * u_xlat1.xy;
    u_xlat1.xy = (-float2(_FaceOpacity, _TopOpacity)) + float2(_TextOpacity, _FaceOpacity);
    u_xlat2.xy = u_xlat2.xy * u_xlat1.xy + float2(_FaceOpacity, _TopOpacity);
    u_xlat4 = u_xlat2.y + (-_FaceOpacity);
    u_xlat0 = u_xlat0 * u_xlat4 + _FaceOpacity;
    u_xlat0 = max(u_xlat0, u_xlat2.x);
    u_xlat0 = u_xlat0 * vs_COLOR0.w;
    SV_Target0.w = u_xlat0;
    SV_Target0.xyz = vs_COLOR0.xyz;
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
	float u_xlat0;
	float2 u_xlat1;
	float2 u_xlat2;
	float u_xlat4;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD0.x + -0.5;
    u_xlat0 = abs(u_xlat0) + -0.25999999;
    u_xlat0 = u_xlat0 * 5.88235235;
    u_xlat0 = clamp(u_xlat0, 0.0, 1.0);
    u_xlat2.x = u_xlat0 * -2.0 + 3.0;
    u_xlat0 = u_xlat0 * u_xlat0;
    u_xlat0 = (-u_xlat2.x) * u_xlat0 + 1.0;
    u_xlat2.xy = (-vs_TEXCOORD0.yy) + float2(0.680000007, 0.879999995);
    u_xlat2.xy = u_xlat2.xy * float2(6.25, 5.88235331);
    u_xlat2.xy = clamp(u_xlat2.xy, 0.0, 1.0);
    u_xlat1.xy = u_xlat2.xy * float2(-2.0, -2.0) + float2(3.0, 3.0);
    u_xlat2.xy = u_xlat2.xy * u_xlat2.xy;
    u_xlat2.xy = u_xlat2.xy * u_xlat1.xy;
    u_xlat1.xy = (-float2(_FaceOpacity, _TopOpacity)) + float2(_TextOpacity, _FaceOpacity);
    u_xlat2.xy = u_xlat2.xy * u_xlat1.xy + float2(_FaceOpacity, _TopOpacity);
    u_xlat4 = u_xlat2.y + (-_FaceOpacity);
    u_xlat0 = u_xlat0 * u_xlat4 + _FaceOpacity;
    u_xlat0 = max(u_xlat0, u_xlat2.x);
    u_xlat0 = u_xlat0 * vs_COLOR0.w;
    SV_Target0.w = u_xlat0;
    SV_Target0.xyz = vs_COLOR0.xyz;
    return SV_Target0;

	return SV_Target0;
}
#endif
ENDHLSL
		}
	}
}

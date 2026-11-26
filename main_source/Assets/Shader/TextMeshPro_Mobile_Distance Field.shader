Shader "TextMeshPro/Mobile/Distance Field" {
	Properties {
		_FaceColor ("Face Color", Color) = (1.0,1.0,1.0,1.0)
		_FaceDilate ("Face Dilate", Range(-1.0, 1.0)) = 0.0
		_OutlineColor ("Outline Color", Color) = (0.0,0.0,0.0,1.0)
		_OutlineWidth ("Outline Thickness", Range(0.0, 1.0)) = 0.0
		_OutlineSoftness ("Outline Softness", Range(0.0, 1.0)) = 0.0
		_UnderlayColor ("Border Color", Color) = (0.0,0.0,0.0,0.5)
		_UnderlayOffsetX ("Border OffsetX", Range(-1.0, 1.0)) = 0.0
		_UnderlayOffsetY ("Border OffsetY", Range(-1.0, 1.0)) = 0.0
		_UnderlayDilate ("Border Dilate", Range(-1.0, 1.0)) = 0.0
		_UnderlaySoftness ("Border Softness", Range(0.0, 1.0)) = 0.0
		_WeightNormal ("Weight Normal", Float) = 0.0
		_WeightBold ("Weight Bold", Float) = 0.5
		_ShaderFlags ("Flags", Float) = 0.0
		_ScaleRatioA ("Scale RatioA", Float) = 1.0
		_ScaleRatioB ("Scale RatioB", Float) = 1.0
		_ScaleRatioC ("Scale RatioC", Float) = 1.0
		_MainTex ("Font Atlas", 2D) = "white" {}
		_TextureWidth ("Texture Width", Float) = 512.0
		_TextureHeight ("Texture Height", Float) = 512.0
		_GradientScale ("Gradient Scale", Float) = 5.0
		_ScaleX ("Scale X", Float) = 1.0
		_ScaleY ("Scale Y", Float) = 1.0
		_PerspectiveFilter ("Perspective Correction", Range(0.0, 1.0)) = 0.875
		_Sharpness ("Sharpness", Range(-1.0, 1.0)) = 0.0
		_VertexOffsetX ("Vertex OffsetX", Float) = 0.0
		_VertexOffsetY ("Vertex OffsetY", Float) = 0.0
		_ClipRect ("Clip Rect", Vector) = (-32767.0,-32767.0,32767.0,32767.0)
		_MaskSoftnessX ("Mask SoftnessX", Float) = 0.0
		_MaskSoftnessY ("Mask SoftnessY", Float) = 0.0
		_StencilComp ("Stencil Comparison", Float) = 8.0
		_Stencil ("Stencil ID", Float) = 0.0
		_StencilOp ("Stencil Operation", Float) = 0.0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255.0
		_StencilReadMask ("Stencil Read Mask", Float) = 255.0
		_CullMode ("Cull Mode", Float) = 0.0
		_ColorMask ("Color Mask", Float) = 15.0
	}
	SubShader {
		Tags { "IgnoreProjector"="true" "Queue"="Transparent" "RenderType"="Transparent" }
		Pass {
			Tags { "IgnoreProjector"="true" "Queue"="Transparent" "RenderType"="Transparent" }
			Blend One OneMinusSrcAlpha
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
float4 _FaceColor;
float _FaceDilate;
float _GradientScale;
float _MaskSoftnessX;
float _MaskSoftnessY;
float4 _OutlineColor;
float _OutlineSoftness;
float _OutlineWidth;
float _PerspectiveFilter;
float _ScaleRatioA;
float _ScaleX;
float _ScaleY;
float _Sharpness;
float _UIMaskSoftnessX;
float _UIMaskSoftnessY;
float _VertexOffsetX;
float _VertexOffsetY;
float _WeightBold;
float _WeightNormal;
#if defined(UNITY_UI_ALPHACLIP) && defined(UNITY_UI_CLIP_RECT)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_COLOR0 : COLOR0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_COLOR0 : COLOR0;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float4 vs_TEXCOORD1 : TEXCOORD1;
	float4 vs_TEXCOORD2 : TEXCOORD2;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_COLOR0 = genIn.in_COLOR0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_COLOR0 = (float4)0;
	float4 vs_TEXCOORD0 = (float4)0;
	float4 vs_TEXCOORD1 = (float4)0;
	float4 vs_TEXCOORD2 = (float4)0;
	float4 vs_COLOR1;
	float2 u_xlat0;
	bool u_xlatb0;
	float4 u_xlat1;
	float4 u_xlat2;
	float4 u_xlat3;
	float4 u_xlat16_3;
	float4 u_xlat4;
	float u_xlat5;
	float u_xlat10;
	float2 u_xlat11;
	float u_xlat15;
	bool u_xlatb15;
	float u_xlat16;
	float4 gl_Position = 0;

    u_xlat0.xy = in_POSITION0.xy + float2(_VertexOffsetX, _VertexOffsetY);
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat1 = MCOL(unity_ObjectToWorld,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat1;
    u_xlat2 = u_xlat1 + MCOL(unity_ObjectToWorld,3);
    u_xlat1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat1.xyz;
    u_xlat1.xyz = (-u_xlat1.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat3 = u_xlat2.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat3 = MCOL(unity_MatrixVP,0) * u_xlat2.xxxx + u_xlat3;
    u_xlat3 = MCOL(unity_MatrixVP,2) * u_xlat2.zzzz + u_xlat3;
    u_xlat2 = MCOL(unity_MatrixVP,3) * u_xlat2.wwww + u_xlat3;
    gl_Position = u_xlat2;
    u_xlat16_3 = in_COLOR0 * _FaceColor;
    u_xlat16_3.xyz = u_xlat16_3.www * u_xlat16_3.xyz;
    vs_COLOR0 = u_xlat16_3;
    u_xlat10 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat10 = (1.0/sqrt(u_xlat10));
    u_xlat1.xyz = ((float3)(u_xlat10)) * u_xlat1.xyz;
    u_xlat2.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat2.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat2.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat10 = dot(u_xlat2.xyz, u_xlat2.xyz);
    u_xlat10 = (1.0/sqrt(u_xlat10));
    u_xlat2.xyz = ((float3)(u_xlat10)) * u_xlat2.xyz;
    u_xlat10 = dot(u_xlat2.xyz, u_xlat1.xyz);
    u_xlat1.xy = _ScreenParams.yy * MCOL(glstate_matrix_projection,1).xy;
    u_xlat1.xy = MCOL(glstate_matrix_projection,0).xy * _ScreenParams.xx + u_xlat1.xy;
    u_xlat1.xy = abs(u_xlat1.xy) * float2(_ScaleX, _ScaleY);
    u_xlat1.xy = u_xlat2.ww / u_xlat1.xy;
    u_xlat15 = dot(u_xlat1.xy, u_xlat1.xy);
    u_xlat15 = (1.0/sqrt(u_xlat15));
    u_xlat11.x = abs(in_TEXCOORD0.w) * _GradientScale;
    u_xlat16 = _Sharpness + 1.0;
    u_xlat11.x = u_xlat16 * u_xlat11.x;
    u_xlat16 = u_xlat15 * u_xlat11.x;
    u_xlat2.x = (-_PerspectiveFilter) + 1.0;
    u_xlat2.x = abs(u_xlat16) * u_xlat2.x;
    u_xlat15 = u_xlat15 * u_xlat11.x + (-u_xlat2.x);
    u_xlat10 = abs(u_xlat10) * u_xlat15 + u_xlat2.x;
    u_xlatb15 = MCOL(glstate_matrix_projection,3).w==0.0;
    u_xlat10 = (u_xlatb15) ? u_xlat10 : u_xlat16;
    u_xlat15 = _OutlineSoftness * _ScaleRatioA;
    u_xlat15 = u_xlat15 * u_xlat10 + 1.0;
    u_xlat2.x = u_xlat10 / u_xlat15;
    u_xlat10 = _OutlineWidth * _ScaleRatioA;
    u_xlat10 = u_xlat2.x * u_xlat10;
    u_xlat15 = min(u_xlat10, 1.0);
    u_xlat15 = sqrt(u_xlat15);
    u_xlat11.x = in_COLOR0.w * _OutlineColor.w;
    u_xlat4.xyz = _OutlineColor.xyz * u_xlat11.xxx + (-u_xlat16_3.xyz);
    u_xlat4.w = _OutlineColor.w * in_COLOR0.w + (-u_xlat16_3.w);
    u_xlat3 = ((float4)(u_xlat15)) * u_xlat4 + u_xlat16_3;
    vs_COLOR1 = u_xlat3;
    u_xlat3 = max(_ClipRect, float4(-2e+10, -2e+10, -2e+10, -2e+10));
    u_xlat3 = min(u_xlat3, float4(2e+10, 2e+10, 2e+10, 2e+10));
    u_xlat11.xy = u_xlat0.xy + (-u_xlat3.xy);
    u_xlat0.xy = u_xlat0.xy * float2(2.0, 2.0) + (-u_xlat3.xy);
    u_xlat4.xy = (-u_xlat3.zw) + u_xlat0.xy;
    u_xlat0.xy = (-u_xlat3.xy) + u_xlat3.zw;
    vs_TEXCOORD0.zw = u_xlat11.xy / u_xlat0.xy;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy;
    u_xlatb0 = 0.0>=in_TEXCOORD0.w;
    u_xlat0.x = u_xlatb0 ? 1.0 : float(0.0);
    u_xlat5 = (-_WeightNormal) + _WeightBold;
    u_xlat0.x = u_xlat0.x * u_xlat5 + _WeightNormal;
    u_xlat0.x = u_xlat0.x * 0.25 + _FaceDilate;
    u_xlat0.x = u_xlat0.x * _ScaleRatioA;
    u_xlat0.x = (-u_xlat0.x) * 0.5 + 0.5;
    u_xlat2.w = u_xlat0.x * u_xlat2.x + -0.5;
    u_xlat2.y = (-u_xlat10) * 0.5 + u_xlat2.w;
    u_xlat2.z = u_xlat10 * 0.5 + u_xlat2.w;
    vs_TEXCOORD1 = u_xlat2;
    u_xlat0.xy = max(float2(_MaskSoftnessX, _MaskSoftnessY), float2(_UIMaskSoftnessX, _UIMaskSoftnessY));
    u_xlat0.xy = u_xlat0.xy * float2(0.25, 0.25) + u_xlat1.xy;
    u_xlat4.zw = float2(0.25, 0.25) / u_xlat0.xy;
    vs_TEXCOORD2 = u_xlat4;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_COLOR0 = vs_COLOR0;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float4 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float2 u_xlat0;
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
    u_xlat16_0.x = tex2D(_MainTex, vs_TEXCOORD0.xy).w;
    u_xlat16_4 = u_xlat16_0.x * vs_TEXCOORD1.x + (-vs_TEXCOORD1.w);
    u_xlat16_4 = clamp(u_xlat16_4, 0.0, 1.0);
    u_xlat16_0 = ((float4)(u_xlat16_4)) * vs_COLOR0;
    u_xlat16_4 = u_xlat16_0.w * u_xlat16_1 + -0.00100000005;
    u_xlat16_0 = ((float4)(u_xlat16_1)) * u_xlat16_0;
    SV_Target0 = u_xlat16_0;
    u_xlatb2 = u_xlat16_4<0.0;
    if(u_xlatb2){discard;}
    return SV_Target0;

	return SV_Target0;
}
#elif defined(UNITY_UI_ALPHACLIP) && !defined(UNITY_UI_CLIP_RECT)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_COLOR0 : COLOR0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_COLOR0 : COLOR0;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float4 vs_TEXCOORD1 : TEXCOORD1;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_COLOR0 = genIn.in_COLOR0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_COLOR0 = (float4)0;
	float4 vs_TEXCOORD0 = (float4)0;
	float4 vs_TEXCOORD1 = (float4)0;
	float4 vs_COLOR1;
	float4 vs_TEXCOORD2;
	float2 u_xlat0;
	bool u_xlatb0;
	float4 u_xlat1;
	float4 u_xlat2;
	float4 u_xlat3;
	float4 u_xlat16_3;
	float4 u_xlat4;
	float u_xlat5;
	float u_xlat10;
	float2 u_xlat11;
	float u_xlat15;
	bool u_xlatb15;
	float u_xlat16;
	float4 gl_Position = 0;

    u_xlat0.xy = in_POSITION0.xy + float2(_VertexOffsetX, _VertexOffsetY);
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat1 = MCOL(unity_ObjectToWorld,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat1;
    u_xlat2 = u_xlat1 + MCOL(unity_ObjectToWorld,3);
    u_xlat1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat1.xyz;
    u_xlat1.xyz = (-u_xlat1.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat3 = u_xlat2.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat3 = MCOL(unity_MatrixVP,0) * u_xlat2.xxxx + u_xlat3;
    u_xlat3 = MCOL(unity_MatrixVP,2) * u_xlat2.zzzz + u_xlat3;
    u_xlat2 = MCOL(unity_MatrixVP,3) * u_xlat2.wwww + u_xlat3;
    gl_Position = u_xlat2;
    u_xlat16_3 = in_COLOR0 * _FaceColor;
    u_xlat16_3.xyz = u_xlat16_3.www * u_xlat16_3.xyz;
    vs_COLOR0 = u_xlat16_3;
    u_xlat10 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat10 = (1.0/sqrt(u_xlat10));
    u_xlat1.xyz = ((float3)(u_xlat10)) * u_xlat1.xyz;
    u_xlat2.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat2.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat2.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat10 = dot(u_xlat2.xyz, u_xlat2.xyz);
    u_xlat10 = (1.0/sqrt(u_xlat10));
    u_xlat2.xyz = ((float3)(u_xlat10)) * u_xlat2.xyz;
    u_xlat10 = dot(u_xlat2.xyz, u_xlat1.xyz);
    u_xlat1.xy = _ScreenParams.yy * MCOL(glstate_matrix_projection,1).xy;
    u_xlat1.xy = MCOL(glstate_matrix_projection,0).xy * _ScreenParams.xx + u_xlat1.xy;
    u_xlat1.xy = abs(u_xlat1.xy) * float2(_ScaleX, _ScaleY);
    u_xlat1.xy = u_xlat2.ww / u_xlat1.xy;
    u_xlat15 = dot(u_xlat1.xy, u_xlat1.xy);
    u_xlat15 = (1.0/sqrt(u_xlat15));
    u_xlat11.x = abs(in_TEXCOORD0.w) * _GradientScale;
    u_xlat16 = _Sharpness + 1.0;
    u_xlat11.x = u_xlat16 * u_xlat11.x;
    u_xlat16 = u_xlat15 * u_xlat11.x;
    u_xlat2.x = (-_PerspectiveFilter) + 1.0;
    u_xlat2.x = abs(u_xlat16) * u_xlat2.x;
    u_xlat15 = u_xlat15 * u_xlat11.x + (-u_xlat2.x);
    u_xlat10 = abs(u_xlat10) * u_xlat15 + u_xlat2.x;
    u_xlatb15 = MCOL(glstate_matrix_projection,3).w==0.0;
    u_xlat10 = (u_xlatb15) ? u_xlat10 : u_xlat16;
    u_xlat15 = _OutlineSoftness * _ScaleRatioA;
    u_xlat15 = u_xlat15 * u_xlat10 + 1.0;
    u_xlat2.x = u_xlat10 / u_xlat15;
    u_xlat10 = _OutlineWidth * _ScaleRatioA;
    u_xlat10 = u_xlat2.x * u_xlat10;
    u_xlat15 = min(u_xlat10, 1.0);
    u_xlat15 = sqrt(u_xlat15);
    u_xlat11.x = in_COLOR0.w * _OutlineColor.w;
    u_xlat4.xyz = _OutlineColor.xyz * u_xlat11.xxx + (-u_xlat16_3.xyz);
    u_xlat4.w = _OutlineColor.w * in_COLOR0.w + (-u_xlat16_3.w);
    u_xlat3 = ((float4)(u_xlat15)) * u_xlat4 + u_xlat16_3;
    vs_COLOR1 = u_xlat3;
    u_xlat3 = max(_ClipRect, float4(-2e+10, -2e+10, -2e+10, -2e+10));
    u_xlat3 = min(u_xlat3, float4(2e+10, 2e+10, 2e+10, 2e+10));
    u_xlat11.xy = u_xlat0.xy + (-u_xlat3.xy);
    u_xlat0.xy = u_xlat0.xy * float2(2.0, 2.0) + (-u_xlat3.xy);
    u_xlat4.xy = (-u_xlat3.zw) + u_xlat0.xy;
    u_xlat0.xy = (-u_xlat3.xy) + u_xlat3.zw;
    vs_TEXCOORD0.zw = u_xlat11.xy / u_xlat0.xy;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy;
    u_xlatb0 = 0.0>=in_TEXCOORD0.w;
    u_xlat0.x = u_xlatb0 ? 1.0 : float(0.0);
    u_xlat5 = (-_WeightNormal) + _WeightBold;
    u_xlat0.x = u_xlat0.x * u_xlat5 + _WeightNormal;
    u_xlat0.x = u_xlat0.x * 0.25 + _FaceDilate;
    u_xlat0.x = u_xlat0.x * _ScaleRatioA;
    u_xlat0.x = (-u_xlat0.x) * 0.5 + 0.5;
    u_xlat2.w = u_xlat0.x * u_xlat2.x + -0.5;
    u_xlat2.y = (-u_xlat10) * 0.5 + u_xlat2.w;
    u_xlat2.z = u_xlat10 * 0.5 + u_xlat2.w;
    vs_TEXCOORD1 = u_xlat2;
    u_xlat0.xy = max(float2(_MaskSoftnessX, _MaskSoftnessY), float2(_UIMaskSoftnessX, _UIMaskSoftnessY));
    u_xlat0.xy = u_xlat0.xy * float2(0.25, 0.25) + u_xlat1.xy;
    u_xlat4.zw = float2(0.25, 0.25) / u_xlat0.xy;
    vs_TEXCOORD2 = u_xlat4;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_COLOR0 = vs_COLOR0;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float4 u_xlat16_0;
	float u_xlat16_1;
	bool u_xlatb2;
	float u_xlat16_4;
	float4 SV_Target0 = 0;

    u_xlat16_0.x = tex2D(_MainTex, vs_TEXCOORD0.xy).w;
    u_xlat16_1 = u_xlat16_0.x * vs_TEXCOORD1.x + (-vs_TEXCOORD1.w);
    u_xlat16_1 = clamp(u_xlat16_1, 0.0, 1.0);
    u_xlat16_4 = vs_COLOR0.w * u_xlat16_1 + -0.00100000005;
    u_xlat16_0 = ((float4)(u_xlat16_1)) * vs_COLOR0;
    SV_Target0 = u_xlat16_0;
    u_xlatb2 = u_xlat16_4<0.0;
    if(u_xlatb2){discard;}
    return SV_Target0;

	return SV_Target0;
}
#elif defined(UNITY_UI_CLIP_RECT) && !defined(UNITY_UI_ALPHACLIP)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_COLOR0 : COLOR0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_COLOR0 : COLOR0;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float4 vs_TEXCOORD1 : TEXCOORD1;
	float4 vs_TEXCOORD2 : TEXCOORD2;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_COLOR0 = genIn.in_COLOR0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_COLOR0 = (float4)0;
	float4 vs_TEXCOORD0 = (float4)0;
	float4 vs_TEXCOORD1 = (float4)0;
	float4 vs_TEXCOORD2 = (float4)0;
	float4 vs_COLOR1;
	float2 u_xlat0;
	bool u_xlatb0;
	float4 u_xlat1;
	float4 u_xlat2;
	float4 u_xlat3;
	float4 u_xlat16_3;
	float4 u_xlat4;
	float u_xlat5;
	float u_xlat10;
	float2 u_xlat11;
	float u_xlat15;
	bool u_xlatb15;
	float u_xlat16;
	float4 gl_Position = 0;

    u_xlat0.xy = in_POSITION0.xy + float2(_VertexOffsetX, _VertexOffsetY);
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat1 = MCOL(unity_ObjectToWorld,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat1;
    u_xlat2 = u_xlat1 + MCOL(unity_ObjectToWorld,3);
    u_xlat1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat1.xyz;
    u_xlat1.xyz = (-u_xlat1.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat3 = u_xlat2.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat3 = MCOL(unity_MatrixVP,0) * u_xlat2.xxxx + u_xlat3;
    u_xlat3 = MCOL(unity_MatrixVP,2) * u_xlat2.zzzz + u_xlat3;
    u_xlat2 = MCOL(unity_MatrixVP,3) * u_xlat2.wwww + u_xlat3;
    gl_Position = u_xlat2;
    u_xlat16_3 = in_COLOR0 * _FaceColor;
    u_xlat16_3.xyz = u_xlat16_3.www * u_xlat16_3.xyz;
    vs_COLOR0 = u_xlat16_3;
    u_xlat10 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat10 = (1.0/sqrt(u_xlat10));
    u_xlat1.xyz = ((float3)(u_xlat10)) * u_xlat1.xyz;
    u_xlat2.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat2.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat2.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat10 = dot(u_xlat2.xyz, u_xlat2.xyz);
    u_xlat10 = (1.0/sqrt(u_xlat10));
    u_xlat2.xyz = ((float3)(u_xlat10)) * u_xlat2.xyz;
    u_xlat10 = dot(u_xlat2.xyz, u_xlat1.xyz);
    u_xlat1.xy = _ScreenParams.yy * MCOL(glstate_matrix_projection,1).xy;
    u_xlat1.xy = MCOL(glstate_matrix_projection,0).xy * _ScreenParams.xx + u_xlat1.xy;
    u_xlat1.xy = abs(u_xlat1.xy) * float2(_ScaleX, _ScaleY);
    u_xlat1.xy = u_xlat2.ww / u_xlat1.xy;
    u_xlat15 = dot(u_xlat1.xy, u_xlat1.xy);
    u_xlat15 = (1.0/sqrt(u_xlat15));
    u_xlat11.x = abs(in_TEXCOORD0.w) * _GradientScale;
    u_xlat16 = _Sharpness + 1.0;
    u_xlat11.x = u_xlat16 * u_xlat11.x;
    u_xlat16 = u_xlat15 * u_xlat11.x;
    u_xlat2.x = (-_PerspectiveFilter) + 1.0;
    u_xlat2.x = abs(u_xlat16) * u_xlat2.x;
    u_xlat15 = u_xlat15 * u_xlat11.x + (-u_xlat2.x);
    u_xlat10 = abs(u_xlat10) * u_xlat15 + u_xlat2.x;
    u_xlatb15 = MCOL(glstate_matrix_projection,3).w==0.0;
    u_xlat10 = (u_xlatb15) ? u_xlat10 : u_xlat16;
    u_xlat15 = _OutlineSoftness * _ScaleRatioA;
    u_xlat15 = u_xlat15 * u_xlat10 + 1.0;
    u_xlat2.x = u_xlat10 / u_xlat15;
    u_xlat10 = _OutlineWidth * _ScaleRatioA;
    u_xlat10 = u_xlat2.x * u_xlat10;
    u_xlat15 = min(u_xlat10, 1.0);
    u_xlat15 = sqrt(u_xlat15);
    u_xlat11.x = in_COLOR0.w * _OutlineColor.w;
    u_xlat4.xyz = _OutlineColor.xyz * u_xlat11.xxx + (-u_xlat16_3.xyz);
    u_xlat4.w = _OutlineColor.w * in_COLOR0.w + (-u_xlat16_3.w);
    u_xlat3 = ((float4)(u_xlat15)) * u_xlat4 + u_xlat16_3;
    vs_COLOR1 = u_xlat3;
    u_xlat3 = max(_ClipRect, float4(-2e+10, -2e+10, -2e+10, -2e+10));
    u_xlat3 = min(u_xlat3, float4(2e+10, 2e+10, 2e+10, 2e+10));
    u_xlat11.xy = u_xlat0.xy + (-u_xlat3.xy);
    u_xlat0.xy = u_xlat0.xy * float2(2.0, 2.0) + (-u_xlat3.xy);
    u_xlat4.xy = (-u_xlat3.zw) + u_xlat0.xy;
    u_xlat0.xy = (-u_xlat3.xy) + u_xlat3.zw;
    vs_TEXCOORD0.zw = u_xlat11.xy / u_xlat0.xy;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy;
    u_xlatb0 = 0.0>=in_TEXCOORD0.w;
    u_xlat0.x = u_xlatb0 ? 1.0 : float(0.0);
    u_xlat5 = (-_WeightNormal) + _WeightBold;
    u_xlat0.x = u_xlat0.x * u_xlat5 + _WeightNormal;
    u_xlat0.x = u_xlat0.x * 0.25 + _FaceDilate;
    u_xlat0.x = u_xlat0.x * _ScaleRatioA;
    u_xlat0.x = (-u_xlat0.x) * 0.5 + 0.5;
    u_xlat2.w = u_xlat0.x * u_xlat2.x + -0.5;
    u_xlat2.y = (-u_xlat10) * 0.5 + u_xlat2.w;
    u_xlat2.z = u_xlat10 * 0.5 + u_xlat2.w;
    vs_TEXCOORD1 = u_xlat2;
    u_xlat0.xy = max(float2(_MaskSoftnessX, _MaskSoftnessY), float2(_UIMaskSoftnessX, _UIMaskSoftnessY));
    u_xlat0.xy = u_xlat0.xy * float2(0.25, 0.25) + u_xlat1.xy;
    u_xlat4.zw = float2(0.25, 0.25) / u_xlat0.xy;
    vs_TEXCOORD2 = u_xlat4;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_COLOR0 = vs_COLOR0;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float4 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float2 u_xlat0;
	float4 u_xlat16_0;
	float u_xlat16_1;
	float u_xlat16_3;
	float4 SV_Target0 = 0;

    u_xlat0.xy = (-_ClipRect.xy) + _ClipRect.zw;
    u_xlat0.xy = u_xlat0.xy + -abs(vs_TEXCOORD2.xy);
    u_xlat0.xy = u_xlat0.xy * vs_TEXCOORD2.zw;
    u_xlat0.xy = clamp(u_xlat0.xy, 0.0, 1.0);
    u_xlat16_1 = u_xlat0.y * u_xlat0.x;
    u_xlat16_0.x = tex2D(_MainTex, vs_TEXCOORD0.xy).w;
    u_xlat16_3 = u_xlat16_0.x * vs_TEXCOORD1.x + (-vs_TEXCOORD1.w);
    u_xlat16_3 = clamp(u_xlat16_3, 0.0, 1.0);
    u_xlat16_0 = ((float4)(u_xlat16_3)) * vs_COLOR0;
    SV_Target0 = ((float4)(u_xlat16_1)) * u_xlat16_0;
    return SV_Target0;

	return SV_Target0;
}
#elif !defined(UNITY_UI_ALPHACLIP) && !defined(UNITY_UI_CLIP_RECT)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_COLOR0 : COLOR0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_COLOR0 : COLOR0;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float4 vs_TEXCOORD1 : TEXCOORD1;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_COLOR0 = genIn.in_COLOR0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_COLOR0 = (float4)0;
	float4 vs_TEXCOORD0 = (float4)0;
	float4 vs_TEXCOORD1 = (float4)0;
	float4 vs_COLOR1;
	float4 vs_TEXCOORD2;
	float2 u_xlat0;
	bool u_xlatb0;
	float4 u_xlat1;
	float4 u_xlat2;
	float4 u_xlat3;
	float4 u_xlat16_3;
	float4 u_xlat4;
	float u_xlat5;
	float u_xlat10;
	float2 u_xlat11;
	float u_xlat15;
	bool u_xlatb15;
	float u_xlat16;
	float4 gl_Position = 0;

    u_xlat0.xy = in_POSITION0.xy + float2(_VertexOffsetX, _VertexOffsetY);
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat1 = MCOL(unity_ObjectToWorld,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat1;
    u_xlat2 = u_xlat1 + MCOL(unity_ObjectToWorld,3);
    u_xlat1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat1.xyz;
    u_xlat1.xyz = (-u_xlat1.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat3 = u_xlat2.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat3 = MCOL(unity_MatrixVP,0) * u_xlat2.xxxx + u_xlat3;
    u_xlat3 = MCOL(unity_MatrixVP,2) * u_xlat2.zzzz + u_xlat3;
    u_xlat2 = MCOL(unity_MatrixVP,3) * u_xlat2.wwww + u_xlat3;
    gl_Position = u_xlat2;
    u_xlat16_3 = in_COLOR0 * _FaceColor;
    u_xlat16_3.xyz = u_xlat16_3.www * u_xlat16_3.xyz;
    vs_COLOR0 = u_xlat16_3;
    u_xlat10 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat10 = (1.0/sqrt(u_xlat10));
    u_xlat1.xyz = ((float3)(u_xlat10)) * u_xlat1.xyz;
    u_xlat2.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat2.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat2.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat10 = dot(u_xlat2.xyz, u_xlat2.xyz);
    u_xlat10 = (1.0/sqrt(u_xlat10));
    u_xlat2.xyz = ((float3)(u_xlat10)) * u_xlat2.xyz;
    u_xlat10 = dot(u_xlat2.xyz, u_xlat1.xyz);
    u_xlat1.xy = _ScreenParams.yy * MCOL(glstate_matrix_projection,1).xy;
    u_xlat1.xy = MCOL(glstate_matrix_projection,0).xy * _ScreenParams.xx + u_xlat1.xy;
    u_xlat1.xy = abs(u_xlat1.xy) * float2(_ScaleX, _ScaleY);
    u_xlat1.xy = u_xlat2.ww / u_xlat1.xy;
    u_xlat15 = dot(u_xlat1.xy, u_xlat1.xy);
    u_xlat15 = (1.0/sqrt(u_xlat15));
    u_xlat11.x = abs(in_TEXCOORD0.w) * _GradientScale;
    u_xlat16 = _Sharpness + 1.0;
    u_xlat11.x = u_xlat16 * u_xlat11.x;
    u_xlat16 = u_xlat15 * u_xlat11.x;
    u_xlat2.x = (-_PerspectiveFilter) + 1.0;
    u_xlat2.x = abs(u_xlat16) * u_xlat2.x;
    u_xlat15 = u_xlat15 * u_xlat11.x + (-u_xlat2.x);
    u_xlat10 = abs(u_xlat10) * u_xlat15 + u_xlat2.x;
    u_xlatb15 = MCOL(glstate_matrix_projection,3).w==0.0;
    u_xlat10 = (u_xlatb15) ? u_xlat10 : u_xlat16;
    u_xlat15 = _OutlineSoftness * _ScaleRatioA;
    u_xlat15 = u_xlat15 * u_xlat10 + 1.0;
    u_xlat2.x = u_xlat10 / u_xlat15;
    u_xlat10 = _OutlineWidth * _ScaleRatioA;
    u_xlat10 = u_xlat2.x * u_xlat10;
    u_xlat15 = min(u_xlat10, 1.0);
    u_xlat15 = sqrt(u_xlat15);
    u_xlat11.x = in_COLOR0.w * _OutlineColor.w;
    u_xlat4.xyz = _OutlineColor.xyz * u_xlat11.xxx + (-u_xlat16_3.xyz);
    u_xlat4.w = _OutlineColor.w * in_COLOR0.w + (-u_xlat16_3.w);
    u_xlat3 = ((float4)(u_xlat15)) * u_xlat4 + u_xlat16_3;
    vs_COLOR1 = u_xlat3;
    u_xlat3 = max(_ClipRect, float4(-2e+10, -2e+10, -2e+10, -2e+10));
    u_xlat3 = min(u_xlat3, float4(2e+10, 2e+10, 2e+10, 2e+10));
    u_xlat11.xy = u_xlat0.xy + (-u_xlat3.xy);
    u_xlat0.xy = u_xlat0.xy * float2(2.0, 2.0) + (-u_xlat3.xy);
    u_xlat4.xy = (-u_xlat3.zw) + u_xlat0.xy;
    u_xlat0.xy = (-u_xlat3.xy) + u_xlat3.zw;
    vs_TEXCOORD0.zw = u_xlat11.xy / u_xlat0.xy;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy;
    u_xlatb0 = 0.0>=in_TEXCOORD0.w;
    u_xlat0.x = u_xlatb0 ? 1.0 : float(0.0);
    u_xlat5 = (-_WeightNormal) + _WeightBold;
    u_xlat0.x = u_xlat0.x * u_xlat5 + _WeightNormal;
    u_xlat0.x = u_xlat0.x * 0.25 + _FaceDilate;
    u_xlat0.x = u_xlat0.x * _ScaleRatioA;
    u_xlat0.x = (-u_xlat0.x) * 0.5 + 0.5;
    u_xlat2.w = u_xlat0.x * u_xlat2.x + -0.5;
    u_xlat2.y = (-u_xlat10) * 0.5 + u_xlat2.w;
    u_xlat2.z = u_xlat10 * 0.5 + u_xlat2.w;
    vs_TEXCOORD1 = u_xlat2;
    u_xlat0.xy = max(float2(_MaskSoftnessX, _MaskSoftnessY), float2(_UIMaskSoftnessX, _UIMaskSoftnessY));
    u_xlat0.xy = u_xlat0.xy * float2(0.25, 0.25) + u_xlat1.xy;
    u_xlat4.zw = float2(0.25, 0.25) / u_xlat0.xy;
    vs_TEXCOORD2 = u_xlat4;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_COLOR0 = vs_COLOR0;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float u_xlat16_0;
	float u_xlat16_1;
	float4 SV_Target0 = 0;

    u_xlat16_0 = tex2D(_MainTex, vs_TEXCOORD0.xy).w;
    u_xlat16_1 = u_xlat16_0 * vs_TEXCOORD1.x + (-vs_TEXCOORD1.w);
    u_xlat16_1 = clamp(u_xlat16_1, 0.0, 1.0);
    SV_Target0 = ((float4)(u_xlat16_1)) * vs_COLOR0;
    return SV_Target0;

	return SV_Target0;
}
#else
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_COLOR0 : COLOR0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_COLOR0 : COLOR0;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float4 vs_TEXCOORD1 : TEXCOORD1;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_COLOR0 = genIn.in_COLOR0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_COLOR0 = (float4)0;
	float4 vs_TEXCOORD0 = (float4)0;
	float4 vs_TEXCOORD1 = (float4)0;
	float4 vs_COLOR1;
	float4 vs_TEXCOORD2;
	float2 u_xlat0;
	bool u_xlatb0;
	float4 u_xlat1;
	float4 u_xlat2;
	float4 u_xlat3;
	float4 u_xlat16_3;
	float4 u_xlat4;
	float u_xlat5;
	float u_xlat10;
	float2 u_xlat11;
	float u_xlat15;
	bool u_xlatb15;
	float u_xlat16;
	float4 gl_Position = 0;

    u_xlat0.xy = in_POSITION0.xy + float2(_VertexOffsetX, _VertexOffsetY);
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat1 = MCOL(unity_ObjectToWorld,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat1;
    u_xlat2 = u_xlat1 + MCOL(unity_ObjectToWorld,3);
    u_xlat1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat1.xyz;
    u_xlat1.xyz = (-u_xlat1.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat3 = u_xlat2.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat3 = MCOL(unity_MatrixVP,0) * u_xlat2.xxxx + u_xlat3;
    u_xlat3 = MCOL(unity_MatrixVP,2) * u_xlat2.zzzz + u_xlat3;
    u_xlat2 = MCOL(unity_MatrixVP,3) * u_xlat2.wwww + u_xlat3;
    gl_Position = u_xlat2;
    u_xlat16_3 = in_COLOR0 * _FaceColor;
    u_xlat16_3.xyz = u_xlat16_3.www * u_xlat16_3.xyz;
    vs_COLOR0 = u_xlat16_3;
    u_xlat10 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat10 = (1.0/sqrt(u_xlat10));
    u_xlat1.xyz = ((float3)(u_xlat10)) * u_xlat1.xyz;
    u_xlat2.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat2.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat2.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat10 = dot(u_xlat2.xyz, u_xlat2.xyz);
    u_xlat10 = (1.0/sqrt(u_xlat10));
    u_xlat2.xyz = ((float3)(u_xlat10)) * u_xlat2.xyz;
    u_xlat10 = dot(u_xlat2.xyz, u_xlat1.xyz);
    u_xlat1.xy = _ScreenParams.yy * MCOL(glstate_matrix_projection,1).xy;
    u_xlat1.xy = MCOL(glstate_matrix_projection,0).xy * _ScreenParams.xx + u_xlat1.xy;
    u_xlat1.xy = abs(u_xlat1.xy) * float2(_ScaleX, _ScaleY);
    u_xlat1.xy = u_xlat2.ww / u_xlat1.xy;
    u_xlat15 = dot(u_xlat1.xy, u_xlat1.xy);
    u_xlat15 = (1.0/sqrt(u_xlat15));
    u_xlat11.x = abs(in_TEXCOORD0.w) * _GradientScale;
    u_xlat16 = _Sharpness + 1.0;
    u_xlat11.x = u_xlat16 * u_xlat11.x;
    u_xlat16 = u_xlat15 * u_xlat11.x;
    u_xlat2.x = (-_PerspectiveFilter) + 1.0;
    u_xlat2.x = abs(u_xlat16) * u_xlat2.x;
    u_xlat15 = u_xlat15 * u_xlat11.x + (-u_xlat2.x);
    u_xlat10 = abs(u_xlat10) * u_xlat15 + u_xlat2.x;
    u_xlatb15 = MCOL(glstate_matrix_projection,3).w==0.0;
    u_xlat10 = (u_xlatb15) ? u_xlat10 : u_xlat16;
    u_xlat15 = _OutlineSoftness * _ScaleRatioA;
    u_xlat15 = u_xlat15 * u_xlat10 + 1.0;
    u_xlat2.x = u_xlat10 / u_xlat15;
    u_xlat10 = _OutlineWidth * _ScaleRatioA;
    u_xlat10 = u_xlat2.x * u_xlat10;
    u_xlat15 = min(u_xlat10, 1.0);
    u_xlat15 = sqrt(u_xlat15);
    u_xlat11.x = in_COLOR0.w * _OutlineColor.w;
    u_xlat4.xyz = _OutlineColor.xyz * u_xlat11.xxx + (-u_xlat16_3.xyz);
    u_xlat4.w = _OutlineColor.w * in_COLOR0.w + (-u_xlat16_3.w);
    u_xlat3 = ((float4)(u_xlat15)) * u_xlat4 + u_xlat16_3;
    vs_COLOR1 = u_xlat3;
    u_xlat3 = max(_ClipRect, float4(-2e+10, -2e+10, -2e+10, -2e+10));
    u_xlat3 = min(u_xlat3, float4(2e+10, 2e+10, 2e+10, 2e+10));
    u_xlat11.xy = u_xlat0.xy + (-u_xlat3.xy);
    u_xlat0.xy = u_xlat0.xy * float2(2.0, 2.0) + (-u_xlat3.xy);
    u_xlat4.xy = (-u_xlat3.zw) + u_xlat0.xy;
    u_xlat0.xy = (-u_xlat3.xy) + u_xlat3.zw;
    vs_TEXCOORD0.zw = u_xlat11.xy / u_xlat0.xy;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy;
    u_xlatb0 = 0.0>=in_TEXCOORD0.w;
    u_xlat0.x = u_xlatb0 ? 1.0 : float(0.0);
    u_xlat5 = (-_WeightNormal) + _WeightBold;
    u_xlat0.x = u_xlat0.x * u_xlat5 + _WeightNormal;
    u_xlat0.x = u_xlat0.x * 0.25 + _FaceDilate;
    u_xlat0.x = u_xlat0.x * _ScaleRatioA;
    u_xlat0.x = (-u_xlat0.x) * 0.5 + 0.5;
    u_xlat2.w = u_xlat0.x * u_xlat2.x + -0.5;
    u_xlat2.y = (-u_xlat10) * 0.5 + u_xlat2.w;
    u_xlat2.z = u_xlat10 * 0.5 + u_xlat2.w;
    vs_TEXCOORD1 = u_xlat2;
    u_xlat0.xy = max(float2(_MaskSoftnessX, _MaskSoftnessY), float2(_UIMaskSoftnessX, _UIMaskSoftnessY));
    u_xlat0.xy = u_xlat0.xy * float2(0.25, 0.25) + u_xlat1.xy;
    u_xlat4.zw = float2(0.25, 0.25) / u_xlat0.xy;
    vs_TEXCOORD2 = u_xlat4;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_COLOR0 = vs_COLOR0;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float u_xlat16_0;
	float u_xlat16_1;
	float4 SV_Target0 = 0;

    u_xlat16_0 = tex2D(_MainTex, vs_TEXCOORD0.xy).w;
    u_xlat16_1 = u_xlat16_0 * vs_TEXCOORD1.x + (-vs_TEXCOORD1.w);
    u_xlat16_1 = clamp(u_xlat16_1, 0.0, 1.0);
    SV_Target0 = ((float4)(u_xlat16_1)) * vs_COLOR0;
    return SV_Target0;

	return SV_Target0;
}
#endif
ENDHLSL
		}
	}
}

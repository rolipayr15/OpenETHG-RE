Shader "Eric/BuiltIn_AlphaBlendFlow" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		[HDR] _TintColor ("Tint Color", Color) = (1.0,1.0,1.0,1.0)
		_FlowSpeed ("MainTex Flow Speed", Vector) = (0.0,0.0,0.0,0.0)
		_Brightness ("Brightness Boost", Float) = 1.0
		[Header(Soft Particles)] [Toggle(_SOFTPARTICLES_ON)] _SoftParticlesEnabled ("Enable Soft Particles", Float) = 0.0
		_SoftParticlesFadeDistance ("Fade Distance", Range(0.009999999776482582, 10.0)) = 1.0
		[Header(UV Distortion)] _DistortionMap ("Distortion Map", 2D) = "white" {}
		_DistortionPower ("Distortion Power", Range(0.0, 1.0)) = 0.0
		_DistortionSpeed ("Distortion Speed", Vector) = (0.0,0.0,0.0,0.0)
		[Header(Fresnel Rim Light)] [Toggle(_FRESNEL_ON)] _UseFresnel ("Enable Fresnel", Float) = 0.0
		[Enum(Normal Based, 0, UV Based, 1)] _FresnelType ("Fresnel Type", Float) = 0.0
		[HDR] _FresnelColor ("Fresnel Color", Color) = (1.0,1.0,0.0,1.0)
		_FresnelIntensity ("Fresnel Intensity", Float) = 5.0
		_FresnelWidth ("Fresnel Width", Range(0.10000000149011612, 15.0)) = 5.0
		[Toggle(_FRESNEL_MASK_ON)] _FresnelMasksTexture ("Use Fresnel as Texture Mask", Float) = 0.0
		[Header(Dissolve)] [Toggle(_USEPARTICLEALPHADISSOLVE_ON)] _UseAlphaDissolve ("Use Alpha as Dissolve", Float) = 0.0
		_DissolveTex ("Dissolve Texture", 2D) = "white" {}
		_DissolveAmount ("Dissolve Amount", Range(0.0, 1.0)) = 0.0
		_VertexAlphaRef ("Alpha Ref", Range(0.009999999776482582, 1.0)) = 1.0
		[HDR] _DissolveEdgeColor ("Dissolve Edge Color", Color) = (1.0,0.0,0.0,1.0)
		_DissolveEdgeWidth ("Dissolve Edge Width", Range(0.0, 0.20000000298023224)) = 0.05000000074505806
		[Header(Overlay)] _OverlayTex ("Overlay Texture", 2D) = "white" {}
		[HDR] _OverlayColor ("Overlay Color", Color) = (1.0,1.0,1.0,1.0)
		_OverlayFlowSpeed ("Overlay Flow Speed", Vector) = (0.0,0.0,0.0,0.0)
		[Header(Vertex Distort)] [Toggle(_VERTEX_DISTORT_ON)] _UseVertexDistort ("Enable Vertex Distort", Float) = 0.0
		_DistortStrength ("Vertex Strength", Range(0.0, 0.5)) = 0.0
		_DistortSpeed ("Vertex Speed", Float) = 1.0
		_DistortFrequency ("Vertex Frequency", Float) = 2.0
	}
	SubShader {
		Tags { "IgnoreProjector"="true" "PreviewType"="Plane" "Queue"="Transparent" "RenderType"="Transparent" }
		Pass {
			Tags { "IgnoreProjector"="true" "PreviewType"="Plane" "Queue"="Transparent" "RenderType"="Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Cull Off
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#pragma multi_compile _ SOFTPARTICLES_ON
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

sampler2D _DissolveTex;
sampler2D _DistortionMap;
sampler2D _MainTex;
sampler2D _OverlayTex;
float _Brightness;
float _DissolveAmount;
float4 _DissolveEdgeColor;
float _DissolveEdgeWidth;
float4 _DissolveTex_ST;
float4 _DistortionMap_ST;
float _DistortionPower;
float4 _DistortionSpeed;
float4 _FlowSpeed;
float4 _MainTex_ST;
float4 _OverlayColor;
float4 _OverlayFlowSpeed;
float4 _OverlayTex_ST;
float4 _TintColor;
#if defined(SOFTPARTICLES_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float2 in_TEXCOORD0 : TEXCOORD0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_COLOR0 : COLOR0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float2 vs_TEXCOORD0 : TEXCOORD0;
	float4 vs_COLOR0 : COLOR0;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float2 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_COLOR0 = genIn.in_COLOR0;
	float2 vs_TEXCOORD0 = (float2)0;
	float4 vs_COLOR0 = (float4)0;
	float4 vs_TEXCOORD1;
	float3 vs_TEXCOORD3;
	float3 vs_TEXCOORD4;
	float4 u_xlat0;
	float4 u_xlat1;
	float u_xlat2;
	float u_xlat6;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat1 = MCOL(unity_MatrixVP,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_MatrixVP,2) * u_xlat0.zzzz + u_xlat1;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat0.wwww + u_xlat1;
    gl_Position = u_xlat1;
    u_xlat2 = u_xlat0.y * MCOL(unity_MatrixV,1).z;
    u_xlat0.x = MCOL(unity_MatrixV,0).z * u_xlat0.x + u_xlat2;
    u_xlat0.x = MCOL(unity_MatrixV,2).z * u_xlat0.z + u_xlat0.x;
    u_xlat0.x = MCOL(unity_MatrixV,3).z * u_xlat0.w + u_xlat0.x;
    vs_TEXCOORD1.z = (-u_xlat0.x);
    u_xlat0.x = u_xlat1.y * _ProjectionParams.x;
    u_xlat0.w = u_xlat0.x * 0.5;
    u_xlat0.xz = u_xlat1.xw * float2(0.5, 0.5);
    vs_TEXCOORD1.w = u_xlat1.w;
    vs_TEXCOORD1.xy = u_xlat0.zz + u_xlat0.xw;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = (1.0/sqrt(u_xlat6));
    vs_TEXCOORD3.xyz = ((float3)(u_xlat6)) * u_xlat0.xyz;
    u_xlat0.xyz = in_POSITION0.yyy * MCOL(unity_ObjectToWorld,1).xyz;
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,0).xyz * in_POSITION0.xxx + u_xlat0.xyz;
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,2).xyz * in_POSITION0.zzz + u_xlat0.xyz;
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    vs_TEXCOORD4.xyz = (-u_xlat0.xyz) + _WorldSpaceCameraPos.xyz;
    vs_COLOR0 = in_COLOR0;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_COLOR0 = vs_COLOR0;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float2 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float3 u_xlat16_0;
	float4 u_xlat1;
	float4 u_xlat10_1;
	bool2 u_xlatb1;
	float3 u_xlat2;
	float4 u_xlat16_2;
	float3 u_xlat16_3;
	float4 u_xlat16_4;
	float u_xlat16_5;
	float u_xlat16_10;
	float2 u_xlat11;
	float2 u_xlat10_11;
	float4 SV_Target0 = 0;

    u_xlat16_0.x = _DissolveAmount + (-_DissolveEdgeWidth);
    u_xlat1.xy = vs_TEXCOORD0.xy * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
    u_xlat1.x = tex2D(_DissolveTex, u_xlat1.xy).x;
    u_xlatb1.y = u_xlat1.x>=u_xlat16_0.x;
    u_xlatb1.x = u_xlat1.x>=_DissolveAmount;
    u_xlat16_0.x = (u_xlatb1.x) ? float(1.0) : float(0.0);
    u_xlat16_0.y = (u_xlatb1.x) ? float(-1.0) : float(-0.0);
    u_xlat16_0.z = (u_xlatb1.y) ? float(1.0) : float(0.0);
    u_xlat16_5 = u_xlat16_0.y + u_xlat16_0.z;
    u_xlat16_0.y = max(u_xlat16_5, 0.0);
    u_xlat1.xy = vs_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    u_xlat1.xy = _FlowSpeed.xy * _Time.yy + u_xlat1.xy;
    u_xlat11.xy = vs_TEXCOORD0.xy * _DistortionMap_ST.xy + _DistortionMap_ST.zw;
    u_xlat11.xy = _DistortionSpeed.xy * _Time.yy + u_xlat11.xy;
    u_xlat10_11.xy = tex2D(_DistortionMap, u_xlat11.xy).xy;
    u_xlat11.xy = u_xlat10_11.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
    u_xlat1.xy = u_xlat11.xy * ((float2)(float2(_DistortionPower, _DistortionPower))) + u_xlat1.xy;
    u_xlat16_2 = tex2D(_MainTex, u_xlat1.xy);
    u_xlat16_0.xy = u_xlat16_0.xy * u_xlat16_2.ww;
    u_xlat16_10 = _DissolveAmount * 100.0;
    u_xlat16_10 = clamp(u_xlat16_10, 0.0, 1.0);
    u_xlat16_5 = u_xlat16_10 * u_xlat16_0.y;
    u_xlat16_3.xyz = ((float3)(u_xlat16_5)) * _DissolveEdgeColor.xyz;
    u_xlat1.xy = vs_TEXCOORD0.xy * _OverlayTex_ST.xy + _OverlayTex_ST.zw;
    u_xlat1.xy = _OverlayFlowSpeed.xy * _Time.yy + u_xlat1.xy;
    u_xlat1.xy = u_xlat11.xy * ((float2)(float2(_DistortionPower, _DistortionPower))) + u_xlat1.xy;
    u_xlat10_1 = tex2D(_OverlayTex, u_xlat1.xy);
    u_xlat1 = u_xlat10_1 * _OverlayColor;
    u_xlat16_4 = u_xlat16_2 * _TintColor;
    u_xlat2.xyz = u_xlat16_4.xyz * vs_COLOR0.xyz;
    u_xlat16_10 = u_xlat1.w * u_xlat16_4.w;
    u_xlat1.xyz = u_xlat1.xyz * u_xlat2.xyz;
    u_xlat1.xyz = u_xlat1.xyz * ((float3)(_Brightness));
    SV_Target0.xyz = u_xlat1.xyz * u_xlat16_0.xxx + u_xlat16_3.xyz;
    u_xlat16_0.x = u_xlat16_10 * u_xlat16_0.x + u_xlat16_5;
    u_xlat16_0.x = clamp(u_xlat16_0.x, 0.0, 1.0);
    u_xlat1.x = u_xlat16_0.x * vs_COLOR0.w;
    SV_Target0.w = u_xlat1.x;
    return SV_Target0;

	return SV_Target0;
}
#elif !defined(SOFTPARTICLES_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float2 in_TEXCOORD0 : TEXCOORD0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_COLOR0 : COLOR0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float2 vs_TEXCOORD0 : TEXCOORD0;
	float4 vs_COLOR0 : COLOR0;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float2 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_COLOR0 = genIn.in_COLOR0;
	float2 vs_TEXCOORD0 = (float2)0;
	float4 vs_COLOR0 = (float4)0;
	float4 vs_TEXCOORD1;
	float3 vs_TEXCOORD3;
	float3 vs_TEXCOORD4;
	float4 u_xlat0;
	float4 u_xlat1;
	float u_xlat2;
	float u_xlat6;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat1 = MCOL(unity_MatrixVP,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_MatrixVP,2) * u_xlat0.zzzz + u_xlat1;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat0.wwww + u_xlat1;
    gl_Position = u_xlat1;
    u_xlat2 = u_xlat0.y * MCOL(unity_MatrixV,1).z;
    u_xlat0.x = MCOL(unity_MatrixV,0).z * u_xlat0.x + u_xlat2;
    u_xlat0.x = MCOL(unity_MatrixV,2).z * u_xlat0.z + u_xlat0.x;
    u_xlat0.x = MCOL(unity_MatrixV,3).z * u_xlat0.w + u_xlat0.x;
    vs_TEXCOORD1.z = (-u_xlat0.x);
    u_xlat0.x = u_xlat1.y * _ProjectionParams.x;
    u_xlat0.w = u_xlat0.x * 0.5;
    u_xlat0.xz = u_xlat1.xw * float2(0.5, 0.5);
    vs_TEXCOORD1.w = u_xlat1.w;
    vs_TEXCOORD1.xy = u_xlat0.zz + u_xlat0.xw;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = (1.0/sqrt(u_xlat6));
    vs_TEXCOORD3.xyz = ((float3)(u_xlat6)) * u_xlat0.xyz;
    u_xlat0.xyz = in_POSITION0.yyy * MCOL(unity_ObjectToWorld,1).xyz;
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,0).xyz * in_POSITION0.xxx + u_xlat0.xyz;
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,2).xyz * in_POSITION0.zzz + u_xlat0.xyz;
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    vs_TEXCOORD4.xyz = (-u_xlat0.xyz) + _WorldSpaceCameraPos.xyz;
    vs_COLOR0 = in_COLOR0;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_COLOR0 = vs_COLOR0;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float2 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float3 u_xlat16_0;
	float4 u_xlat1;
	float4 u_xlat10_1;
	bool2 u_xlatb1;
	float3 u_xlat2;
	float4 u_xlat16_2;
	float3 u_xlat16_3;
	float4 u_xlat16_4;
	float u_xlat16_5;
	float u_xlat16_10;
	float2 u_xlat11;
	float2 u_xlat10_11;
	float4 SV_Target0 = 0;

    u_xlat16_0.x = _DissolveAmount + (-_DissolveEdgeWidth);
    u_xlat1.xy = vs_TEXCOORD0.xy * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
    u_xlat1.x = tex2D(_DissolveTex, u_xlat1.xy).x;
    u_xlatb1.y = u_xlat1.x>=u_xlat16_0.x;
    u_xlatb1.x = u_xlat1.x>=_DissolveAmount;
    u_xlat16_0.x = (u_xlatb1.x) ? float(1.0) : float(0.0);
    u_xlat16_0.y = (u_xlatb1.x) ? float(-1.0) : float(-0.0);
    u_xlat16_0.z = (u_xlatb1.y) ? float(1.0) : float(0.0);
    u_xlat16_5 = u_xlat16_0.y + u_xlat16_0.z;
    u_xlat16_0.y = max(u_xlat16_5, 0.0);
    u_xlat1.xy = vs_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    u_xlat1.xy = _FlowSpeed.xy * _Time.yy + u_xlat1.xy;
    u_xlat11.xy = vs_TEXCOORD0.xy * _DistortionMap_ST.xy + _DistortionMap_ST.zw;
    u_xlat11.xy = _DistortionSpeed.xy * _Time.yy + u_xlat11.xy;
    u_xlat10_11.xy = tex2D(_DistortionMap, u_xlat11.xy).xy;
    u_xlat11.xy = u_xlat10_11.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
    u_xlat1.xy = u_xlat11.xy * ((float2)(float2(_DistortionPower, _DistortionPower))) + u_xlat1.xy;
    u_xlat16_2 = tex2D(_MainTex, u_xlat1.xy);
    u_xlat16_0.xy = u_xlat16_0.xy * u_xlat16_2.ww;
    u_xlat16_10 = _DissolveAmount * 100.0;
    u_xlat16_10 = clamp(u_xlat16_10, 0.0, 1.0);
    u_xlat16_5 = u_xlat16_10 * u_xlat16_0.y;
    u_xlat16_3.xyz = ((float3)(u_xlat16_5)) * _DissolveEdgeColor.xyz;
    u_xlat1.xy = vs_TEXCOORD0.xy * _OverlayTex_ST.xy + _OverlayTex_ST.zw;
    u_xlat1.xy = _OverlayFlowSpeed.xy * _Time.yy + u_xlat1.xy;
    u_xlat1.xy = u_xlat11.xy * ((float2)(float2(_DistortionPower, _DistortionPower))) + u_xlat1.xy;
    u_xlat10_1 = tex2D(_OverlayTex, u_xlat1.xy);
    u_xlat1 = u_xlat10_1 * _OverlayColor;
    u_xlat16_4 = u_xlat16_2 * _TintColor;
    u_xlat2.xyz = u_xlat16_4.xyz * vs_COLOR0.xyz;
    u_xlat16_10 = u_xlat1.w * u_xlat16_4.w;
    u_xlat1.xyz = u_xlat1.xyz * u_xlat2.xyz;
    u_xlat1.xyz = u_xlat1.xyz * ((float3)(_Brightness));
    SV_Target0.xyz = u_xlat1.xyz * u_xlat16_0.xxx + u_xlat16_3.xyz;
    u_xlat16_0.x = u_xlat16_10 * u_xlat16_0.x + u_xlat16_5;
    u_xlat16_0.x = clamp(u_xlat16_0.x, 0.0, 1.0);
    u_xlat1.x = u_xlat16_0.x * vs_COLOR0.w;
    SV_Target0.w = u_xlat1.x;
    return SV_Target0;

	return SV_Target0;
}
#else
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float2 in_TEXCOORD0 : TEXCOORD0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_COLOR0 : COLOR0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float2 vs_TEXCOORD0 : TEXCOORD0;
	float4 vs_COLOR0 : COLOR0;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float2 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_COLOR0 = genIn.in_COLOR0;
	float2 vs_TEXCOORD0 = (float2)0;
	float4 vs_COLOR0 = (float4)0;
	float4 vs_TEXCOORD1;
	float3 vs_TEXCOORD3;
	float3 vs_TEXCOORD4;
	float4 u_xlat0;
	float4 u_xlat1;
	float u_xlat2;
	float u_xlat6;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat1 = MCOL(unity_MatrixVP,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_MatrixVP,2) * u_xlat0.zzzz + u_xlat1;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat0.wwww + u_xlat1;
    gl_Position = u_xlat1;
    u_xlat2 = u_xlat0.y * MCOL(unity_MatrixV,1).z;
    u_xlat0.x = MCOL(unity_MatrixV,0).z * u_xlat0.x + u_xlat2;
    u_xlat0.x = MCOL(unity_MatrixV,2).z * u_xlat0.z + u_xlat0.x;
    u_xlat0.x = MCOL(unity_MatrixV,3).z * u_xlat0.w + u_xlat0.x;
    vs_TEXCOORD1.z = (-u_xlat0.x);
    u_xlat0.x = u_xlat1.y * _ProjectionParams.x;
    u_xlat0.w = u_xlat0.x * 0.5;
    u_xlat0.xz = u_xlat1.xw * float2(0.5, 0.5);
    vs_TEXCOORD1.w = u_xlat1.w;
    vs_TEXCOORD1.xy = u_xlat0.zz + u_xlat0.xw;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = (1.0/sqrt(u_xlat6));
    vs_TEXCOORD3.xyz = ((float3)(u_xlat6)) * u_xlat0.xyz;
    u_xlat0.xyz = in_POSITION0.yyy * MCOL(unity_ObjectToWorld,1).xyz;
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,0).xyz * in_POSITION0.xxx + u_xlat0.xyz;
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,2).xyz * in_POSITION0.zzz + u_xlat0.xyz;
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    vs_TEXCOORD4.xyz = (-u_xlat0.xyz) + _WorldSpaceCameraPos.xyz;
    vs_COLOR0 = in_COLOR0;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_COLOR0 = vs_COLOR0;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float2 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 vs_COLOR0 = genIn.vs_COLOR0;
	float3 u_xlat16_0;
	float4 u_xlat1;
	float4 u_xlat10_1;
	bool2 u_xlatb1;
	float3 u_xlat2;
	float4 u_xlat16_2;
	float3 u_xlat16_3;
	float4 u_xlat16_4;
	float u_xlat16_5;
	float u_xlat16_10;
	float2 u_xlat11;
	float2 u_xlat10_11;
	float4 SV_Target0 = 0;

    u_xlat16_0.x = _DissolveAmount + (-_DissolveEdgeWidth);
    u_xlat1.xy = vs_TEXCOORD0.xy * _DissolveTex_ST.xy + _DissolveTex_ST.zw;
    u_xlat1.x = tex2D(_DissolveTex, u_xlat1.xy).x;
    u_xlatb1.y = u_xlat1.x>=u_xlat16_0.x;
    u_xlatb1.x = u_xlat1.x>=_DissolveAmount;
    u_xlat16_0.x = (u_xlatb1.x) ? float(1.0) : float(0.0);
    u_xlat16_0.y = (u_xlatb1.x) ? float(-1.0) : float(-0.0);
    u_xlat16_0.z = (u_xlatb1.y) ? float(1.0) : float(0.0);
    u_xlat16_5 = u_xlat16_0.y + u_xlat16_0.z;
    u_xlat16_0.y = max(u_xlat16_5, 0.0);
    u_xlat1.xy = vs_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    u_xlat1.xy = _FlowSpeed.xy * _Time.yy + u_xlat1.xy;
    u_xlat11.xy = vs_TEXCOORD0.xy * _DistortionMap_ST.xy + _DistortionMap_ST.zw;
    u_xlat11.xy = _DistortionSpeed.xy * _Time.yy + u_xlat11.xy;
    u_xlat10_11.xy = tex2D(_DistortionMap, u_xlat11.xy).xy;
    u_xlat11.xy = u_xlat10_11.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
    u_xlat1.xy = u_xlat11.xy * ((float2)(float2(_DistortionPower, _DistortionPower))) + u_xlat1.xy;
    u_xlat16_2 = tex2D(_MainTex, u_xlat1.xy);
    u_xlat16_0.xy = u_xlat16_0.xy * u_xlat16_2.ww;
    u_xlat16_10 = _DissolveAmount * 100.0;
    u_xlat16_10 = clamp(u_xlat16_10, 0.0, 1.0);
    u_xlat16_5 = u_xlat16_10 * u_xlat16_0.y;
    u_xlat16_3.xyz = ((float3)(u_xlat16_5)) * _DissolveEdgeColor.xyz;
    u_xlat1.xy = vs_TEXCOORD0.xy * _OverlayTex_ST.xy + _OverlayTex_ST.zw;
    u_xlat1.xy = _OverlayFlowSpeed.xy * _Time.yy + u_xlat1.xy;
    u_xlat1.xy = u_xlat11.xy * ((float2)(float2(_DistortionPower, _DistortionPower))) + u_xlat1.xy;
    u_xlat10_1 = tex2D(_OverlayTex, u_xlat1.xy);
    u_xlat1 = u_xlat10_1 * _OverlayColor;
    u_xlat16_4 = u_xlat16_2 * _TintColor;
    u_xlat2.xyz = u_xlat16_4.xyz * vs_COLOR0.xyz;
    u_xlat16_10 = u_xlat1.w * u_xlat16_4.w;
    u_xlat1.xyz = u_xlat1.xyz * u_xlat2.xyz;
    u_xlat1.xyz = u_xlat1.xyz * ((float3)(_Brightness));
    SV_Target0.xyz = u_xlat1.xyz * u_xlat16_0.xxx + u_xlat16_3.xyz;
    u_xlat16_0.x = u_xlat16_10 * u_xlat16_0.x + u_xlat16_5;
    u_xlat16_0.x = clamp(u_xlat16_0.x, 0.0, 1.0);
    u_xlat1.x = u_xlat16_0.x * vs_COLOR0.w;
    SV_Target0.w = u_xlat1.x;
    return SV_Target0;

	return SV_Target0;
}
#endif
ENDHLSL
		}
	}
}

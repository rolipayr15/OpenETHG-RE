Shader "Custom/DiffuseHighlight" {
	Properties {
		_Color ("Main Color", Color) = (1.0,1.0,1.0,1.0)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		LOD 200
		Tags { "RenderType"="Opaque" }
		Pass {
			Tags { "RenderType"="Opaque" }
			ZWrite On
			ZTest LEqual
			Cull Front
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
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

float4 _MainTex_ST;
float _OutlineWidth;
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float2 vs_TEXCOORD0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0.xyz = ((float3)(_OutlineWidth)) * in_NORMAL0.xyz + in_POSITION0.xyz;
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat1 = MCOL(unity_ObjectToWorld,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * u_xlat0.zzzz + u_xlat1;
    u_xlat0 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat1 = MCOL(unity_MatrixVP,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_MatrixVP,2) * u_xlat0.zzzz + u_xlat1;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat0.wwww + u_xlat1;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 SV_Target0 = 0;

    SV_Target0 = float4(1.0, 1.0, 1.0, 1.0);
    return SV_Target0;

	return SV_Target0;
}
ENDHLSL
		}
		Pass {
			Tags { "RenderType"="Opaque" "ShadowSupport"="true" }
			ZWrite On
			ZTest LEqual
			Cull Back
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#pragma multi_compile _ DIRECTIONAL
#pragma multi_compile _ LIGHTMAP_ON
#pragma multi_compile _ LIGHTPROBE_SH
#pragma multi_compile _ SHADOWS_SCREEN
#pragma multi_compile _ VERTEXLIGHT_ON
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

float _FalloffOffset;
float _FalloffScale;
float4 _MainTex_ST;
#if defined(DIRECTIONAL) && defined(LIGHTMAP_ON) && defined(LIGHTPROBE_SH) && defined(SHADOWS_SCREEN) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_TEXCOORD0 : TEXCOORD0;
	float4 in_TEXCOORD1 : TEXCOORD1;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 in_TEXCOORD1 = genIn.in_TEXCOORD1;
	float4 vs_TEXCOORD0;
	float vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    u_xlat0.x = u_xlat0.x + 0.660000026;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    gl_Position = u_xlat0;
    u_xlat0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    u_xlat0.zw = in_TEXCOORD1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    vs_TEXCOORD0 = u_xlat0;
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 u_xlat0;
	float4 SV_Target0 = 0;

    u_xlat0 = frac(_Time);
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTPROBE_SH) && defined(SHADOWS_SCREEN) && defined(VERTEXLIGHT_ON) && !defined(LIGHTMAP_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0;
	float vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    u_xlat0.x = u_xlat0.x + 0.660000026;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    gl_Position = u_xlat0;
    u_xlat0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat0.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 u_xlat0;
	float4 SV_Target0 = 0;

    u_xlat0 = frac(_Time);
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTMAP_ON) && defined(LIGHTPROBE_SH) && !defined(SHADOWS_SCREEN) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_TEXCOORD0 : TEXCOORD0;
	float4 in_TEXCOORD1 : TEXCOORD1;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 in_TEXCOORD1 = genIn.in_TEXCOORD1;
	float4 vs_TEXCOORD0;
	float vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    u_xlat0.x = u_xlat0.x + 0.660000026;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    gl_Position = u_xlat0;
    u_xlat0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    u_xlat0.zw = in_TEXCOORD1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    vs_TEXCOORD0 = u_xlat0;
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 u_xlat0;
	float4 SV_Target0 = 0;

    u_xlat0 = frac(_Time);
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTPROBE_SH) && defined(SHADOWS_SCREEN) && !defined(LIGHTMAP_ON) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0;
	float vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    u_xlat0.x = u_xlat0.x + 0.660000026;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    gl_Position = u_xlat0;
    u_xlat0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat0.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 u_xlat0;
	float4 SV_Target0 = 0;

    u_xlat0 = frac(_Time);
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTMAP_ON) && defined(SHADOWS_SCREEN) && !defined(LIGHTPROBE_SH) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_TEXCOORD0 : TEXCOORD0;
	float4 in_TEXCOORD1 : TEXCOORD1;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 in_TEXCOORD1 = genIn.in_TEXCOORD1;
	float4 vs_TEXCOORD0;
	float vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    u_xlat0.x = u_xlat0.x + 0.660000026;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    gl_Position = u_xlat0;
    u_xlat0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    u_xlat0.zw = in_TEXCOORD1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    vs_TEXCOORD0 = u_xlat0;
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 u_xlat0;
	float4 SV_Target0 = 0;

    u_xlat0 = frac(_Time);
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTPROBE_SH) && defined(VERTEXLIGHT_ON) && !defined(LIGHTMAP_ON) && !defined(SHADOWS_SCREEN)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0;
	float vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    u_xlat0.x = u_xlat0.x + 0.660000026;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    gl_Position = u_xlat0;
    u_xlat0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat0.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 u_xlat0;
	float4 SV_Target0 = 0;

    u_xlat0 = frac(_Time);
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(SHADOWS_SCREEN) && defined(VERTEXLIGHT_ON) && !defined(LIGHTMAP_ON) && !defined(LIGHTPROBE_SH)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0;
	float vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    u_xlat0.x = u_xlat0.x + 0.660000026;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    gl_Position = u_xlat0;
    u_xlat0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat0.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 u_xlat0;
	float4 SV_Target0 = 0;

    u_xlat0 = frac(_Time);
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTPROBE_SH) && !defined(LIGHTMAP_ON) && !defined(SHADOWS_SCREEN) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0;
	float vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    u_xlat0.x = u_xlat0.x + 0.660000026;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    gl_Position = u_xlat0;
    u_xlat0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat0.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 u_xlat0;
	float4 SV_Target0 = 0;

    u_xlat0 = frac(_Time);
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTMAP_ON) && !defined(LIGHTPROBE_SH) && !defined(SHADOWS_SCREEN) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_TEXCOORD0 : TEXCOORD0;
	float4 in_TEXCOORD1 : TEXCOORD1;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 in_TEXCOORD1 = genIn.in_TEXCOORD1;
	float4 vs_TEXCOORD0;
	float vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    u_xlat0.x = u_xlat0.x + 0.660000026;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    gl_Position = u_xlat0;
    u_xlat0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    u_xlat0.zw = in_TEXCOORD1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    vs_TEXCOORD0 = u_xlat0;
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 u_xlat0;
	float4 SV_Target0 = 0;

    u_xlat0 = frac(_Time);
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(SHADOWS_SCREEN) && !defined(LIGHTMAP_ON) && !defined(LIGHTPROBE_SH) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0;
	float vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    u_xlat0.x = u_xlat0.x + 0.660000026;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    gl_Position = u_xlat0;
    u_xlat0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat0.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 u_xlat0;
	float4 SV_Target0 = 0;

    u_xlat0 = frac(_Time);
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(VERTEXLIGHT_ON) && !defined(LIGHTMAP_ON) && !defined(LIGHTPROBE_SH) && !defined(SHADOWS_SCREEN)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0;
	float vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    u_xlat0.x = u_xlat0.x + 0.660000026;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    gl_Position = u_xlat0;
    u_xlat0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat0.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 u_xlat0;
	float4 SV_Target0 = 0;

    u_xlat0 = frac(_Time);
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && !defined(LIGHTMAP_ON) && !defined(LIGHTPROBE_SH) && !defined(SHADOWS_SCREEN) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0;
	float vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    u_xlat0.x = u_xlat0.x + 0.660000026;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    gl_Position = u_xlat0;
    u_xlat0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat0.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 u_xlat0;
	float4 SV_Target0 = 0;

    u_xlat0 = frac(_Time);
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#else
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0;
	float vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    u_xlat0.x = u_xlat0.x + 0.660000026;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    gl_Position = u_xlat0;
    u_xlat0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat0.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 u_xlat0;
	float4 SV_Target0 = 0;

    u_xlat0 = frac(_Time);
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#endif
ENDHLSL
		}
	}
}

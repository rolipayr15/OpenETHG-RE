Shader "Eyes/TransparentNoFolloffNoLightmapNoLight" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Pass {
			Tags { "LightMode"="FORWARDBASE" "Queue"="Transparent" "RenderType"="Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Cull Back
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

sampler2D _MainTex;
float _DesaturationAdditiveLevel;
float _FalloffOffset;
float _FalloffScale;
float4 _MainTex_ST;
float _ShaderBrightness;
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_TEXCOORD0 : TEXCOORD0;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0 = (float4)0;
	float vs_TEXCOORD1;
	float3 vs_TEXCOORD2;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    u_xlat1.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    vs_TEXCOORD2.xyz = u_xlat0.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    vs_TEXCOORD1 = u_xlat0.x;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float4 u_xlat16_0;
	float3 u_xlat16_1;
	float u_xlat2;
	float3 u_xlat16_4;
	float4 SV_Target0 = 0;

    u_xlat16_0 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat16_1.x = dot(u_xlat16_0.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_4.xyz = u_xlat16_0.xyz + (-u_xlat16_1.xxx);
    u_xlat16_1.xyz = ((float3)(float3(_DesaturationAdditiveLevel, _DesaturationAdditiveLevel, _DesaturationAdditiveLevel))) * u_xlat16_4.xyz + u_xlat16_1.xxx;
    u_xlat2 = _ShaderBrightness + 1.0;
    u_xlat16_0.xyz = u_xlat16_1.xyz * ((float3)(u_xlat2));
    SV_Target0 = u_xlat16_0;
    return SV_Target0;

	return SV_Target0;
}
ENDHLSL
		}
	}
}

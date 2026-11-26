Shader "Eyes/Dog/DogReplacementShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Pass {
			Tags { "RenderType"="Opaque" }
			ZWrite On
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
float3 _FalloffColor;
float _FalloffOffset;
float _FalloffScale;
float _FalloffSmooth;
float4 _MainTex_ST;
float _ShaderBrightness;
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float2 vs_TEXCOORD0 : TEXCOORD0;
	float vs_TEXCOORD3 : TEXCOORD3;
	float vs_COLOR0 : COLOR0;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float2 vs_TEXCOORD0 = (float2)0;
	float vs_TEXCOORD3 = (float)0;
	float vs_COLOR0 = (float)0;
	float4 phase0_Output0_1;
	float2 vs_TEXCOORD1;
	float3 vs_TEXCOORD2;
	float4 u_xlat0;
	float4 u_xlat1;
	float u_xlat16_2;
	float u_xlat9;
	float u_xlat10;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat1 = MCOL(unity_MatrixVP,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_MatrixVP,2) * u_xlat0.zzzz + u_xlat1;
    u_xlat0 = MCOL(unity_MatrixVP,3) * u_xlat0.wwww + u_xlat1;
    u_xlat1.xyz = in_POSITION0.yyy * MCOL(unity_ObjectToWorld,1).xyz;
    u_xlat1.xyz = MCOL(unity_ObjectToWorld,0).xyz * in_POSITION0.xxx + u_xlat1.xyz;
    u_xlat1.xyz = MCOL(unity_ObjectToWorld,2).xyz * in_POSITION0.zzz + u_xlat1.xyz;
    u_xlat1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat1.xyz;
    u_xlat16_2 = u_xlat1.z + u_xlat1.y;
    u_xlat10 = _Time.y * 3.0;
    u_xlat10 = u_xlat16_2 * 3.0 + u_xlat10;
    u_xlat10 = sin(u_xlat10);
    u_xlat0.x = u_xlat10 * 0.100000001 + u_xlat0.x;
    gl_Position = u_xlat0;
    u_xlat0.xyz = u_xlat1.xyz + (-_WorldSpaceCameraPos.xyz);
    u_xlat1.xyz = (-u_xlat1.xyz) + _WorldSpaceCameraPos.xyz;
    u_xlat9 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat9 = sqrt(u_xlat9);
    u_xlat9 = u_xlat9 + u_xlat9;
    u_xlat9 = sin(u_xlat9);
    u_xlat9 = u_xlat9 + 0.200000003;
    vs_COLOR0 = u_xlat9;
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.z = u_xlat0.x * _FalloffScale + _FalloffOffset;
    u_xlat0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    phase0_Output0_1.xyz = u_xlat0.xyz;
    vs_TEXCOORD1.xy = float2(0.0, 0.0);
    vs_TEXCOORD2.xyz = float3(0.0, 0.0, 0.0);
vs_TEXCOORD0 = phase0_Output0_1.xy;
vs_TEXCOORD3 = phase0_Output0_1.z;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD3 = vs_TEXCOORD3;
	genOut.vs_COLOR0 = vs_COLOR0;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float2 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float vs_TEXCOORD3 = genIn.vs_TEXCOORD3;
	float vs_COLOR0 = genIn.vs_COLOR0;
	float3 u_xlat16_0;
	float4 u_xlat1;
	float4 u_xlat16_1;
	float u_xlat2;
	float3 u_xlat16_3;
	float3 u_xlat5;
	float u_xlat16_6;
	float4 SV_Target0 = 0;

    u_xlat16_0.x = vs_TEXCOORD3;
    u_xlat16_0.x = clamp(u_xlat16_0.x, 0.0, 1.0);
    u_xlat16_3.x = u_xlat16_0.x * u_xlat16_0.x;
    u_xlat16_6 = (-u_xlat16_0.x) * 2.0 + 3.0;
    u_xlat16_3.x = u_xlat16_3.x * u_xlat16_6 + (-u_xlat16_0.x);
    u_xlat16_0.x = _FalloffSmooth * u_xlat16_3.x + u_xlat16_0.x;
    u_xlat16_1 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat1 = (-u_xlat16_1) + float4(1.0, 1.0, 1.0, 1.0);
    u_xlat1 = u_xlat1 * ((float4)(vs_COLOR0));
    u_xlat2 = _ShaderBrightness + 1.0;
    u_xlat5.xyz = u_xlat1.xyz * ((float3)(u_xlat2));
    u_xlat16_3.xyz = (-u_xlat1.xyz) * ((float3)(u_xlat2)) + _FalloffColor.xyz;
    SV_Target0.w = u_xlat1.w;
    u_xlat16_0.xyz = u_xlat16_0.xxx * u_xlat16_3.xyz + u_xlat5.xyz;
    SV_Target0.xyz = ((float3)(dot(u_xlat16_0.xyz, float3(0.219999999, 0.707000017, 0.0710000023))));
    return SV_Target0;

	return SV_Target0;
}
ENDHLSL
		}
	}
}

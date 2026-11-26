Shader "Invisible/InvisibleShadowCaster" {
	Properties {
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Pass {
			Name "FORWARD"
			Tags { "LightMode"="FORWARDBASE" "Queue"="Transparent" "RenderType"="Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			ZWrite Off
			ZTest LEqual
			Cull Back
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#pragma multi_compile _ DIRECTIONAL
#pragma multi_compile _ LIGHTMAP_ON
#pragma multi_compile _ LIGHTPROBE_SH
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

#if defined(DIRECTIONAL) && defined(LIGHTMAP_ON) && defined(LIGHTPROBE_SH) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD1 : TEXCOORD1;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD1 = genIn.in_TEXCOORD1;
	float3 vs_TEXCOORD0;
	float3 vs_TEXCOORD1;
	float4 vs_TEXCOORD2;
	float4 u_xlat0;
	float4 u_xlat1;
	float u_xlat6;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    vs_TEXCOORD1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = (1.0/sqrt(u_xlat6));
    vs_TEXCOORD0.xyz = ((float3)(u_xlat6)) * u_xlat0.xyz;
    vs_TEXCOORD2.xy = in_TEXCOORD1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    vs_TEXCOORD2.zw = float2(0.0, 0.0);
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 SV_Target0 = 0;

    SV_Target0 = float4(0.0, 0.0, 0.0, 0.0);
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTPROBE_SH) && defined(VERTEXLIGHT_ON) && !defined(LIGHTMAP_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float3 vs_TEXCOORD0;
	float3 vs_TEXCOORD1;
	float3 vs_TEXCOORD2;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float4 u_xlat16_2;
	float4 u_xlat3;
	float4 u_xlat4;
	float3 u_xlat16_5;
	float3 u_xlat16_6;
	float u_xlat21;
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
    u_xlat1.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat1.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat1.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat21 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat21 = (1.0/sqrt(u_xlat21));
    u_xlat1.xyz = ((float3)(u_xlat21)) * u_xlat1.xyz;
    vs_TEXCOORD0.xyz = u_xlat1.xyz;
    vs_TEXCOORD1.xyz = u_xlat0.xyz;
    u_xlat2 = (-u_xlat0.xxxx) + unity_4LightPosX0;
    u_xlat3 = (-u_xlat0.yyyy) + unity_4LightPosY0;
    u_xlat0 = (-u_xlat0.zzzz) + unity_4LightPosZ0;
    u_xlat4 = u_xlat1.yyyy * u_xlat3;
    u_xlat3 = u_xlat3 * u_xlat3;
    u_xlat3 = u_xlat2 * u_xlat2 + u_xlat3;
    u_xlat2 = u_xlat2 * u_xlat1.xxxx + u_xlat4;
    u_xlat2 = u_xlat0 * u_xlat1.zzzz + u_xlat2;
    u_xlat0 = u_xlat0 * u_xlat0 + u_xlat3;
    u_xlat0 = max(u_xlat0, float4(9.99999997e-07, 9.99999997e-07, 9.99999997e-07, 9.99999997e-07));
    u_xlat3 = (1.0/sqrt(u_xlat0));
    u_xlat0 = u_xlat0 * unity_4LightAtten0 + float4(1.0, 1.0, 1.0, 1.0);
    u_xlat0 = float4(1.0, 1.0, 1.0, 1.0) / u_xlat0;
    u_xlat2 = u_xlat2 * u_xlat3;
    u_xlat2 = max(u_xlat2, float4(0.0, 0.0, 0.0, 0.0));
    u_xlat0 = u_xlat0 * u_xlat2;
    u_xlat2.xyz = u_xlat0.yyy * unity_LightColor[1].xyz;
    u_xlat2.xyz = unity_LightColor[0].xyz * u_xlat0.xxx + u_xlat2.xyz;
    u_xlat0.xyz = unity_LightColor[2].xyz * u_xlat0.zzz + u_xlat2.xyz;
    u_xlat0.xyz = unity_LightColor[3].xyz * u_xlat0.www + u_xlat0.xyz;
    u_xlat16_5.x = u_xlat1.y * u_xlat1.y;
    u_xlat16_5.x = u_xlat1.x * u_xlat1.x + (-u_xlat16_5.x);
    u_xlat16_2 = u_xlat1.yzzx * u_xlat1.xyzz;
    u_xlat16_6.x = dot(unity_SHBr, u_xlat16_2);
    u_xlat16_6.y = dot(unity_SHBg, u_xlat16_2);
    u_xlat16_6.z = dot(unity_SHBb, u_xlat16_2);
    u_xlat16_5.xyz = unity_SHC.xyz * u_xlat16_5.xxx + u_xlat16_6.xyz;
    u_xlat1.w = 1.0;
    u_xlat16_6.x = dot(unity_SHAr, u_xlat1);
    u_xlat16_6.y = dot(unity_SHAg, u_xlat1);
    u_xlat16_6.z = dot(unity_SHAb, u_xlat1);
    u_xlat16_5.xyz = u_xlat16_5.xyz + u_xlat16_6.xyz;
    u_xlat16_5.xyz = max(u_xlat16_5.xyz, float3(0.0, 0.0, 0.0));
    u_xlat1.xyz = log2(u_xlat16_5.xyz);
    u_xlat1.xyz = u_xlat1.xyz * float3(0.416666657, 0.416666657, 0.416666657);
    u_xlat1.xyz = exp2(u_xlat1.xyz);
    u_xlat1.xyz = u_xlat1.xyz * float3(1.05499995, 1.05499995, 1.05499995) + float3(-0.0549999997, -0.0549999997, -0.0549999997);
    u_xlat1.xyz = max(u_xlat1.xyz, float3(0.0, 0.0, 0.0));
    vs_TEXCOORD2.xyz = u_xlat0.xyz + u_xlat1.xyz;
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 SV_Target0 = 0;

    SV_Target0 = float4(0.0, 0.0, 0.0, 0.0);
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTPROBE_SH) && !defined(LIGHTMAP_ON) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float3 vs_TEXCOORD0;
	float3 vs_TEXCOORD1;
	float3 vs_TEXCOORD2;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat16_1;
	float3 u_xlat16_2;
	float3 u_xlat16_3;
	float u_xlat12;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    vs_TEXCOORD1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat12 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat12 = (1.0/sqrt(u_xlat12));
    u_xlat0.xyz = ((float3)(u_xlat12)) * u_xlat0.xyz;
    vs_TEXCOORD0.xyz = u_xlat0.xyz;
    u_xlat16_2.x = u_xlat0.y * u_xlat0.y;
    u_xlat16_2.x = u_xlat0.x * u_xlat0.x + (-u_xlat16_2.x);
    u_xlat16_1 = u_xlat0.yzzx * u_xlat0.xyzz;
    u_xlat16_3.x = dot(unity_SHBr, u_xlat16_1);
    u_xlat16_3.y = dot(unity_SHBg, u_xlat16_1);
    u_xlat16_3.z = dot(unity_SHBb, u_xlat16_1);
    u_xlat16_2.xyz = unity_SHC.xyz * u_xlat16_2.xxx + u_xlat16_3.xyz;
    u_xlat0.w = 1.0;
    u_xlat16_3.x = dot(unity_SHAr, u_xlat0);
    u_xlat16_3.y = dot(unity_SHAg, u_xlat0);
    u_xlat16_3.z = dot(unity_SHAb, u_xlat0);
    u_xlat16_2.xyz = u_xlat16_2.xyz + u_xlat16_3.xyz;
    u_xlat16_2.xyz = max(u_xlat16_2.xyz, float3(0.0, 0.0, 0.0));
    u_xlat0.xyz = log2(u_xlat16_2.xyz);
    u_xlat0.xyz = u_xlat0.xyz * float3(0.416666657, 0.416666657, 0.416666657);
    u_xlat0.xyz = exp2(u_xlat0.xyz);
    u_xlat0.xyz = u_xlat0.xyz * float3(1.05499995, 1.05499995, 1.05499995) + float3(-0.0549999997, -0.0549999997, -0.0549999997);
    u_xlat0.xyz = max(u_xlat0.xyz, float3(0.0, 0.0, 0.0));
    vs_TEXCOORD2.xyz = u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 SV_Target0 = 0;

    SV_Target0 = float4(0.0, 0.0, 0.0, 0.0);
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTMAP_ON) && !defined(LIGHTPROBE_SH) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD1 : TEXCOORD1;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD1 = genIn.in_TEXCOORD1;
	float3 vs_TEXCOORD0;
	float3 vs_TEXCOORD1;
	float4 vs_TEXCOORD2;
	float4 u_xlat0;
	float4 u_xlat1;
	float u_xlat6;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    vs_TEXCOORD1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = (1.0/sqrt(u_xlat6));
    vs_TEXCOORD0.xyz = ((float3)(u_xlat6)) * u_xlat0.xyz;
    vs_TEXCOORD2.xy = in_TEXCOORD1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    vs_TEXCOORD2.zw = float2(0.0, 0.0);
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 SV_Target0 = 0;

    SV_Target0 = float4(0.0, 0.0, 0.0, 0.0);
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && !defined(LIGHTMAP_ON) && !defined(LIGHTPROBE_SH) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float3 vs_TEXCOORD0;
	float3 vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float u_xlat6;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    vs_TEXCOORD1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = (1.0/sqrt(u_xlat6));
    vs_TEXCOORD0.xyz = ((float3)(u_xlat6)) * u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 SV_Target0 = 0;

    SV_Target0 = float4(0.0, 0.0, 0.0, 0.0);
    return SV_Target0;

	return SV_Target0;
}
#else
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float3 vs_TEXCOORD0;
	float3 vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float u_xlat6;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    vs_TEXCOORD1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = (1.0/sqrt(u_xlat6));
    vs_TEXCOORD0.xyz = ((float3)(u_xlat6)) * u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 SV_Target0 = 0;

    SV_Target0 = float4(0.0, 0.0, 0.0, 0.0);
    return SV_Target0;

	return SV_Target0;
}
#endif
ENDHLSL
		}
		Pass {
			Name "FORWARD"
			Tags { "LightMode"="FORWARDADD" "Queue"="Transparent" "RenderType"="Transparent" }
			Blend SrcAlpha One
			ColorMask RGB
			ZWrite Off
			ZTest LEqual
			Cull Back
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 3.0
#pragma multi_compile _ DIRECTIONAL
#pragma multi_compile _ DIRECTIONAL_COOKIE
#pragma multi_compile _ POINT
#pragma multi_compile _ POINT_COOKIE
#pragma multi_compile _ SPOT
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

float4x4 unity_WorldToLight;
#if defined(POINT) && !defined(DIRECTIONAL) && !defined(DIRECTIONAL_COOKIE) && !defined(POINT_COOKIE) && !defined(SPOT)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float3 vs_TEXCOORD0;
	float3 vs_TEXCOORD1;
	float3 vs_TEXCOORD2;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat10;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    u_xlat1.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat1.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat1.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat10 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat10 = (1.0/sqrt(u_xlat10));
    vs_TEXCOORD0.xyz = ((float3)(u_xlat10)) * u_xlat1.xyz;
    vs_TEXCOORD1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0 = MCOL(unity_ObjectToWorld,3) * in_POSITION0.wwww + u_xlat0;
    u_xlat1.xyz = u_xlat0.yyy * MCOL(unity_WorldToLight,1).xyz;
    u_xlat1.xyz = MCOL(unity_WorldToLight,0).xyz * u_xlat0.xxx + u_xlat1.xyz;
    u_xlat0.xyz = MCOL(unity_WorldToLight,2).xyz * u_xlat0.zzz + u_xlat1.xyz;
    vs_TEXCOORD2.xyz = MCOL(unity_WorldToLight,3).xyz * u_xlat0.www + u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 SV_Target0 = 0;

    SV_Target0 = float4(0.0, 0.0, 0.0, 0.0);
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && !defined(DIRECTIONAL_COOKIE) && !defined(POINT) && !defined(POINT_COOKIE) && !defined(SPOT)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float3 vs_TEXCOORD0;
	float3 vs_TEXCOORD1;
	float4 u_xlat0;
	float4 u_xlat1;
	float u_xlat6;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    vs_TEXCOORD1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat0 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat0;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat0;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat6 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat6 = (1.0/sqrt(u_xlat6));
    vs_TEXCOORD0.xyz = ((float3)(u_xlat6)) * u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 SV_Target0 = 0;

    SV_Target0 = float4(0.0, 0.0, 0.0, 0.0);
    return SV_Target0;

	return SV_Target0;
}
#elif defined(SPOT) && !defined(DIRECTIONAL) && !defined(DIRECTIONAL_COOKIE) && !defined(POINT) && !defined(POINT_COOKIE)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float3 vs_TEXCOORD0;
	float3 vs_TEXCOORD1;
	float4 vs_TEXCOORD2;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat10;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    u_xlat1.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat1.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat1.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat10 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat10 = (1.0/sqrt(u_xlat10));
    vs_TEXCOORD0.xyz = ((float3)(u_xlat10)) * u_xlat1.xyz;
    vs_TEXCOORD1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0 = MCOL(unity_ObjectToWorld,3) * in_POSITION0.wwww + u_xlat0;
    u_xlat1 = u_xlat0.yyyy * MCOL(unity_WorldToLight,1);
    u_xlat1 = MCOL(unity_WorldToLight,0) * u_xlat0.xxxx + u_xlat1;
    u_xlat1 = MCOL(unity_WorldToLight,2) * u_xlat0.zzzz + u_xlat1;
    vs_TEXCOORD2 = MCOL(unity_WorldToLight,3) * u_xlat0.wwww + u_xlat1;
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 SV_Target0 = 0;

    SV_Target0 = float4(0.0, 0.0, 0.0, 0.0);
    return SV_Target0;

	return SV_Target0;
}
#elif defined(POINT_COOKIE) && !defined(DIRECTIONAL) && !defined(DIRECTIONAL_COOKIE) && !defined(POINT) && !defined(SPOT)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float3 vs_TEXCOORD0;
	float3 vs_TEXCOORD1;
	float3 vs_TEXCOORD2;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat10;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    u_xlat1.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat1.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat1.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat10 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat10 = (1.0/sqrt(u_xlat10));
    vs_TEXCOORD0.xyz = ((float3)(u_xlat10)) * u_xlat1.xyz;
    vs_TEXCOORD1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0 = MCOL(unity_ObjectToWorld,3) * in_POSITION0.wwww + u_xlat0;
    u_xlat1.xyz = u_xlat0.yyy * MCOL(unity_WorldToLight,1).xyz;
    u_xlat1.xyz = MCOL(unity_WorldToLight,0).xyz * u_xlat0.xxx + u_xlat1.xyz;
    u_xlat0.xyz = MCOL(unity_WorldToLight,2).xyz * u_xlat0.zzz + u_xlat1.xyz;
    vs_TEXCOORD2.xyz = MCOL(unity_WorldToLight,3).xyz * u_xlat0.www + u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 SV_Target0 = 0;

    SV_Target0 = float4(0.0, 0.0, 0.0, 0.0);
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL_COOKIE) && !defined(DIRECTIONAL) && !defined(POINT) && !defined(POINT_COOKIE) && !defined(SPOT)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float3 vs_TEXCOORD0;
	float3 vs_TEXCOORD1;
	float2 vs_TEXCOORD2;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat10;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    u_xlat1.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat1.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat1.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat10 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat10 = (1.0/sqrt(u_xlat10));
    vs_TEXCOORD0.xyz = ((float3)(u_xlat10)) * u_xlat1.xyz;
    vs_TEXCOORD1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0 = MCOL(unity_ObjectToWorld,3) * in_POSITION0.wwww + u_xlat0;
    u_xlat1.xy = u_xlat0.yy * MCOL(unity_WorldToLight,1).xy;
    u_xlat0.xy = MCOL(unity_WorldToLight,0).xy * u_xlat0.xx + u_xlat1.xy;
    u_xlat0.xy = MCOL(unity_WorldToLight,2).xy * u_xlat0.zz + u_xlat0.xy;
    vs_TEXCOORD2.xy = MCOL(unity_WorldToLight,3).xy * u_xlat0.ww + u_xlat0.xy;
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 SV_Target0 = 0;

    SV_Target0 = float4(0.0, 0.0, 0.0, 0.0);
    return SV_Target0;

	return SV_Target0;
}
#else
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float3 vs_TEXCOORD0;
	float3 vs_TEXCOORD1;
	float2 vs_TEXCOORD2;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat10;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    gl_Position = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    u_xlat1.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat1.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat1.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat10 = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat10 = (1.0/sqrt(u_xlat10));
    vs_TEXCOORD0.xyz = ((float3)(u_xlat10)) * u_xlat1.xyz;
    vs_TEXCOORD1.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat0 = MCOL(unity_ObjectToWorld,3) * in_POSITION0.wwww + u_xlat0;
    u_xlat1.xy = u_xlat0.yy * MCOL(unity_WorldToLight,1).xy;
    u_xlat0.xy = MCOL(unity_WorldToLight,0).xy * u_xlat0.xx + u_xlat1.xy;
    u_xlat0.xy = MCOL(unity_WorldToLight,2).xy * u_xlat0.zz + u_xlat0.xy;
    vs_TEXCOORD2.xy = MCOL(unity_WorldToLight,3).xy * u_xlat0.ww + u_xlat0.xy;
    

	genOut.gen_pos = gl_Position;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 SV_Target0 = 0;

    SV_Target0 = float4(0.0, 0.0, 0.0, 0.0);
    return SV_Target0;

	return SV_Target0;
}
#endif
ENDHLSL
		}
	}
	Fallback "Diffuse"
}

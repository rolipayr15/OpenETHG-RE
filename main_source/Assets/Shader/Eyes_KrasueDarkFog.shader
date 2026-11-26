Shader "Eyes/KrasueDarkFog" {
	Properties {
		_MainTex ("Fog Texture", 2D) = "white" {}
		_TintColor ("Tint Color", Color) = (1.0,1.0,1.0,1.0)
		_InvFade ("Soft Particles Factor", Range(0.009999999776482582, 3.0)) = 1.0
	}
	SubShader {
		Tags { "IgnoreProjector"="true" "PreviewType"="Plane" "Queue"="Transparent" "RenderType"="Transparent" }
		Pass {
			Tags { "IgnoreProjector"="true" "PreviewType"="Plane" "Queue"="Transparent" "RenderType"="Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
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

sampler2D _CameraDepthTexture;
sampler2D _MainTex;
float _InvFade;
float4 _MainTex_ST;
float4 _TintColor;
#if defined(SOFTPARTICLES_ON)
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
	float u_xlat2;
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
    vs_COLOR0 = in_COLOR0 * _TintColor;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
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
	float4 u_xlat0;
	float4 u_xlat16_1;
	float4 SV_Target0 = 0;

    u_xlat0.xy = vs_TEXCOORD1.xy / vs_TEXCOORD1.ww;
    u_xlat0.x = tex2D(_CameraDepthTexture, u_xlat0.xy).x;
    u_xlat0.x = _ZBufferParams.z * u_xlat0.x + _ZBufferParams.w;
    u_xlat0.x = float(1.0) / u_xlat0.x;
    u_xlat0.x = u_xlat0.x + (-vs_TEXCOORD1.z);
    u_xlat0.x = u_xlat0.x * _InvFade;
    u_xlat0.x = clamp(u_xlat0.x, 0.0, 1.0);
    u_xlat0.x = u_xlat0.x * vs_COLOR0.w;
    u_xlat16_1 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat0.w = u_xlat0.x * u_xlat16_1.w;
    u_xlat0.xyz = u_xlat16_1.xyz * vs_COLOR0.xyz;
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#elif !defined(SOFTPARTICLES_ON)
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
	float u_xlat2;
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
    vs_COLOR0 = in_COLOR0 * _TintColor;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
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
    u_xlat0 = u_xlat16_0 * vs_COLOR0;
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
	float4 u_xlat0;
	float4 u_xlat1;
	float u_xlat2;
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
    vs_COLOR0 = in_COLOR0 * _TintColor;
    vs_TEXCOORD0.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
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
    u_xlat0 = u_xlat16_0 * vs_COLOR0;
    SV_Target0 = u_xlat0;
    return SV_Target0;

	return SV_Target0;
}
#endif
ENDHLSL
		}
	}
}

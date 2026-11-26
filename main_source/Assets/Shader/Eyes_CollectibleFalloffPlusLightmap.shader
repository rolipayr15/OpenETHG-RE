Shader "Eyes/CollectibleFalloffPlusLightmap" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Main Color", Color) = (1.0,1.0,1.0,1.0)
		_Gain ("Gain (albedo multiplier, 1 = as authored)", Range(0.25, 4.0)) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Pass {
			Tags { "LightMode"="FORWARDBASE" "RenderType"="Opaque" "ShadowSupport"="true" }
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

sampler2D _LightTexture0X;
sampler2D _MainTex;
float3 _AmbientColor;
float _CollectibleBlink;
float _CollectibleBlinkUngated;
float4 _Color;
float _DesaturationAdditiveLevel;
float _DesaturationLevel;
float3 _FalloffColor;
float _FalloffOffset;
float _FalloffScale;
float _FalloffSmooth;
float _FlashlightBoostInterferenceMul;
float _FlashlightFloor;
float _FlashlightIntensity;
float4 _FlashlightMixWeights;
float _Gain;
float _LMScale;
float4 _LightColor0X;
float4 _MainTex_ST;
float _RevealNormalize;
float _ShaderBrightness;
float4 _WorldSpaceLightPos0X;
float4x4 unity_WorldToLightX;
#if defined(DIRECTIONAL) && defined(LIGHTMAP_ON) && defined(LIGHTPROBE_SH) && defined(SHADOWS_SCREEN) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD0 : TEXCOORD0;
	float4 in_TEXCOORD1 : TEXCOORD1;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float vs_TEXCOORD1 : TEXCOORD1;
	float3 vs_TEXCOORD2 : TEXCOORD2;
	float3 vs_TEXCOORD3 : TEXCOORD3;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 in_TEXCOORD1 = genIn.in_TEXCOORD1;
	float4 vs_TEXCOORD0 = (float4)0;
	float vs_TEXCOORD1 = (float)0;
	float3 vs_TEXCOORD2 = (float3)0;
	float3 vs_TEXCOORD3 = (float3)0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat9;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    gl_Position = u_xlat1;
    u_xlat1.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    u_xlat1.zw = in_TEXCOORD1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    vs_TEXCOORD0 = u_xlat1;
    u_xlat1.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    vs_TEXCOORD2.xyz = u_xlat0.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = (1.0/sqrt(u_xlat9));
    u_xlat0.xyz = ((float3)(u_xlat9)) * u_xlat0.xyz;
    vs_TEXCOORD3.xyz = u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	genOut.vs_TEXCOORD3 = vs_TEXCOORD3;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float3 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float3 vs_TEXCOORD3 = genIn.vs_TEXCOORD3;
	float4 u_xlat0;
	float2 u_xlat1;
	float4 u_xlat16_1;
	bool u_xlatb1;
	float4 u_xlat16_2;
	float3 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat16_5;
	float3 u_xlat6;
	float u_xlat16_8;
	float u_xlat16_18;
	float u_xlat16_20;
	float u_xlat16_22;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD2.yyyy * MCOL(unity_WorldToLightX,1);
    u_xlat0 = MCOL(unity_WorldToLightX,0) * vs_TEXCOORD2.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_WorldToLightX,2) * vs_TEXCOORD2.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_WorldToLightX,3);
    u_xlat1.xy = u_xlat0.xy / u_xlat0.ww;
    u_xlat1.xy = u_xlat1.xy + float2(0.5, 0.5);
    u_xlat16_18 = tex2D(_LightTexture0X, u_xlat1.xy).w;
    u_xlatb1 = u_xlat0.z>=0.0;
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat16_2.x = (-u_xlat0.x) + 1.0;
    u_xlat16_2.x = max(u_xlat16_2.x, 0.0);
    u_xlat0.x = u_xlatb1 ? 1.0 : float(0.0);
    u_xlat0.x = u_xlat0.x * u_xlat16_18;
    u_xlat16_8 = (-u_xlat0.x) * u_xlat16_2.x + 1.0;
    u_xlat0.x = u_xlat16_2.x * u_xlat0.x;
    u_xlat16_2.x = _CollectibleBlinkUngated * u_xlat16_8 + u_xlat0.x;
    u_xlat16_2.x = u_xlat16_2.x * _CollectibleBlink;
    u_xlat16_1 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat6.xyz = u_xlat16_1.xyz * ((float3)(_Gain)) + float3(0.150000006, 0.150000006, 0.150000006);
    u_xlat3.xyz = u_xlat16_2.xxx * u_xlat6.xyz;
    u_xlat16_8 = dot(u_xlat3.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_2.xzw = u_xlat6.xyz * u_xlat16_2.xxx + (-((float3)(u_xlat16_8)));
    u_xlat16_2.xyz = ((float3)(float3(_DesaturationAdditiveLevel, _DesaturationAdditiveLevel, _DesaturationAdditiveLevel))) * u_xlat16_2.xzw + ((float3)(u_xlat16_8));
    u_xlat16_20 = u_xlat16_1.w * _Color.w;
    u_xlat16_2.xyz = ((float3)(u_xlat16_20)) * u_xlat16_2.xyz;
    u_xlat16_2.xyz = clamp(u_xlat16_2.xyz, 0.0, 1.0);
    u_xlat6.xyz = (-vs_TEXCOORD2.xyz) + _WorldSpaceLightPos0X.xyz;
    u_xlat3.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat3.x = (1.0/sqrt(u_xlat3.x));
    u_xlat6.xyz = u_xlat6.xyz * u_xlat3.xxx;
    u_xlat16_20 = dot(vs_TEXCOORD3.xyz, vs_TEXCOORD3.xyz);
    u_xlat16_20 = (1.0/sqrt(u_xlat16_20));
    u_xlat16_4.xyz = ((float3)(u_xlat16_20)) * vs_TEXCOORD3.xyz;
    u_xlat6.x = dot(u_xlat16_4.xyz, u_xlat6.xyz);
    u_xlat6.x = clamp(u_xlat6.x, 0.0, 1.0);
    u_xlat6.x = u_xlat6.x * _FlashlightMixWeights.y;
    u_xlat16_4.xyz = UNITY_SAMPLE_TEX2D(unity_Lightmap, vs_TEXCOORD0.zw).xyz;
    u_xlat16_5.xyz = unity_Lightmap_HDR.xxx * u_xlat16_4.xyz + ((float3)(float3(_FlashlightFloor, _FlashlightFloor, _FlashlightFloor)));
    u_xlat16_4.xyz = u_xlat16_4.xyz * unity_Lightmap_HDR.xxx;
    u_xlat16_5.xyz = u_xlat16_5.xyz * ((float3)(float3(_RevealNormalize, _RevealNormalize, _RevealNormalize)));
    u_xlat6.xyz = u_xlat16_5.xyz * _FlashlightMixWeights.xxx + u_xlat6.xxx;
    u_xlat6.xyz = u_xlat6.xyz * _LightColor0X.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat6.xyz;
    u_xlat16_20 = _FlashlightBoostInterferenceMul * _FlashlightIntensity;
    u_xlat0.xyz = u_xlat0.xyz * ((float3)(u_xlat16_20));
    u_xlat0.xyz = u_xlat16_4.xyz * ((float3)(float3(_LMScale, _LMScale, _LMScale))) + u_xlat0.xyz;
    u_xlat16_4.xyz = u_xlat16_1.xyz * ((float3)(_Gain));
    SV_Target0.w = u_xlat16_1.w;
    u_xlat16_5.xyz = u_xlat0.xyz * u_xlat16_4.xyz;
    u_xlat16_20 = dot(u_xlat16_5.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_4.xyz = u_xlat16_4.xyz * u_xlat0.xyz + (-((float3)(u_xlat16_20)));
    u_xlat16_4.xyz = ((float3)(_DesaturationLevel)) * u_xlat16_4.xyz + ((float3)(u_xlat16_20));
    u_xlat0.x = _ShaderBrightness + 1.0;
    u_xlat6.xyz = u_xlat0.xxx * u_xlat16_4.xyz;
    u_xlat16_4.xyz = (-u_xlat16_4.xyz) * u_xlat0.xxx + float3(1.0, 1.0, 1.0);
    u_xlat16_2.xyz = u_xlat16_2.xyz * u_xlat16_4.xyz + u_xlat6.xyz;
    u_xlat16_4.xyz = (-u_xlat16_2.xyz) + _FalloffColor.xyz;
    u_xlat16_20 = vs_TEXCOORD1;
    u_xlat16_20 = clamp(u_xlat16_20, 0.0, 1.0);
    u_xlat16_22 = u_xlat16_20 * u_xlat16_20;
    u_xlat16_5.x = (-u_xlat16_20) * 2.0 + 3.0;
    u_xlat16_22 = u_xlat16_22 * u_xlat16_5.x + (-u_xlat16_20);
    u_xlat16_20 = _FalloffSmooth * u_xlat16_22 + u_xlat16_20;
    SV_Target0.xyz = ((float3)(u_xlat16_20)) * u_xlat16_4.xyz + u_xlat16_2.xyz;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTPROBE_SH) && defined(SHADOWS_SCREEN) && defined(VERTEXLIGHT_ON) && !defined(LIGHTMAP_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float vs_TEXCOORD1 : TEXCOORD1;
	float3 vs_TEXCOORD2 : TEXCOORD2;
	float3 vs_TEXCOORD3 : TEXCOORD3;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0 = (float4)0;
	float vs_TEXCOORD1 = (float)0;
	float3 vs_TEXCOORD2 = (float3)0;
	float3 vs_TEXCOORD3 = (float3)0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat9;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    gl_Position = u_xlat1;
    u_xlat1.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat1.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    u_xlat1.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    vs_TEXCOORD2.xyz = u_xlat0.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = (1.0/sqrt(u_xlat9));
    u_xlat0.xyz = ((float3)(u_xlat9)) * u_xlat0.xyz;
    vs_TEXCOORD3.xyz = u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	genOut.vs_TEXCOORD3 = vs_TEXCOORD3;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float3 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float3 vs_TEXCOORD3 = genIn.vs_TEXCOORD3;
	float4 u_xlat0;
	float2 u_xlat1;
	float4 u_xlat16_1;
	bool u_xlatb1;
	float4 u_xlat16_2;
	float3 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat16_5;
	float3 u_xlat6;
	float u_xlat16_8;
	float u_xlat16_18;
	float u_xlat16_20;
	float u_xlat16_22;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD2.yyyy * MCOL(unity_WorldToLightX,1);
    u_xlat0 = MCOL(unity_WorldToLightX,0) * vs_TEXCOORD2.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_WorldToLightX,2) * vs_TEXCOORD2.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_WorldToLightX,3);
    u_xlat1.xy = u_xlat0.xy / u_xlat0.ww;
    u_xlat1.xy = u_xlat1.xy + float2(0.5, 0.5);
    u_xlat16_18 = tex2D(_LightTexture0X, u_xlat1.xy).w;
    u_xlatb1 = u_xlat0.z>=0.0;
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat16_2.x = (-u_xlat0.x) + 1.0;
    u_xlat16_2.x = max(u_xlat16_2.x, 0.0);
    u_xlat0.x = u_xlatb1 ? 1.0 : float(0.0);
    u_xlat0.x = u_xlat0.x * u_xlat16_18;
    u_xlat16_8 = (-u_xlat0.x) * u_xlat16_2.x + 1.0;
    u_xlat0.x = u_xlat16_2.x * u_xlat0.x;
    u_xlat16_2.x = _CollectibleBlinkUngated * u_xlat16_8 + u_xlat0.x;
    u_xlat16_2.x = u_xlat16_2.x * _CollectibleBlink;
    u_xlat16_1 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat6.xyz = u_xlat16_1.xyz * ((float3)(_Gain)) + float3(0.150000006, 0.150000006, 0.150000006);
    u_xlat3.xyz = u_xlat16_2.xxx * u_xlat6.xyz;
    u_xlat16_8 = dot(u_xlat3.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_2.xzw = u_xlat6.xyz * u_xlat16_2.xxx + (-((float3)(u_xlat16_8)));
    u_xlat16_2.xyz = ((float3)(float3(_DesaturationAdditiveLevel, _DesaturationAdditiveLevel, _DesaturationAdditiveLevel))) * u_xlat16_2.xzw + ((float3)(u_xlat16_8));
    u_xlat16_20 = u_xlat16_1.w * _Color.w;
    u_xlat16_2.xyz = ((float3)(u_xlat16_20)) * u_xlat16_2.xyz;
    u_xlat16_2.xyz = clamp(u_xlat16_2.xyz, 0.0, 1.0);
    u_xlat6.xyz = (-vs_TEXCOORD2.xyz) + _WorldSpaceLightPos0X.xyz;
    u_xlat3.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat3.x = (1.0/sqrt(u_xlat3.x));
    u_xlat6.xyz = u_xlat6.xyz * u_xlat3.xxx;
    u_xlat16_20 = dot(vs_TEXCOORD3.xyz, vs_TEXCOORD3.xyz);
    u_xlat16_20 = (1.0/sqrt(u_xlat16_20));
    u_xlat16_4.xyz = ((float3)(u_xlat16_20)) * vs_TEXCOORD3.xyz;
    u_xlat6.x = dot(u_xlat16_4.xyz, u_xlat6.xyz);
    u_xlat6.x = clamp(u_xlat6.x, 0.0, 1.0);
    u_xlat6.x = u_xlat6.x * _FlashlightMixWeights.y;
    u_xlat16_20 = _FlashlightFloor + 1.0;
    u_xlat16_20 = u_xlat16_20 * _RevealNormalize;
    u_xlat6.x = u_xlat16_20 * _FlashlightMixWeights.x + u_xlat6.x;
    u_xlat6.xyz = u_xlat6.xxx * _LightColor0X.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat6.xyz;
    u_xlat16_20 = _FlashlightBoostInterferenceMul * _FlashlightIntensity;
    u_xlat0.xyz = u_xlat0.xyz * ((float3)(u_xlat16_20)) + _AmbientColor.xyz;
    u_xlat16_4.xyz = u_xlat16_1.xyz * ((float3)(_Gain));
    SV_Target0.w = u_xlat16_1.w;
    u_xlat16_5.xyz = u_xlat0.xyz * u_xlat16_4.xyz;
    u_xlat16_20 = dot(u_xlat16_5.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_4.xyz = u_xlat16_4.xyz * u_xlat0.xyz + (-((float3)(u_xlat16_20)));
    u_xlat16_4.xyz = ((float3)(_DesaturationLevel)) * u_xlat16_4.xyz + ((float3)(u_xlat16_20));
    u_xlat0.x = _ShaderBrightness + 1.0;
    u_xlat6.xyz = u_xlat0.xxx * u_xlat16_4.xyz;
    u_xlat16_4.xyz = (-u_xlat16_4.xyz) * u_xlat0.xxx + float3(1.0, 1.0, 1.0);
    u_xlat16_2.xyz = u_xlat16_2.xyz * u_xlat16_4.xyz + u_xlat6.xyz;
    u_xlat16_4.xyz = (-u_xlat16_2.xyz) + _FalloffColor.xyz;
    u_xlat16_20 = vs_TEXCOORD1;
    u_xlat16_20 = clamp(u_xlat16_20, 0.0, 1.0);
    u_xlat16_22 = u_xlat16_20 * u_xlat16_20;
    u_xlat16_5.x = (-u_xlat16_20) * 2.0 + 3.0;
    u_xlat16_22 = u_xlat16_22 * u_xlat16_5.x + (-u_xlat16_20);
    u_xlat16_20 = _FalloffSmooth * u_xlat16_22 + u_xlat16_20;
    SV_Target0.xyz = ((float3)(u_xlat16_20)) * u_xlat16_4.xyz + u_xlat16_2.xyz;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTMAP_ON) && defined(LIGHTPROBE_SH) && !defined(SHADOWS_SCREEN) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD0 : TEXCOORD0;
	float4 in_TEXCOORD1 : TEXCOORD1;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float vs_TEXCOORD1 : TEXCOORD1;
	float3 vs_TEXCOORD2 : TEXCOORD2;
	float3 vs_TEXCOORD3 : TEXCOORD3;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 in_TEXCOORD1 = genIn.in_TEXCOORD1;
	float4 vs_TEXCOORD0 = (float4)0;
	float vs_TEXCOORD1 = (float)0;
	float3 vs_TEXCOORD2 = (float3)0;
	float3 vs_TEXCOORD3 = (float3)0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat9;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    gl_Position = u_xlat1;
    u_xlat1.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    u_xlat1.zw = in_TEXCOORD1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    vs_TEXCOORD0 = u_xlat1;
    u_xlat1.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    vs_TEXCOORD2.xyz = u_xlat0.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = (1.0/sqrt(u_xlat9));
    u_xlat0.xyz = ((float3)(u_xlat9)) * u_xlat0.xyz;
    vs_TEXCOORD3.xyz = u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	genOut.vs_TEXCOORD3 = vs_TEXCOORD3;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float3 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float3 vs_TEXCOORD3 = genIn.vs_TEXCOORD3;
	float4 u_xlat0;
	float2 u_xlat1;
	float4 u_xlat16_1;
	bool u_xlatb1;
	float4 u_xlat16_2;
	float3 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat16_5;
	float3 u_xlat6;
	float u_xlat16_8;
	float u_xlat16_18;
	float u_xlat16_20;
	float u_xlat16_22;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD2.yyyy * MCOL(unity_WorldToLightX,1);
    u_xlat0 = MCOL(unity_WorldToLightX,0) * vs_TEXCOORD2.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_WorldToLightX,2) * vs_TEXCOORD2.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_WorldToLightX,3);
    u_xlat1.xy = u_xlat0.xy / u_xlat0.ww;
    u_xlat1.xy = u_xlat1.xy + float2(0.5, 0.5);
    u_xlat16_18 = tex2D(_LightTexture0X, u_xlat1.xy).w;
    u_xlatb1 = u_xlat0.z>=0.0;
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat16_2.x = (-u_xlat0.x) + 1.0;
    u_xlat16_2.x = max(u_xlat16_2.x, 0.0);
    u_xlat0.x = u_xlatb1 ? 1.0 : float(0.0);
    u_xlat0.x = u_xlat0.x * u_xlat16_18;
    u_xlat16_8 = (-u_xlat0.x) * u_xlat16_2.x + 1.0;
    u_xlat0.x = u_xlat16_2.x * u_xlat0.x;
    u_xlat16_2.x = _CollectibleBlinkUngated * u_xlat16_8 + u_xlat0.x;
    u_xlat16_2.x = u_xlat16_2.x * _CollectibleBlink;
    u_xlat16_1 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat6.xyz = u_xlat16_1.xyz * ((float3)(_Gain)) + float3(0.150000006, 0.150000006, 0.150000006);
    u_xlat3.xyz = u_xlat16_2.xxx * u_xlat6.xyz;
    u_xlat16_8 = dot(u_xlat3.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_2.xzw = u_xlat6.xyz * u_xlat16_2.xxx + (-((float3)(u_xlat16_8)));
    u_xlat16_2.xyz = ((float3)(float3(_DesaturationAdditiveLevel, _DesaturationAdditiveLevel, _DesaturationAdditiveLevel))) * u_xlat16_2.xzw + ((float3)(u_xlat16_8));
    u_xlat16_20 = u_xlat16_1.w * _Color.w;
    u_xlat16_2.xyz = ((float3)(u_xlat16_20)) * u_xlat16_2.xyz;
    u_xlat16_2.xyz = clamp(u_xlat16_2.xyz, 0.0, 1.0);
    u_xlat6.xyz = (-vs_TEXCOORD2.xyz) + _WorldSpaceLightPos0X.xyz;
    u_xlat3.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat3.x = (1.0/sqrt(u_xlat3.x));
    u_xlat6.xyz = u_xlat6.xyz * u_xlat3.xxx;
    u_xlat16_20 = dot(vs_TEXCOORD3.xyz, vs_TEXCOORD3.xyz);
    u_xlat16_20 = (1.0/sqrt(u_xlat16_20));
    u_xlat16_4.xyz = ((float3)(u_xlat16_20)) * vs_TEXCOORD3.xyz;
    u_xlat6.x = dot(u_xlat16_4.xyz, u_xlat6.xyz);
    u_xlat6.x = clamp(u_xlat6.x, 0.0, 1.0);
    u_xlat6.x = u_xlat6.x * _FlashlightMixWeights.y;
    u_xlat16_4.xyz = UNITY_SAMPLE_TEX2D(unity_Lightmap, vs_TEXCOORD0.zw).xyz;
    u_xlat16_5.xyz = unity_Lightmap_HDR.xxx * u_xlat16_4.xyz + ((float3)(float3(_FlashlightFloor, _FlashlightFloor, _FlashlightFloor)));
    u_xlat16_4.xyz = u_xlat16_4.xyz * unity_Lightmap_HDR.xxx;
    u_xlat16_5.xyz = u_xlat16_5.xyz * ((float3)(float3(_RevealNormalize, _RevealNormalize, _RevealNormalize)));
    u_xlat6.xyz = u_xlat16_5.xyz * _FlashlightMixWeights.xxx + u_xlat6.xxx;
    u_xlat6.xyz = u_xlat6.xyz * _LightColor0X.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat6.xyz;
    u_xlat16_20 = _FlashlightBoostInterferenceMul * _FlashlightIntensity;
    u_xlat0.xyz = u_xlat0.xyz * ((float3)(u_xlat16_20));
    u_xlat0.xyz = u_xlat16_4.xyz * ((float3)(float3(_LMScale, _LMScale, _LMScale))) + u_xlat0.xyz;
    u_xlat16_4.xyz = u_xlat16_1.xyz * ((float3)(_Gain));
    SV_Target0.w = u_xlat16_1.w;
    u_xlat16_5.xyz = u_xlat0.xyz * u_xlat16_4.xyz;
    u_xlat16_20 = dot(u_xlat16_5.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_4.xyz = u_xlat16_4.xyz * u_xlat0.xyz + (-((float3)(u_xlat16_20)));
    u_xlat16_4.xyz = ((float3)(_DesaturationLevel)) * u_xlat16_4.xyz + ((float3)(u_xlat16_20));
    u_xlat0.x = _ShaderBrightness + 1.0;
    u_xlat6.xyz = u_xlat0.xxx * u_xlat16_4.xyz;
    u_xlat16_4.xyz = (-u_xlat16_4.xyz) * u_xlat0.xxx + float3(1.0, 1.0, 1.0);
    u_xlat16_2.xyz = u_xlat16_2.xyz * u_xlat16_4.xyz + u_xlat6.xyz;
    u_xlat16_4.xyz = (-u_xlat16_2.xyz) + _FalloffColor.xyz;
    u_xlat16_20 = vs_TEXCOORD1;
    u_xlat16_20 = clamp(u_xlat16_20, 0.0, 1.0);
    u_xlat16_22 = u_xlat16_20 * u_xlat16_20;
    u_xlat16_5.x = (-u_xlat16_20) * 2.0 + 3.0;
    u_xlat16_22 = u_xlat16_22 * u_xlat16_5.x + (-u_xlat16_20);
    u_xlat16_20 = _FalloffSmooth * u_xlat16_22 + u_xlat16_20;
    SV_Target0.xyz = ((float3)(u_xlat16_20)) * u_xlat16_4.xyz + u_xlat16_2.xyz;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTPROBE_SH) && defined(SHADOWS_SCREEN) && !defined(LIGHTMAP_ON) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float vs_TEXCOORD1 : TEXCOORD1;
	float3 vs_TEXCOORD2 : TEXCOORD2;
	float3 vs_TEXCOORD3 : TEXCOORD3;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0 = (float4)0;
	float vs_TEXCOORD1 = (float)0;
	float3 vs_TEXCOORD2 = (float3)0;
	float3 vs_TEXCOORD3 = (float3)0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat9;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    gl_Position = u_xlat1;
    u_xlat1.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat1.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    u_xlat1.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    vs_TEXCOORD2.xyz = u_xlat0.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = (1.0/sqrt(u_xlat9));
    u_xlat0.xyz = ((float3)(u_xlat9)) * u_xlat0.xyz;
    vs_TEXCOORD3.xyz = u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	genOut.vs_TEXCOORD3 = vs_TEXCOORD3;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float3 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float3 vs_TEXCOORD3 = genIn.vs_TEXCOORD3;
	float4 u_xlat0;
	float2 u_xlat1;
	float4 u_xlat16_1;
	bool u_xlatb1;
	float4 u_xlat16_2;
	float3 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat16_5;
	float3 u_xlat6;
	float u_xlat16_8;
	float u_xlat16_18;
	float u_xlat16_20;
	float u_xlat16_22;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD2.yyyy * MCOL(unity_WorldToLightX,1);
    u_xlat0 = MCOL(unity_WorldToLightX,0) * vs_TEXCOORD2.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_WorldToLightX,2) * vs_TEXCOORD2.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_WorldToLightX,3);
    u_xlat1.xy = u_xlat0.xy / u_xlat0.ww;
    u_xlat1.xy = u_xlat1.xy + float2(0.5, 0.5);
    u_xlat16_18 = tex2D(_LightTexture0X, u_xlat1.xy).w;
    u_xlatb1 = u_xlat0.z>=0.0;
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat16_2.x = (-u_xlat0.x) + 1.0;
    u_xlat16_2.x = max(u_xlat16_2.x, 0.0);
    u_xlat0.x = u_xlatb1 ? 1.0 : float(0.0);
    u_xlat0.x = u_xlat0.x * u_xlat16_18;
    u_xlat16_8 = (-u_xlat0.x) * u_xlat16_2.x + 1.0;
    u_xlat0.x = u_xlat16_2.x * u_xlat0.x;
    u_xlat16_2.x = _CollectibleBlinkUngated * u_xlat16_8 + u_xlat0.x;
    u_xlat16_2.x = u_xlat16_2.x * _CollectibleBlink;
    u_xlat16_1 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat6.xyz = u_xlat16_1.xyz * ((float3)(_Gain)) + float3(0.150000006, 0.150000006, 0.150000006);
    u_xlat3.xyz = u_xlat16_2.xxx * u_xlat6.xyz;
    u_xlat16_8 = dot(u_xlat3.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_2.xzw = u_xlat6.xyz * u_xlat16_2.xxx + (-((float3)(u_xlat16_8)));
    u_xlat16_2.xyz = ((float3)(float3(_DesaturationAdditiveLevel, _DesaturationAdditiveLevel, _DesaturationAdditiveLevel))) * u_xlat16_2.xzw + ((float3)(u_xlat16_8));
    u_xlat16_20 = u_xlat16_1.w * _Color.w;
    u_xlat16_2.xyz = ((float3)(u_xlat16_20)) * u_xlat16_2.xyz;
    u_xlat16_2.xyz = clamp(u_xlat16_2.xyz, 0.0, 1.0);
    u_xlat6.xyz = (-vs_TEXCOORD2.xyz) + _WorldSpaceLightPos0X.xyz;
    u_xlat3.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat3.x = (1.0/sqrt(u_xlat3.x));
    u_xlat6.xyz = u_xlat6.xyz * u_xlat3.xxx;
    u_xlat16_20 = dot(vs_TEXCOORD3.xyz, vs_TEXCOORD3.xyz);
    u_xlat16_20 = (1.0/sqrt(u_xlat16_20));
    u_xlat16_4.xyz = ((float3)(u_xlat16_20)) * vs_TEXCOORD3.xyz;
    u_xlat6.x = dot(u_xlat16_4.xyz, u_xlat6.xyz);
    u_xlat6.x = clamp(u_xlat6.x, 0.0, 1.0);
    u_xlat6.x = u_xlat6.x * _FlashlightMixWeights.y;
    u_xlat16_20 = _FlashlightFloor + 1.0;
    u_xlat16_20 = u_xlat16_20 * _RevealNormalize;
    u_xlat6.x = u_xlat16_20 * _FlashlightMixWeights.x + u_xlat6.x;
    u_xlat6.xyz = u_xlat6.xxx * _LightColor0X.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat6.xyz;
    u_xlat16_20 = _FlashlightBoostInterferenceMul * _FlashlightIntensity;
    u_xlat0.xyz = u_xlat0.xyz * ((float3)(u_xlat16_20)) + _AmbientColor.xyz;
    u_xlat16_4.xyz = u_xlat16_1.xyz * ((float3)(_Gain));
    SV_Target0.w = u_xlat16_1.w;
    u_xlat16_5.xyz = u_xlat0.xyz * u_xlat16_4.xyz;
    u_xlat16_20 = dot(u_xlat16_5.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_4.xyz = u_xlat16_4.xyz * u_xlat0.xyz + (-((float3)(u_xlat16_20)));
    u_xlat16_4.xyz = ((float3)(_DesaturationLevel)) * u_xlat16_4.xyz + ((float3)(u_xlat16_20));
    u_xlat0.x = _ShaderBrightness + 1.0;
    u_xlat6.xyz = u_xlat0.xxx * u_xlat16_4.xyz;
    u_xlat16_4.xyz = (-u_xlat16_4.xyz) * u_xlat0.xxx + float3(1.0, 1.0, 1.0);
    u_xlat16_2.xyz = u_xlat16_2.xyz * u_xlat16_4.xyz + u_xlat6.xyz;
    u_xlat16_4.xyz = (-u_xlat16_2.xyz) + _FalloffColor.xyz;
    u_xlat16_20 = vs_TEXCOORD1;
    u_xlat16_20 = clamp(u_xlat16_20, 0.0, 1.0);
    u_xlat16_22 = u_xlat16_20 * u_xlat16_20;
    u_xlat16_5.x = (-u_xlat16_20) * 2.0 + 3.0;
    u_xlat16_22 = u_xlat16_22 * u_xlat16_5.x + (-u_xlat16_20);
    u_xlat16_20 = _FalloffSmooth * u_xlat16_22 + u_xlat16_20;
    SV_Target0.xyz = ((float3)(u_xlat16_20)) * u_xlat16_4.xyz + u_xlat16_2.xyz;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTMAP_ON) && defined(SHADOWS_SCREEN) && !defined(LIGHTPROBE_SH) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD0 : TEXCOORD0;
	float4 in_TEXCOORD1 : TEXCOORD1;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float vs_TEXCOORD1 : TEXCOORD1;
	float3 vs_TEXCOORD2 : TEXCOORD2;
	float3 vs_TEXCOORD3 : TEXCOORD3;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 in_TEXCOORD1 = genIn.in_TEXCOORD1;
	float4 vs_TEXCOORD0 = (float4)0;
	float vs_TEXCOORD1 = (float)0;
	float3 vs_TEXCOORD2 = (float3)0;
	float3 vs_TEXCOORD3 = (float3)0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat9;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    gl_Position = u_xlat1;
    u_xlat1.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    u_xlat1.zw = in_TEXCOORD1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    vs_TEXCOORD0 = u_xlat1;
    u_xlat1.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    vs_TEXCOORD2.xyz = u_xlat0.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = (1.0/sqrt(u_xlat9));
    u_xlat0.xyz = ((float3)(u_xlat9)) * u_xlat0.xyz;
    vs_TEXCOORD3.xyz = u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	genOut.vs_TEXCOORD3 = vs_TEXCOORD3;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float3 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float3 vs_TEXCOORD3 = genIn.vs_TEXCOORD3;
	float4 u_xlat0;
	float2 u_xlat1;
	float4 u_xlat16_1;
	bool u_xlatb1;
	float4 u_xlat16_2;
	float3 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat16_5;
	float3 u_xlat6;
	float u_xlat16_8;
	float u_xlat16_18;
	float u_xlat16_20;
	float u_xlat16_22;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD2.yyyy * MCOL(unity_WorldToLightX,1);
    u_xlat0 = MCOL(unity_WorldToLightX,0) * vs_TEXCOORD2.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_WorldToLightX,2) * vs_TEXCOORD2.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_WorldToLightX,3);
    u_xlat1.xy = u_xlat0.xy / u_xlat0.ww;
    u_xlat1.xy = u_xlat1.xy + float2(0.5, 0.5);
    u_xlat16_18 = tex2D(_LightTexture0X, u_xlat1.xy).w;
    u_xlatb1 = u_xlat0.z>=0.0;
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat16_2.x = (-u_xlat0.x) + 1.0;
    u_xlat16_2.x = max(u_xlat16_2.x, 0.0);
    u_xlat0.x = u_xlatb1 ? 1.0 : float(0.0);
    u_xlat0.x = u_xlat0.x * u_xlat16_18;
    u_xlat16_8 = (-u_xlat0.x) * u_xlat16_2.x + 1.0;
    u_xlat0.x = u_xlat16_2.x * u_xlat0.x;
    u_xlat16_2.x = _CollectibleBlinkUngated * u_xlat16_8 + u_xlat0.x;
    u_xlat16_2.x = u_xlat16_2.x * _CollectibleBlink;
    u_xlat16_1 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat6.xyz = u_xlat16_1.xyz * ((float3)(_Gain)) + float3(0.150000006, 0.150000006, 0.150000006);
    u_xlat3.xyz = u_xlat16_2.xxx * u_xlat6.xyz;
    u_xlat16_8 = dot(u_xlat3.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_2.xzw = u_xlat6.xyz * u_xlat16_2.xxx + (-((float3)(u_xlat16_8)));
    u_xlat16_2.xyz = ((float3)(float3(_DesaturationAdditiveLevel, _DesaturationAdditiveLevel, _DesaturationAdditiveLevel))) * u_xlat16_2.xzw + ((float3)(u_xlat16_8));
    u_xlat16_20 = u_xlat16_1.w * _Color.w;
    u_xlat16_2.xyz = ((float3)(u_xlat16_20)) * u_xlat16_2.xyz;
    u_xlat16_2.xyz = clamp(u_xlat16_2.xyz, 0.0, 1.0);
    u_xlat6.xyz = (-vs_TEXCOORD2.xyz) + _WorldSpaceLightPos0X.xyz;
    u_xlat3.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat3.x = (1.0/sqrt(u_xlat3.x));
    u_xlat6.xyz = u_xlat6.xyz * u_xlat3.xxx;
    u_xlat16_20 = dot(vs_TEXCOORD3.xyz, vs_TEXCOORD3.xyz);
    u_xlat16_20 = (1.0/sqrt(u_xlat16_20));
    u_xlat16_4.xyz = ((float3)(u_xlat16_20)) * vs_TEXCOORD3.xyz;
    u_xlat6.x = dot(u_xlat16_4.xyz, u_xlat6.xyz);
    u_xlat6.x = clamp(u_xlat6.x, 0.0, 1.0);
    u_xlat6.x = u_xlat6.x * _FlashlightMixWeights.y;
    u_xlat16_4.xyz = UNITY_SAMPLE_TEX2D(unity_Lightmap, vs_TEXCOORD0.zw).xyz;
    u_xlat16_5.xyz = unity_Lightmap_HDR.xxx * u_xlat16_4.xyz + ((float3)(float3(_FlashlightFloor, _FlashlightFloor, _FlashlightFloor)));
    u_xlat16_4.xyz = u_xlat16_4.xyz * unity_Lightmap_HDR.xxx;
    u_xlat16_5.xyz = u_xlat16_5.xyz * ((float3)(float3(_RevealNormalize, _RevealNormalize, _RevealNormalize)));
    u_xlat6.xyz = u_xlat16_5.xyz * _FlashlightMixWeights.xxx + u_xlat6.xxx;
    u_xlat6.xyz = u_xlat6.xyz * _LightColor0X.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat6.xyz;
    u_xlat16_20 = _FlashlightBoostInterferenceMul * _FlashlightIntensity;
    u_xlat0.xyz = u_xlat0.xyz * ((float3)(u_xlat16_20));
    u_xlat0.xyz = u_xlat16_4.xyz * ((float3)(float3(_LMScale, _LMScale, _LMScale))) + u_xlat0.xyz;
    u_xlat16_4.xyz = u_xlat16_1.xyz * ((float3)(_Gain));
    SV_Target0.w = u_xlat16_1.w;
    u_xlat16_5.xyz = u_xlat0.xyz * u_xlat16_4.xyz;
    u_xlat16_20 = dot(u_xlat16_5.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_4.xyz = u_xlat16_4.xyz * u_xlat0.xyz + (-((float3)(u_xlat16_20)));
    u_xlat16_4.xyz = ((float3)(_DesaturationLevel)) * u_xlat16_4.xyz + ((float3)(u_xlat16_20));
    u_xlat0.x = _ShaderBrightness + 1.0;
    u_xlat6.xyz = u_xlat0.xxx * u_xlat16_4.xyz;
    u_xlat16_4.xyz = (-u_xlat16_4.xyz) * u_xlat0.xxx + float3(1.0, 1.0, 1.0);
    u_xlat16_2.xyz = u_xlat16_2.xyz * u_xlat16_4.xyz + u_xlat6.xyz;
    u_xlat16_4.xyz = (-u_xlat16_2.xyz) + _FalloffColor.xyz;
    u_xlat16_20 = vs_TEXCOORD1;
    u_xlat16_20 = clamp(u_xlat16_20, 0.0, 1.0);
    u_xlat16_22 = u_xlat16_20 * u_xlat16_20;
    u_xlat16_5.x = (-u_xlat16_20) * 2.0 + 3.0;
    u_xlat16_22 = u_xlat16_22 * u_xlat16_5.x + (-u_xlat16_20);
    u_xlat16_20 = _FalloffSmooth * u_xlat16_22 + u_xlat16_20;
    SV_Target0.xyz = ((float3)(u_xlat16_20)) * u_xlat16_4.xyz + u_xlat16_2.xyz;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTPROBE_SH) && defined(VERTEXLIGHT_ON) && !defined(LIGHTMAP_ON) && !defined(SHADOWS_SCREEN)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float vs_TEXCOORD1 : TEXCOORD1;
	float3 vs_TEXCOORD2 : TEXCOORD2;
	float3 vs_TEXCOORD3 : TEXCOORD3;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0 = (float4)0;
	float vs_TEXCOORD1 = (float)0;
	float3 vs_TEXCOORD2 = (float3)0;
	float3 vs_TEXCOORD3 = (float3)0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat9;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    gl_Position = u_xlat1;
    u_xlat1.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat1.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    u_xlat1.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    vs_TEXCOORD2.xyz = u_xlat0.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = (1.0/sqrt(u_xlat9));
    u_xlat0.xyz = ((float3)(u_xlat9)) * u_xlat0.xyz;
    vs_TEXCOORD3.xyz = u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	genOut.vs_TEXCOORD3 = vs_TEXCOORD3;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float3 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float3 vs_TEXCOORD3 = genIn.vs_TEXCOORD3;
	float4 u_xlat0;
	float2 u_xlat1;
	float4 u_xlat16_1;
	bool u_xlatb1;
	float4 u_xlat16_2;
	float3 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat16_5;
	float3 u_xlat6;
	float u_xlat16_8;
	float u_xlat16_18;
	float u_xlat16_20;
	float u_xlat16_22;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD2.yyyy * MCOL(unity_WorldToLightX,1);
    u_xlat0 = MCOL(unity_WorldToLightX,0) * vs_TEXCOORD2.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_WorldToLightX,2) * vs_TEXCOORD2.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_WorldToLightX,3);
    u_xlat1.xy = u_xlat0.xy / u_xlat0.ww;
    u_xlat1.xy = u_xlat1.xy + float2(0.5, 0.5);
    u_xlat16_18 = tex2D(_LightTexture0X, u_xlat1.xy).w;
    u_xlatb1 = u_xlat0.z>=0.0;
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat16_2.x = (-u_xlat0.x) + 1.0;
    u_xlat16_2.x = max(u_xlat16_2.x, 0.0);
    u_xlat0.x = u_xlatb1 ? 1.0 : float(0.0);
    u_xlat0.x = u_xlat0.x * u_xlat16_18;
    u_xlat16_8 = (-u_xlat0.x) * u_xlat16_2.x + 1.0;
    u_xlat0.x = u_xlat16_2.x * u_xlat0.x;
    u_xlat16_2.x = _CollectibleBlinkUngated * u_xlat16_8 + u_xlat0.x;
    u_xlat16_2.x = u_xlat16_2.x * _CollectibleBlink;
    u_xlat16_1 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat6.xyz = u_xlat16_1.xyz * ((float3)(_Gain)) + float3(0.150000006, 0.150000006, 0.150000006);
    u_xlat3.xyz = u_xlat16_2.xxx * u_xlat6.xyz;
    u_xlat16_8 = dot(u_xlat3.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_2.xzw = u_xlat6.xyz * u_xlat16_2.xxx + (-((float3)(u_xlat16_8)));
    u_xlat16_2.xyz = ((float3)(float3(_DesaturationAdditiveLevel, _DesaturationAdditiveLevel, _DesaturationAdditiveLevel))) * u_xlat16_2.xzw + ((float3)(u_xlat16_8));
    u_xlat16_20 = u_xlat16_1.w * _Color.w;
    u_xlat16_2.xyz = ((float3)(u_xlat16_20)) * u_xlat16_2.xyz;
    u_xlat16_2.xyz = clamp(u_xlat16_2.xyz, 0.0, 1.0);
    u_xlat6.xyz = (-vs_TEXCOORD2.xyz) + _WorldSpaceLightPos0X.xyz;
    u_xlat3.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat3.x = (1.0/sqrt(u_xlat3.x));
    u_xlat6.xyz = u_xlat6.xyz * u_xlat3.xxx;
    u_xlat16_20 = dot(vs_TEXCOORD3.xyz, vs_TEXCOORD3.xyz);
    u_xlat16_20 = (1.0/sqrt(u_xlat16_20));
    u_xlat16_4.xyz = ((float3)(u_xlat16_20)) * vs_TEXCOORD3.xyz;
    u_xlat6.x = dot(u_xlat16_4.xyz, u_xlat6.xyz);
    u_xlat6.x = clamp(u_xlat6.x, 0.0, 1.0);
    u_xlat6.x = u_xlat6.x * _FlashlightMixWeights.y;
    u_xlat16_20 = _FlashlightFloor + 1.0;
    u_xlat16_20 = u_xlat16_20 * _RevealNormalize;
    u_xlat6.x = u_xlat16_20 * _FlashlightMixWeights.x + u_xlat6.x;
    u_xlat6.xyz = u_xlat6.xxx * _LightColor0X.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat6.xyz;
    u_xlat16_20 = _FlashlightBoostInterferenceMul * _FlashlightIntensity;
    u_xlat0.xyz = u_xlat0.xyz * ((float3)(u_xlat16_20)) + _AmbientColor.xyz;
    u_xlat16_4.xyz = u_xlat16_1.xyz * ((float3)(_Gain));
    SV_Target0.w = u_xlat16_1.w;
    u_xlat16_5.xyz = u_xlat0.xyz * u_xlat16_4.xyz;
    u_xlat16_20 = dot(u_xlat16_5.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_4.xyz = u_xlat16_4.xyz * u_xlat0.xyz + (-((float3)(u_xlat16_20)));
    u_xlat16_4.xyz = ((float3)(_DesaturationLevel)) * u_xlat16_4.xyz + ((float3)(u_xlat16_20));
    u_xlat0.x = _ShaderBrightness + 1.0;
    u_xlat6.xyz = u_xlat0.xxx * u_xlat16_4.xyz;
    u_xlat16_4.xyz = (-u_xlat16_4.xyz) * u_xlat0.xxx + float3(1.0, 1.0, 1.0);
    u_xlat16_2.xyz = u_xlat16_2.xyz * u_xlat16_4.xyz + u_xlat6.xyz;
    u_xlat16_4.xyz = (-u_xlat16_2.xyz) + _FalloffColor.xyz;
    u_xlat16_20 = vs_TEXCOORD1;
    u_xlat16_20 = clamp(u_xlat16_20, 0.0, 1.0);
    u_xlat16_22 = u_xlat16_20 * u_xlat16_20;
    u_xlat16_5.x = (-u_xlat16_20) * 2.0 + 3.0;
    u_xlat16_22 = u_xlat16_22 * u_xlat16_5.x + (-u_xlat16_20);
    u_xlat16_20 = _FalloffSmooth * u_xlat16_22 + u_xlat16_20;
    SV_Target0.xyz = ((float3)(u_xlat16_20)) * u_xlat16_4.xyz + u_xlat16_2.xyz;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(SHADOWS_SCREEN) && defined(VERTEXLIGHT_ON) && !defined(LIGHTMAP_ON) && !defined(LIGHTPROBE_SH)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float vs_TEXCOORD1 : TEXCOORD1;
	float3 vs_TEXCOORD2 : TEXCOORD2;
	float3 vs_TEXCOORD3 : TEXCOORD3;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0 = (float4)0;
	float vs_TEXCOORD1 = (float)0;
	float3 vs_TEXCOORD2 = (float3)0;
	float3 vs_TEXCOORD3 = (float3)0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat9;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    gl_Position = u_xlat1;
    u_xlat1.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat1.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    u_xlat1.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    vs_TEXCOORD2.xyz = u_xlat0.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = (1.0/sqrt(u_xlat9));
    u_xlat0.xyz = ((float3)(u_xlat9)) * u_xlat0.xyz;
    vs_TEXCOORD3.xyz = u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	genOut.vs_TEXCOORD3 = vs_TEXCOORD3;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float3 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float3 vs_TEXCOORD3 = genIn.vs_TEXCOORD3;
	float4 u_xlat0;
	float2 u_xlat1;
	float4 u_xlat16_1;
	bool u_xlatb1;
	float4 u_xlat16_2;
	float3 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat16_5;
	float3 u_xlat6;
	float u_xlat16_8;
	float u_xlat16_18;
	float u_xlat16_20;
	float u_xlat16_22;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD2.yyyy * MCOL(unity_WorldToLightX,1);
    u_xlat0 = MCOL(unity_WorldToLightX,0) * vs_TEXCOORD2.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_WorldToLightX,2) * vs_TEXCOORD2.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_WorldToLightX,3);
    u_xlat1.xy = u_xlat0.xy / u_xlat0.ww;
    u_xlat1.xy = u_xlat1.xy + float2(0.5, 0.5);
    u_xlat16_18 = tex2D(_LightTexture0X, u_xlat1.xy).w;
    u_xlatb1 = u_xlat0.z>=0.0;
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat16_2.x = (-u_xlat0.x) + 1.0;
    u_xlat16_2.x = max(u_xlat16_2.x, 0.0);
    u_xlat0.x = u_xlatb1 ? 1.0 : float(0.0);
    u_xlat0.x = u_xlat0.x * u_xlat16_18;
    u_xlat16_8 = (-u_xlat0.x) * u_xlat16_2.x + 1.0;
    u_xlat0.x = u_xlat16_2.x * u_xlat0.x;
    u_xlat16_2.x = _CollectibleBlinkUngated * u_xlat16_8 + u_xlat0.x;
    u_xlat16_2.x = u_xlat16_2.x * _CollectibleBlink;
    u_xlat16_1 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat6.xyz = u_xlat16_1.xyz * ((float3)(_Gain)) + float3(0.150000006, 0.150000006, 0.150000006);
    u_xlat3.xyz = u_xlat16_2.xxx * u_xlat6.xyz;
    u_xlat16_8 = dot(u_xlat3.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_2.xzw = u_xlat6.xyz * u_xlat16_2.xxx + (-((float3)(u_xlat16_8)));
    u_xlat16_2.xyz = ((float3)(float3(_DesaturationAdditiveLevel, _DesaturationAdditiveLevel, _DesaturationAdditiveLevel))) * u_xlat16_2.xzw + ((float3)(u_xlat16_8));
    u_xlat16_20 = u_xlat16_1.w * _Color.w;
    u_xlat16_2.xyz = ((float3)(u_xlat16_20)) * u_xlat16_2.xyz;
    u_xlat16_2.xyz = clamp(u_xlat16_2.xyz, 0.0, 1.0);
    u_xlat6.xyz = (-vs_TEXCOORD2.xyz) + _WorldSpaceLightPos0X.xyz;
    u_xlat3.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat3.x = (1.0/sqrt(u_xlat3.x));
    u_xlat6.xyz = u_xlat6.xyz * u_xlat3.xxx;
    u_xlat16_20 = dot(vs_TEXCOORD3.xyz, vs_TEXCOORD3.xyz);
    u_xlat16_20 = (1.0/sqrt(u_xlat16_20));
    u_xlat16_4.xyz = ((float3)(u_xlat16_20)) * vs_TEXCOORD3.xyz;
    u_xlat6.x = dot(u_xlat16_4.xyz, u_xlat6.xyz);
    u_xlat6.x = clamp(u_xlat6.x, 0.0, 1.0);
    u_xlat6.x = u_xlat6.x * _FlashlightMixWeights.y;
    u_xlat16_20 = _FlashlightFloor + 1.0;
    u_xlat16_20 = u_xlat16_20 * _RevealNormalize;
    u_xlat6.x = u_xlat16_20 * _FlashlightMixWeights.x + u_xlat6.x;
    u_xlat6.xyz = u_xlat6.xxx * _LightColor0X.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat6.xyz;
    u_xlat16_20 = _FlashlightBoostInterferenceMul * _FlashlightIntensity;
    u_xlat0.xyz = u_xlat0.xyz * ((float3)(u_xlat16_20)) + _AmbientColor.xyz;
    u_xlat16_4.xyz = u_xlat16_1.xyz * ((float3)(_Gain));
    SV_Target0.w = u_xlat16_1.w;
    u_xlat16_5.xyz = u_xlat0.xyz * u_xlat16_4.xyz;
    u_xlat16_20 = dot(u_xlat16_5.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_4.xyz = u_xlat16_4.xyz * u_xlat0.xyz + (-((float3)(u_xlat16_20)));
    u_xlat16_4.xyz = ((float3)(_DesaturationLevel)) * u_xlat16_4.xyz + ((float3)(u_xlat16_20));
    u_xlat0.x = _ShaderBrightness + 1.0;
    u_xlat6.xyz = u_xlat0.xxx * u_xlat16_4.xyz;
    u_xlat16_4.xyz = (-u_xlat16_4.xyz) * u_xlat0.xxx + float3(1.0, 1.0, 1.0);
    u_xlat16_2.xyz = u_xlat16_2.xyz * u_xlat16_4.xyz + u_xlat6.xyz;
    u_xlat16_4.xyz = (-u_xlat16_2.xyz) + _FalloffColor.xyz;
    u_xlat16_20 = vs_TEXCOORD1;
    u_xlat16_20 = clamp(u_xlat16_20, 0.0, 1.0);
    u_xlat16_22 = u_xlat16_20 * u_xlat16_20;
    u_xlat16_5.x = (-u_xlat16_20) * 2.0 + 3.0;
    u_xlat16_22 = u_xlat16_22 * u_xlat16_5.x + (-u_xlat16_20);
    u_xlat16_20 = _FalloffSmooth * u_xlat16_22 + u_xlat16_20;
    SV_Target0.xyz = ((float3)(u_xlat16_20)) * u_xlat16_4.xyz + u_xlat16_2.xyz;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTPROBE_SH) && !defined(LIGHTMAP_ON) && !defined(SHADOWS_SCREEN) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float vs_TEXCOORD1 : TEXCOORD1;
	float3 vs_TEXCOORD2 : TEXCOORD2;
	float3 vs_TEXCOORD3 : TEXCOORD3;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0 = (float4)0;
	float vs_TEXCOORD1 = (float)0;
	float3 vs_TEXCOORD2 = (float3)0;
	float3 vs_TEXCOORD3 = (float3)0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat9;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    gl_Position = u_xlat1;
    u_xlat1.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat1.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    u_xlat1.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    vs_TEXCOORD2.xyz = u_xlat0.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = (1.0/sqrt(u_xlat9));
    u_xlat0.xyz = ((float3)(u_xlat9)) * u_xlat0.xyz;
    vs_TEXCOORD3.xyz = u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	genOut.vs_TEXCOORD3 = vs_TEXCOORD3;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float3 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float3 vs_TEXCOORD3 = genIn.vs_TEXCOORD3;
	float4 u_xlat0;
	float2 u_xlat1;
	float4 u_xlat16_1;
	bool u_xlatb1;
	float4 u_xlat16_2;
	float3 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat16_5;
	float3 u_xlat6;
	float u_xlat16_8;
	float u_xlat16_18;
	float u_xlat16_20;
	float u_xlat16_22;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD2.yyyy * MCOL(unity_WorldToLightX,1);
    u_xlat0 = MCOL(unity_WorldToLightX,0) * vs_TEXCOORD2.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_WorldToLightX,2) * vs_TEXCOORD2.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_WorldToLightX,3);
    u_xlat1.xy = u_xlat0.xy / u_xlat0.ww;
    u_xlat1.xy = u_xlat1.xy + float2(0.5, 0.5);
    u_xlat16_18 = tex2D(_LightTexture0X, u_xlat1.xy).w;
    u_xlatb1 = u_xlat0.z>=0.0;
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat16_2.x = (-u_xlat0.x) + 1.0;
    u_xlat16_2.x = max(u_xlat16_2.x, 0.0);
    u_xlat0.x = u_xlatb1 ? 1.0 : float(0.0);
    u_xlat0.x = u_xlat0.x * u_xlat16_18;
    u_xlat16_8 = (-u_xlat0.x) * u_xlat16_2.x + 1.0;
    u_xlat0.x = u_xlat16_2.x * u_xlat0.x;
    u_xlat16_2.x = _CollectibleBlinkUngated * u_xlat16_8 + u_xlat0.x;
    u_xlat16_2.x = u_xlat16_2.x * _CollectibleBlink;
    u_xlat16_1 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat6.xyz = u_xlat16_1.xyz * ((float3)(_Gain)) + float3(0.150000006, 0.150000006, 0.150000006);
    u_xlat3.xyz = u_xlat16_2.xxx * u_xlat6.xyz;
    u_xlat16_8 = dot(u_xlat3.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_2.xzw = u_xlat6.xyz * u_xlat16_2.xxx + (-((float3)(u_xlat16_8)));
    u_xlat16_2.xyz = ((float3)(float3(_DesaturationAdditiveLevel, _DesaturationAdditiveLevel, _DesaturationAdditiveLevel))) * u_xlat16_2.xzw + ((float3)(u_xlat16_8));
    u_xlat16_20 = u_xlat16_1.w * _Color.w;
    u_xlat16_2.xyz = ((float3)(u_xlat16_20)) * u_xlat16_2.xyz;
    u_xlat16_2.xyz = clamp(u_xlat16_2.xyz, 0.0, 1.0);
    u_xlat6.xyz = (-vs_TEXCOORD2.xyz) + _WorldSpaceLightPos0X.xyz;
    u_xlat3.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat3.x = (1.0/sqrt(u_xlat3.x));
    u_xlat6.xyz = u_xlat6.xyz * u_xlat3.xxx;
    u_xlat16_20 = dot(vs_TEXCOORD3.xyz, vs_TEXCOORD3.xyz);
    u_xlat16_20 = (1.0/sqrt(u_xlat16_20));
    u_xlat16_4.xyz = ((float3)(u_xlat16_20)) * vs_TEXCOORD3.xyz;
    u_xlat6.x = dot(u_xlat16_4.xyz, u_xlat6.xyz);
    u_xlat6.x = clamp(u_xlat6.x, 0.0, 1.0);
    u_xlat6.x = u_xlat6.x * _FlashlightMixWeights.y;
    u_xlat16_20 = _FlashlightFloor + 1.0;
    u_xlat16_20 = u_xlat16_20 * _RevealNormalize;
    u_xlat6.x = u_xlat16_20 * _FlashlightMixWeights.x + u_xlat6.x;
    u_xlat6.xyz = u_xlat6.xxx * _LightColor0X.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat6.xyz;
    u_xlat16_20 = _FlashlightBoostInterferenceMul * _FlashlightIntensity;
    u_xlat0.xyz = u_xlat0.xyz * ((float3)(u_xlat16_20)) + _AmbientColor.xyz;
    u_xlat16_4.xyz = u_xlat16_1.xyz * ((float3)(_Gain));
    SV_Target0.w = u_xlat16_1.w;
    u_xlat16_5.xyz = u_xlat0.xyz * u_xlat16_4.xyz;
    u_xlat16_20 = dot(u_xlat16_5.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_4.xyz = u_xlat16_4.xyz * u_xlat0.xyz + (-((float3)(u_xlat16_20)));
    u_xlat16_4.xyz = ((float3)(_DesaturationLevel)) * u_xlat16_4.xyz + ((float3)(u_xlat16_20));
    u_xlat0.x = _ShaderBrightness + 1.0;
    u_xlat6.xyz = u_xlat0.xxx * u_xlat16_4.xyz;
    u_xlat16_4.xyz = (-u_xlat16_4.xyz) * u_xlat0.xxx + float3(1.0, 1.0, 1.0);
    u_xlat16_2.xyz = u_xlat16_2.xyz * u_xlat16_4.xyz + u_xlat6.xyz;
    u_xlat16_4.xyz = (-u_xlat16_2.xyz) + _FalloffColor.xyz;
    u_xlat16_20 = vs_TEXCOORD1;
    u_xlat16_20 = clamp(u_xlat16_20, 0.0, 1.0);
    u_xlat16_22 = u_xlat16_20 * u_xlat16_20;
    u_xlat16_5.x = (-u_xlat16_20) * 2.0 + 3.0;
    u_xlat16_22 = u_xlat16_22 * u_xlat16_5.x + (-u_xlat16_20);
    u_xlat16_20 = _FalloffSmooth * u_xlat16_22 + u_xlat16_20;
    SV_Target0.xyz = ((float3)(u_xlat16_20)) * u_xlat16_4.xyz + u_xlat16_2.xyz;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(LIGHTMAP_ON) && !defined(LIGHTPROBE_SH) && !defined(SHADOWS_SCREEN) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD0 : TEXCOORD0;
	float4 in_TEXCOORD1 : TEXCOORD1;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float vs_TEXCOORD1 : TEXCOORD1;
	float3 vs_TEXCOORD2 : TEXCOORD2;
	float3 vs_TEXCOORD3 : TEXCOORD3;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 in_TEXCOORD1 = genIn.in_TEXCOORD1;
	float4 vs_TEXCOORD0 = (float4)0;
	float vs_TEXCOORD1 = (float)0;
	float3 vs_TEXCOORD2 = (float3)0;
	float3 vs_TEXCOORD3 = (float3)0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat9;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    gl_Position = u_xlat1;
    u_xlat1.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    u_xlat1.zw = in_TEXCOORD1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    vs_TEXCOORD0 = u_xlat1;
    u_xlat1.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    vs_TEXCOORD2.xyz = u_xlat0.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = (1.0/sqrt(u_xlat9));
    u_xlat0.xyz = ((float3)(u_xlat9)) * u_xlat0.xyz;
    vs_TEXCOORD3.xyz = u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	genOut.vs_TEXCOORD3 = vs_TEXCOORD3;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float3 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float3 vs_TEXCOORD3 = genIn.vs_TEXCOORD3;
	float4 u_xlat0;
	float2 u_xlat1;
	float4 u_xlat16_1;
	bool u_xlatb1;
	float4 u_xlat16_2;
	float3 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat16_5;
	float3 u_xlat6;
	float u_xlat16_8;
	float u_xlat16_18;
	float u_xlat16_20;
	float u_xlat16_22;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD2.yyyy * MCOL(unity_WorldToLightX,1);
    u_xlat0 = MCOL(unity_WorldToLightX,0) * vs_TEXCOORD2.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_WorldToLightX,2) * vs_TEXCOORD2.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_WorldToLightX,3);
    u_xlat1.xy = u_xlat0.xy / u_xlat0.ww;
    u_xlat1.xy = u_xlat1.xy + float2(0.5, 0.5);
    u_xlat16_18 = tex2D(_LightTexture0X, u_xlat1.xy).w;
    u_xlatb1 = u_xlat0.z>=0.0;
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat16_2.x = (-u_xlat0.x) + 1.0;
    u_xlat16_2.x = max(u_xlat16_2.x, 0.0);
    u_xlat0.x = u_xlatb1 ? 1.0 : float(0.0);
    u_xlat0.x = u_xlat0.x * u_xlat16_18;
    u_xlat16_8 = (-u_xlat0.x) * u_xlat16_2.x + 1.0;
    u_xlat0.x = u_xlat16_2.x * u_xlat0.x;
    u_xlat16_2.x = _CollectibleBlinkUngated * u_xlat16_8 + u_xlat0.x;
    u_xlat16_2.x = u_xlat16_2.x * _CollectibleBlink;
    u_xlat16_1 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat6.xyz = u_xlat16_1.xyz * ((float3)(_Gain)) + float3(0.150000006, 0.150000006, 0.150000006);
    u_xlat3.xyz = u_xlat16_2.xxx * u_xlat6.xyz;
    u_xlat16_8 = dot(u_xlat3.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_2.xzw = u_xlat6.xyz * u_xlat16_2.xxx + (-((float3)(u_xlat16_8)));
    u_xlat16_2.xyz = ((float3)(float3(_DesaturationAdditiveLevel, _DesaturationAdditiveLevel, _DesaturationAdditiveLevel))) * u_xlat16_2.xzw + ((float3)(u_xlat16_8));
    u_xlat16_20 = u_xlat16_1.w * _Color.w;
    u_xlat16_2.xyz = ((float3)(u_xlat16_20)) * u_xlat16_2.xyz;
    u_xlat16_2.xyz = clamp(u_xlat16_2.xyz, 0.0, 1.0);
    u_xlat6.xyz = (-vs_TEXCOORD2.xyz) + _WorldSpaceLightPos0X.xyz;
    u_xlat3.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat3.x = (1.0/sqrt(u_xlat3.x));
    u_xlat6.xyz = u_xlat6.xyz * u_xlat3.xxx;
    u_xlat16_20 = dot(vs_TEXCOORD3.xyz, vs_TEXCOORD3.xyz);
    u_xlat16_20 = (1.0/sqrt(u_xlat16_20));
    u_xlat16_4.xyz = ((float3)(u_xlat16_20)) * vs_TEXCOORD3.xyz;
    u_xlat6.x = dot(u_xlat16_4.xyz, u_xlat6.xyz);
    u_xlat6.x = clamp(u_xlat6.x, 0.0, 1.0);
    u_xlat6.x = u_xlat6.x * _FlashlightMixWeights.y;
    u_xlat16_4.xyz = UNITY_SAMPLE_TEX2D(unity_Lightmap, vs_TEXCOORD0.zw).xyz;
    u_xlat16_5.xyz = unity_Lightmap_HDR.xxx * u_xlat16_4.xyz + ((float3)(float3(_FlashlightFloor, _FlashlightFloor, _FlashlightFloor)));
    u_xlat16_4.xyz = u_xlat16_4.xyz * unity_Lightmap_HDR.xxx;
    u_xlat16_5.xyz = u_xlat16_5.xyz * ((float3)(float3(_RevealNormalize, _RevealNormalize, _RevealNormalize)));
    u_xlat6.xyz = u_xlat16_5.xyz * _FlashlightMixWeights.xxx + u_xlat6.xxx;
    u_xlat6.xyz = u_xlat6.xyz * _LightColor0X.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat6.xyz;
    u_xlat16_20 = _FlashlightBoostInterferenceMul * _FlashlightIntensity;
    u_xlat0.xyz = u_xlat0.xyz * ((float3)(u_xlat16_20));
    u_xlat0.xyz = u_xlat16_4.xyz * ((float3)(float3(_LMScale, _LMScale, _LMScale))) + u_xlat0.xyz;
    u_xlat16_4.xyz = u_xlat16_1.xyz * ((float3)(_Gain));
    SV_Target0.w = u_xlat16_1.w;
    u_xlat16_5.xyz = u_xlat0.xyz * u_xlat16_4.xyz;
    u_xlat16_20 = dot(u_xlat16_5.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_4.xyz = u_xlat16_4.xyz * u_xlat0.xyz + (-((float3)(u_xlat16_20)));
    u_xlat16_4.xyz = ((float3)(_DesaturationLevel)) * u_xlat16_4.xyz + ((float3)(u_xlat16_20));
    u_xlat0.x = _ShaderBrightness + 1.0;
    u_xlat6.xyz = u_xlat0.xxx * u_xlat16_4.xyz;
    u_xlat16_4.xyz = (-u_xlat16_4.xyz) * u_xlat0.xxx + float3(1.0, 1.0, 1.0);
    u_xlat16_2.xyz = u_xlat16_2.xyz * u_xlat16_4.xyz + u_xlat6.xyz;
    u_xlat16_4.xyz = (-u_xlat16_2.xyz) + _FalloffColor.xyz;
    u_xlat16_20 = vs_TEXCOORD1;
    u_xlat16_20 = clamp(u_xlat16_20, 0.0, 1.0);
    u_xlat16_22 = u_xlat16_20 * u_xlat16_20;
    u_xlat16_5.x = (-u_xlat16_20) * 2.0 + 3.0;
    u_xlat16_22 = u_xlat16_22 * u_xlat16_5.x + (-u_xlat16_20);
    u_xlat16_20 = _FalloffSmooth * u_xlat16_22 + u_xlat16_20;
    SV_Target0.xyz = ((float3)(u_xlat16_20)) * u_xlat16_4.xyz + u_xlat16_2.xyz;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(SHADOWS_SCREEN) && !defined(LIGHTMAP_ON) && !defined(LIGHTPROBE_SH) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float vs_TEXCOORD1 : TEXCOORD1;
	float3 vs_TEXCOORD2 : TEXCOORD2;
	float3 vs_TEXCOORD3 : TEXCOORD3;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0 = (float4)0;
	float vs_TEXCOORD1 = (float)0;
	float3 vs_TEXCOORD2 = (float3)0;
	float3 vs_TEXCOORD3 = (float3)0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat9;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    gl_Position = u_xlat1;
    u_xlat1.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat1.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    u_xlat1.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    vs_TEXCOORD2.xyz = u_xlat0.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = (1.0/sqrt(u_xlat9));
    u_xlat0.xyz = ((float3)(u_xlat9)) * u_xlat0.xyz;
    vs_TEXCOORD3.xyz = u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	genOut.vs_TEXCOORD3 = vs_TEXCOORD3;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float3 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float3 vs_TEXCOORD3 = genIn.vs_TEXCOORD3;
	float4 u_xlat0;
	float2 u_xlat1;
	float4 u_xlat16_1;
	bool u_xlatb1;
	float4 u_xlat16_2;
	float3 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat16_5;
	float3 u_xlat6;
	float u_xlat16_8;
	float u_xlat16_18;
	float u_xlat16_20;
	float u_xlat16_22;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD2.yyyy * MCOL(unity_WorldToLightX,1);
    u_xlat0 = MCOL(unity_WorldToLightX,0) * vs_TEXCOORD2.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_WorldToLightX,2) * vs_TEXCOORD2.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_WorldToLightX,3);
    u_xlat1.xy = u_xlat0.xy / u_xlat0.ww;
    u_xlat1.xy = u_xlat1.xy + float2(0.5, 0.5);
    u_xlat16_18 = tex2D(_LightTexture0X, u_xlat1.xy).w;
    u_xlatb1 = u_xlat0.z>=0.0;
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat16_2.x = (-u_xlat0.x) + 1.0;
    u_xlat16_2.x = max(u_xlat16_2.x, 0.0);
    u_xlat0.x = u_xlatb1 ? 1.0 : float(0.0);
    u_xlat0.x = u_xlat0.x * u_xlat16_18;
    u_xlat16_8 = (-u_xlat0.x) * u_xlat16_2.x + 1.0;
    u_xlat0.x = u_xlat16_2.x * u_xlat0.x;
    u_xlat16_2.x = _CollectibleBlinkUngated * u_xlat16_8 + u_xlat0.x;
    u_xlat16_2.x = u_xlat16_2.x * _CollectibleBlink;
    u_xlat16_1 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat6.xyz = u_xlat16_1.xyz * ((float3)(_Gain)) + float3(0.150000006, 0.150000006, 0.150000006);
    u_xlat3.xyz = u_xlat16_2.xxx * u_xlat6.xyz;
    u_xlat16_8 = dot(u_xlat3.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_2.xzw = u_xlat6.xyz * u_xlat16_2.xxx + (-((float3)(u_xlat16_8)));
    u_xlat16_2.xyz = ((float3)(float3(_DesaturationAdditiveLevel, _DesaturationAdditiveLevel, _DesaturationAdditiveLevel))) * u_xlat16_2.xzw + ((float3)(u_xlat16_8));
    u_xlat16_20 = u_xlat16_1.w * _Color.w;
    u_xlat16_2.xyz = ((float3)(u_xlat16_20)) * u_xlat16_2.xyz;
    u_xlat16_2.xyz = clamp(u_xlat16_2.xyz, 0.0, 1.0);
    u_xlat6.xyz = (-vs_TEXCOORD2.xyz) + _WorldSpaceLightPos0X.xyz;
    u_xlat3.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat3.x = (1.0/sqrt(u_xlat3.x));
    u_xlat6.xyz = u_xlat6.xyz * u_xlat3.xxx;
    u_xlat16_20 = dot(vs_TEXCOORD3.xyz, vs_TEXCOORD3.xyz);
    u_xlat16_20 = (1.0/sqrt(u_xlat16_20));
    u_xlat16_4.xyz = ((float3)(u_xlat16_20)) * vs_TEXCOORD3.xyz;
    u_xlat6.x = dot(u_xlat16_4.xyz, u_xlat6.xyz);
    u_xlat6.x = clamp(u_xlat6.x, 0.0, 1.0);
    u_xlat6.x = u_xlat6.x * _FlashlightMixWeights.y;
    u_xlat16_20 = _FlashlightFloor + 1.0;
    u_xlat16_20 = u_xlat16_20 * _RevealNormalize;
    u_xlat6.x = u_xlat16_20 * _FlashlightMixWeights.x + u_xlat6.x;
    u_xlat6.xyz = u_xlat6.xxx * _LightColor0X.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat6.xyz;
    u_xlat16_20 = _FlashlightBoostInterferenceMul * _FlashlightIntensity;
    u_xlat0.xyz = u_xlat0.xyz * ((float3)(u_xlat16_20)) + _AmbientColor.xyz;
    u_xlat16_4.xyz = u_xlat16_1.xyz * ((float3)(_Gain));
    SV_Target0.w = u_xlat16_1.w;
    u_xlat16_5.xyz = u_xlat0.xyz * u_xlat16_4.xyz;
    u_xlat16_20 = dot(u_xlat16_5.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_4.xyz = u_xlat16_4.xyz * u_xlat0.xyz + (-((float3)(u_xlat16_20)));
    u_xlat16_4.xyz = ((float3)(_DesaturationLevel)) * u_xlat16_4.xyz + ((float3)(u_xlat16_20));
    u_xlat0.x = _ShaderBrightness + 1.0;
    u_xlat6.xyz = u_xlat0.xxx * u_xlat16_4.xyz;
    u_xlat16_4.xyz = (-u_xlat16_4.xyz) * u_xlat0.xxx + float3(1.0, 1.0, 1.0);
    u_xlat16_2.xyz = u_xlat16_2.xyz * u_xlat16_4.xyz + u_xlat6.xyz;
    u_xlat16_4.xyz = (-u_xlat16_2.xyz) + _FalloffColor.xyz;
    u_xlat16_20 = vs_TEXCOORD1;
    u_xlat16_20 = clamp(u_xlat16_20, 0.0, 1.0);
    u_xlat16_22 = u_xlat16_20 * u_xlat16_20;
    u_xlat16_5.x = (-u_xlat16_20) * 2.0 + 3.0;
    u_xlat16_22 = u_xlat16_22 * u_xlat16_5.x + (-u_xlat16_20);
    u_xlat16_20 = _FalloffSmooth * u_xlat16_22 + u_xlat16_20;
    SV_Target0.xyz = ((float3)(u_xlat16_20)) * u_xlat16_4.xyz + u_xlat16_2.xyz;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && defined(VERTEXLIGHT_ON) && !defined(LIGHTMAP_ON) && !defined(LIGHTPROBE_SH) && !defined(SHADOWS_SCREEN)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float vs_TEXCOORD1 : TEXCOORD1;
	float3 vs_TEXCOORD2 : TEXCOORD2;
	float3 vs_TEXCOORD3 : TEXCOORD3;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0 = (float4)0;
	float vs_TEXCOORD1 = (float)0;
	float3 vs_TEXCOORD2 = (float3)0;
	float3 vs_TEXCOORD3 = (float3)0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat9;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    gl_Position = u_xlat1;
    u_xlat1.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat1.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    u_xlat1.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    vs_TEXCOORD2.xyz = u_xlat0.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = (1.0/sqrt(u_xlat9));
    u_xlat0.xyz = ((float3)(u_xlat9)) * u_xlat0.xyz;
    vs_TEXCOORD3.xyz = u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	genOut.vs_TEXCOORD3 = vs_TEXCOORD3;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float3 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float3 vs_TEXCOORD3 = genIn.vs_TEXCOORD3;
	float4 u_xlat0;
	float2 u_xlat1;
	float4 u_xlat16_1;
	bool u_xlatb1;
	float4 u_xlat16_2;
	float3 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat16_5;
	float3 u_xlat6;
	float u_xlat16_8;
	float u_xlat16_18;
	float u_xlat16_20;
	float u_xlat16_22;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD2.yyyy * MCOL(unity_WorldToLightX,1);
    u_xlat0 = MCOL(unity_WorldToLightX,0) * vs_TEXCOORD2.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_WorldToLightX,2) * vs_TEXCOORD2.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_WorldToLightX,3);
    u_xlat1.xy = u_xlat0.xy / u_xlat0.ww;
    u_xlat1.xy = u_xlat1.xy + float2(0.5, 0.5);
    u_xlat16_18 = tex2D(_LightTexture0X, u_xlat1.xy).w;
    u_xlatb1 = u_xlat0.z>=0.0;
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat16_2.x = (-u_xlat0.x) + 1.0;
    u_xlat16_2.x = max(u_xlat16_2.x, 0.0);
    u_xlat0.x = u_xlatb1 ? 1.0 : float(0.0);
    u_xlat0.x = u_xlat0.x * u_xlat16_18;
    u_xlat16_8 = (-u_xlat0.x) * u_xlat16_2.x + 1.0;
    u_xlat0.x = u_xlat16_2.x * u_xlat0.x;
    u_xlat16_2.x = _CollectibleBlinkUngated * u_xlat16_8 + u_xlat0.x;
    u_xlat16_2.x = u_xlat16_2.x * _CollectibleBlink;
    u_xlat16_1 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat6.xyz = u_xlat16_1.xyz * ((float3)(_Gain)) + float3(0.150000006, 0.150000006, 0.150000006);
    u_xlat3.xyz = u_xlat16_2.xxx * u_xlat6.xyz;
    u_xlat16_8 = dot(u_xlat3.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_2.xzw = u_xlat6.xyz * u_xlat16_2.xxx + (-((float3)(u_xlat16_8)));
    u_xlat16_2.xyz = ((float3)(float3(_DesaturationAdditiveLevel, _DesaturationAdditiveLevel, _DesaturationAdditiveLevel))) * u_xlat16_2.xzw + ((float3)(u_xlat16_8));
    u_xlat16_20 = u_xlat16_1.w * _Color.w;
    u_xlat16_2.xyz = ((float3)(u_xlat16_20)) * u_xlat16_2.xyz;
    u_xlat16_2.xyz = clamp(u_xlat16_2.xyz, 0.0, 1.0);
    u_xlat6.xyz = (-vs_TEXCOORD2.xyz) + _WorldSpaceLightPos0X.xyz;
    u_xlat3.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat3.x = (1.0/sqrt(u_xlat3.x));
    u_xlat6.xyz = u_xlat6.xyz * u_xlat3.xxx;
    u_xlat16_20 = dot(vs_TEXCOORD3.xyz, vs_TEXCOORD3.xyz);
    u_xlat16_20 = (1.0/sqrt(u_xlat16_20));
    u_xlat16_4.xyz = ((float3)(u_xlat16_20)) * vs_TEXCOORD3.xyz;
    u_xlat6.x = dot(u_xlat16_4.xyz, u_xlat6.xyz);
    u_xlat6.x = clamp(u_xlat6.x, 0.0, 1.0);
    u_xlat6.x = u_xlat6.x * _FlashlightMixWeights.y;
    u_xlat16_20 = _FlashlightFloor + 1.0;
    u_xlat16_20 = u_xlat16_20 * _RevealNormalize;
    u_xlat6.x = u_xlat16_20 * _FlashlightMixWeights.x + u_xlat6.x;
    u_xlat6.xyz = u_xlat6.xxx * _LightColor0X.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat6.xyz;
    u_xlat16_20 = _FlashlightBoostInterferenceMul * _FlashlightIntensity;
    u_xlat0.xyz = u_xlat0.xyz * ((float3)(u_xlat16_20)) + _AmbientColor.xyz;
    u_xlat16_4.xyz = u_xlat16_1.xyz * ((float3)(_Gain));
    SV_Target0.w = u_xlat16_1.w;
    u_xlat16_5.xyz = u_xlat0.xyz * u_xlat16_4.xyz;
    u_xlat16_20 = dot(u_xlat16_5.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_4.xyz = u_xlat16_4.xyz * u_xlat0.xyz + (-((float3)(u_xlat16_20)));
    u_xlat16_4.xyz = ((float3)(_DesaturationLevel)) * u_xlat16_4.xyz + ((float3)(u_xlat16_20));
    u_xlat0.x = _ShaderBrightness + 1.0;
    u_xlat6.xyz = u_xlat0.xxx * u_xlat16_4.xyz;
    u_xlat16_4.xyz = (-u_xlat16_4.xyz) * u_xlat0.xxx + float3(1.0, 1.0, 1.0);
    u_xlat16_2.xyz = u_xlat16_2.xyz * u_xlat16_4.xyz + u_xlat6.xyz;
    u_xlat16_4.xyz = (-u_xlat16_2.xyz) + _FalloffColor.xyz;
    u_xlat16_20 = vs_TEXCOORD1;
    u_xlat16_20 = clamp(u_xlat16_20, 0.0, 1.0);
    u_xlat16_22 = u_xlat16_20 * u_xlat16_20;
    u_xlat16_5.x = (-u_xlat16_20) * 2.0 + 3.0;
    u_xlat16_22 = u_xlat16_22 * u_xlat16_5.x + (-u_xlat16_20);
    u_xlat16_20 = _FalloffSmooth * u_xlat16_22 + u_xlat16_20;
    SV_Target0.xyz = ((float3)(u_xlat16_20)) * u_xlat16_4.xyz + u_xlat16_2.xyz;
    return SV_Target0;

	return SV_Target0;
}
#elif defined(DIRECTIONAL) && !defined(LIGHTMAP_ON) && !defined(LIGHTPROBE_SH) && !defined(SHADOWS_SCREEN) && !defined(VERTEXLIGHT_ON)
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float vs_TEXCOORD1 : TEXCOORD1;
	float3 vs_TEXCOORD2 : TEXCOORD2;
	float3 vs_TEXCOORD3 : TEXCOORD3;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0 = (float4)0;
	float vs_TEXCOORD1 = (float)0;
	float3 vs_TEXCOORD2 = (float3)0;
	float3 vs_TEXCOORD3 = (float3)0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat9;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    gl_Position = u_xlat1;
    u_xlat1.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat1.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    u_xlat1.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    vs_TEXCOORD2.xyz = u_xlat0.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = (1.0/sqrt(u_xlat9));
    u_xlat0.xyz = ((float3)(u_xlat9)) * u_xlat0.xyz;
    vs_TEXCOORD3.xyz = u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	genOut.vs_TEXCOORD3 = vs_TEXCOORD3;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float3 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float3 vs_TEXCOORD3 = genIn.vs_TEXCOORD3;
	float4 u_xlat0;
	float2 u_xlat1;
	float4 u_xlat16_1;
	bool u_xlatb1;
	float4 u_xlat16_2;
	float3 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat16_5;
	float3 u_xlat6;
	float u_xlat16_8;
	float u_xlat16_18;
	float u_xlat16_20;
	float u_xlat16_22;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD2.yyyy * MCOL(unity_WorldToLightX,1);
    u_xlat0 = MCOL(unity_WorldToLightX,0) * vs_TEXCOORD2.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_WorldToLightX,2) * vs_TEXCOORD2.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_WorldToLightX,3);
    u_xlat1.xy = u_xlat0.xy / u_xlat0.ww;
    u_xlat1.xy = u_xlat1.xy + float2(0.5, 0.5);
    u_xlat16_18 = tex2D(_LightTexture0X, u_xlat1.xy).w;
    u_xlatb1 = u_xlat0.z>=0.0;
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat16_2.x = (-u_xlat0.x) + 1.0;
    u_xlat16_2.x = max(u_xlat16_2.x, 0.0);
    u_xlat0.x = u_xlatb1 ? 1.0 : float(0.0);
    u_xlat0.x = u_xlat0.x * u_xlat16_18;
    u_xlat16_8 = (-u_xlat0.x) * u_xlat16_2.x + 1.0;
    u_xlat0.x = u_xlat16_2.x * u_xlat0.x;
    u_xlat16_2.x = _CollectibleBlinkUngated * u_xlat16_8 + u_xlat0.x;
    u_xlat16_2.x = u_xlat16_2.x * _CollectibleBlink;
    u_xlat16_1 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat6.xyz = u_xlat16_1.xyz * ((float3)(_Gain)) + float3(0.150000006, 0.150000006, 0.150000006);
    u_xlat3.xyz = u_xlat16_2.xxx * u_xlat6.xyz;
    u_xlat16_8 = dot(u_xlat3.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_2.xzw = u_xlat6.xyz * u_xlat16_2.xxx + (-((float3)(u_xlat16_8)));
    u_xlat16_2.xyz = ((float3)(float3(_DesaturationAdditiveLevel, _DesaturationAdditiveLevel, _DesaturationAdditiveLevel))) * u_xlat16_2.xzw + ((float3)(u_xlat16_8));
    u_xlat16_20 = u_xlat16_1.w * _Color.w;
    u_xlat16_2.xyz = ((float3)(u_xlat16_20)) * u_xlat16_2.xyz;
    u_xlat16_2.xyz = clamp(u_xlat16_2.xyz, 0.0, 1.0);
    u_xlat6.xyz = (-vs_TEXCOORD2.xyz) + _WorldSpaceLightPos0X.xyz;
    u_xlat3.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat3.x = (1.0/sqrt(u_xlat3.x));
    u_xlat6.xyz = u_xlat6.xyz * u_xlat3.xxx;
    u_xlat16_20 = dot(vs_TEXCOORD3.xyz, vs_TEXCOORD3.xyz);
    u_xlat16_20 = (1.0/sqrt(u_xlat16_20));
    u_xlat16_4.xyz = ((float3)(u_xlat16_20)) * vs_TEXCOORD3.xyz;
    u_xlat6.x = dot(u_xlat16_4.xyz, u_xlat6.xyz);
    u_xlat6.x = clamp(u_xlat6.x, 0.0, 1.0);
    u_xlat6.x = u_xlat6.x * _FlashlightMixWeights.y;
    u_xlat16_20 = _FlashlightFloor + 1.0;
    u_xlat16_20 = u_xlat16_20 * _RevealNormalize;
    u_xlat6.x = u_xlat16_20 * _FlashlightMixWeights.x + u_xlat6.x;
    u_xlat6.xyz = u_xlat6.xxx * _LightColor0X.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat6.xyz;
    u_xlat16_20 = _FlashlightBoostInterferenceMul * _FlashlightIntensity;
    u_xlat0.xyz = u_xlat0.xyz * ((float3)(u_xlat16_20)) + _AmbientColor.xyz;
    u_xlat16_4.xyz = u_xlat16_1.xyz * ((float3)(_Gain));
    SV_Target0.w = u_xlat16_1.w;
    u_xlat16_5.xyz = u_xlat0.xyz * u_xlat16_4.xyz;
    u_xlat16_20 = dot(u_xlat16_5.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_4.xyz = u_xlat16_4.xyz * u_xlat0.xyz + (-((float3)(u_xlat16_20)));
    u_xlat16_4.xyz = ((float3)(_DesaturationLevel)) * u_xlat16_4.xyz + ((float3)(u_xlat16_20));
    u_xlat0.x = _ShaderBrightness + 1.0;
    u_xlat6.xyz = u_xlat0.xxx * u_xlat16_4.xyz;
    u_xlat16_4.xyz = (-u_xlat16_4.xyz) * u_xlat0.xxx + float3(1.0, 1.0, 1.0);
    u_xlat16_2.xyz = u_xlat16_2.xyz * u_xlat16_4.xyz + u_xlat6.xyz;
    u_xlat16_4.xyz = (-u_xlat16_2.xyz) + _FalloffColor.xyz;
    u_xlat16_20 = vs_TEXCOORD1;
    u_xlat16_20 = clamp(u_xlat16_20, 0.0, 1.0);
    u_xlat16_22 = u_xlat16_20 * u_xlat16_20;
    u_xlat16_5.x = (-u_xlat16_20) * 2.0 + 3.0;
    u_xlat16_22 = u_xlat16_22 * u_xlat16_5.x + (-u_xlat16_20);
    u_xlat16_20 = _FalloffSmooth * u_xlat16_22 + u_xlat16_20;
    SV_Target0.xyz = ((float3)(u_xlat16_20)) * u_xlat16_4.xyz + u_xlat16_2.xyz;
    return SV_Target0;

	return SV_Target0;
}
#else
struct appdata_gen {
	float4 in_POSITION0 : POSITION0;
	float3 in_NORMAL0 : NORMAL0;
	float4 in_TEXCOORD0 : TEXCOORD0;
};
struct v2f_gen {
	float4 gen_pos : SV_POSITION;
	float4 vs_TEXCOORD0 : TEXCOORD0;
	float vs_TEXCOORD1 : TEXCOORD1;
	float3 vs_TEXCOORD2 : TEXCOORD2;
	float3 vs_TEXCOORD3 : TEXCOORD3;
};
v2f_gen vert(appdata_gen genIn) {
	v2f_gen genOut = (v2f_gen)0;
	float4 in_POSITION0 = genIn.in_POSITION0;
	float3 in_NORMAL0 = genIn.in_NORMAL0;
	float4 in_TEXCOORD0 = genIn.in_TEXCOORD0;
	float4 vs_TEXCOORD0 = (float4)0;
	float vs_TEXCOORD1 = (float)0;
	float3 vs_TEXCOORD2 = (float3)0;
	float3 vs_TEXCOORD3 = (float3)0;
	float4 u_xlat0;
	float4 u_xlat1;
	float4 u_xlat2;
	float u_xlat9;
	float4 gl_Position = 0;

    u_xlat0 = in_POSITION0.yyyy * MCOL(unity_ObjectToWorld,1);
    u_xlat0 = MCOL(unity_ObjectToWorld,0) * in_POSITION0.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_ObjectToWorld,2) * in_POSITION0.zzzz + u_xlat0;
    u_xlat1 = u_xlat0 + MCOL(unity_ObjectToWorld,3);
    u_xlat0.xyz = MCOL(unity_ObjectToWorld,3).xyz * in_POSITION0.www + u_xlat0.xyz;
    u_xlat2 = u_xlat1.yyyy * MCOL(unity_MatrixVP,1);
    u_xlat2 = MCOL(unity_MatrixVP,0) * u_xlat1.xxxx + u_xlat2;
    u_xlat2 = MCOL(unity_MatrixVP,2) * u_xlat1.zzzz + u_xlat2;
    u_xlat1 = MCOL(unity_MatrixVP,3) * u_xlat1.wwww + u_xlat2;
    gl_Position = u_xlat1;
    u_xlat1.xy = in_TEXCOORD0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    vs_TEXCOORD0.xy = u_xlat1.xy;
    vs_TEXCOORD0.zw = float2(0.0, 0.0);
    u_xlat1.xyz = u_xlat0.xyz + (-_WorldSpaceCameraPos.xyz);
    vs_TEXCOORD2.xyz = u_xlat0.xyz;
    u_xlat0.x = dot(u_xlat1.xyz, u_xlat1.xyz);
    u_xlat0.x = sqrt(u_xlat0.x);
    u_xlat0.x = u_xlat0.x * _FalloffScale + _FalloffOffset;
    vs_TEXCOORD1 = u_xlat0.x;
    u_xlat0.x = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,0).xyz);
    u_xlat0.y = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,1).xyz);
    u_xlat0.z = dot(in_NORMAL0.xyz, MCOL(unity_WorldToObject,2).xyz);
    u_xlat9 = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat9 = (1.0/sqrt(u_xlat9));
    u_xlat0.xyz = ((float3)(u_xlat9)) * u_xlat0.xyz;
    vs_TEXCOORD3.xyz = u_xlat0.xyz;
    

	genOut.gen_pos = gl_Position;
	genOut.vs_TEXCOORD0 = vs_TEXCOORD0;
	genOut.vs_TEXCOORD1 = vs_TEXCOORD1;
	genOut.vs_TEXCOORD2 = vs_TEXCOORD2;
	genOut.vs_TEXCOORD3 = vs_TEXCOORD3;
	return genOut;
}
float4 frag(v2f_gen genIn) : SV_Target {
	float4 vs_TEXCOORD0 = genIn.vs_TEXCOORD0;
	float vs_TEXCOORD1 = genIn.vs_TEXCOORD1;
	float3 vs_TEXCOORD2 = genIn.vs_TEXCOORD2;
	float3 vs_TEXCOORD3 = genIn.vs_TEXCOORD3;
	float4 u_xlat0;
	float2 u_xlat1;
	float4 u_xlat16_1;
	bool u_xlatb1;
	float4 u_xlat16_2;
	float3 u_xlat3;
	float3 u_xlat16_4;
	float3 u_xlat16_5;
	float3 u_xlat6;
	float u_xlat16_8;
	float u_xlat16_18;
	float u_xlat16_20;
	float u_xlat16_22;
	float4 SV_Target0 = 0;

    u_xlat0 = vs_TEXCOORD2.yyyy * MCOL(unity_WorldToLightX,1);
    u_xlat0 = MCOL(unity_WorldToLightX,0) * vs_TEXCOORD2.xxxx + u_xlat0;
    u_xlat0 = MCOL(unity_WorldToLightX,2) * vs_TEXCOORD2.zzzz + u_xlat0;
    u_xlat0 = u_xlat0 + MCOL(unity_WorldToLightX,3);
    u_xlat1.xy = u_xlat0.xy / u_xlat0.ww;
    u_xlat1.xy = u_xlat1.xy + float2(0.5, 0.5);
    u_xlat16_18 = tex2D(_LightTexture0X, u_xlat1.xy).w;
    u_xlatb1 = u_xlat0.z>=0.0;
    u_xlat0.x = dot(u_xlat0.xyz, u_xlat0.xyz);
    u_xlat16_2.x = (-u_xlat0.x) + 1.0;
    u_xlat16_2.x = max(u_xlat16_2.x, 0.0);
    u_xlat0.x = u_xlatb1 ? 1.0 : float(0.0);
    u_xlat0.x = u_xlat0.x * u_xlat16_18;
    u_xlat16_8 = (-u_xlat0.x) * u_xlat16_2.x + 1.0;
    u_xlat0.x = u_xlat16_2.x * u_xlat0.x;
    u_xlat16_2.x = _CollectibleBlinkUngated * u_xlat16_8 + u_xlat0.x;
    u_xlat16_2.x = u_xlat16_2.x * _CollectibleBlink;
    u_xlat16_1 = tex2D(_MainTex, vs_TEXCOORD0.xy);
    u_xlat6.xyz = u_xlat16_1.xyz * ((float3)(_Gain)) + float3(0.150000006, 0.150000006, 0.150000006);
    u_xlat3.xyz = u_xlat16_2.xxx * u_xlat6.xyz;
    u_xlat16_8 = dot(u_xlat3.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_2.xzw = u_xlat6.xyz * u_xlat16_2.xxx + (-((float3)(u_xlat16_8)));
    u_xlat16_2.xyz = ((float3)(float3(_DesaturationAdditiveLevel, _DesaturationAdditiveLevel, _DesaturationAdditiveLevel))) * u_xlat16_2.xzw + ((float3)(u_xlat16_8));
    u_xlat16_20 = u_xlat16_1.w * _Color.w;
    u_xlat16_2.xyz = ((float3)(u_xlat16_20)) * u_xlat16_2.xyz;
    u_xlat16_2.xyz = clamp(u_xlat16_2.xyz, 0.0, 1.0);
    u_xlat6.xyz = (-vs_TEXCOORD2.xyz) + _WorldSpaceLightPos0X.xyz;
    u_xlat3.x = dot(u_xlat6.xyz, u_xlat6.xyz);
    u_xlat3.x = (1.0/sqrt(u_xlat3.x));
    u_xlat6.xyz = u_xlat6.xyz * u_xlat3.xxx;
    u_xlat16_20 = dot(vs_TEXCOORD3.xyz, vs_TEXCOORD3.xyz);
    u_xlat16_20 = (1.0/sqrt(u_xlat16_20));
    u_xlat16_4.xyz = ((float3)(u_xlat16_20)) * vs_TEXCOORD3.xyz;
    u_xlat6.x = dot(u_xlat16_4.xyz, u_xlat6.xyz);
    u_xlat6.x = clamp(u_xlat6.x, 0.0, 1.0);
    u_xlat6.x = u_xlat6.x * _FlashlightMixWeights.y;
    u_xlat16_20 = _FlashlightFloor + 1.0;
    u_xlat16_20 = u_xlat16_20 * _RevealNormalize;
    u_xlat6.x = u_xlat16_20 * _FlashlightMixWeights.x + u_xlat6.x;
    u_xlat6.xyz = u_xlat6.xxx * _LightColor0X.xyz;
    u_xlat0.xyz = u_xlat0.xxx * u_xlat6.xyz;
    u_xlat16_20 = _FlashlightBoostInterferenceMul * _FlashlightIntensity;
    u_xlat0.xyz = u_xlat0.xyz * ((float3)(u_xlat16_20)) + _AmbientColor.xyz;
    u_xlat16_4.xyz = u_xlat16_1.xyz * ((float3)(_Gain));
    SV_Target0.w = u_xlat16_1.w;
    u_xlat16_5.xyz = u_xlat0.xyz * u_xlat16_4.xyz;
    u_xlat16_20 = dot(u_xlat16_5.xyz, float3(0.219999999, 0.707000017, 0.0710000023));
    u_xlat16_4.xyz = u_xlat16_4.xyz * u_xlat0.xyz + (-((float3)(u_xlat16_20)));
    u_xlat16_4.xyz = ((float3)(_DesaturationLevel)) * u_xlat16_4.xyz + ((float3)(u_xlat16_20));
    u_xlat0.x = _ShaderBrightness + 1.0;
    u_xlat6.xyz = u_xlat0.xxx * u_xlat16_4.xyz;
    u_xlat16_4.xyz = (-u_xlat16_4.xyz) * u_xlat0.xxx + float3(1.0, 1.0, 1.0);
    u_xlat16_2.xyz = u_xlat16_2.xyz * u_xlat16_4.xyz + u_xlat6.xyz;
    u_xlat16_4.xyz = (-u_xlat16_2.xyz) + _FalloffColor.xyz;
    u_xlat16_20 = vs_TEXCOORD1;
    u_xlat16_20 = clamp(u_xlat16_20, 0.0, 1.0);
    u_xlat16_22 = u_xlat16_20 * u_xlat16_20;
    u_xlat16_5.x = (-u_xlat16_20) * 2.0 + 3.0;
    u_xlat16_22 = u_xlat16_22 * u_xlat16_5.x + (-u_xlat16_20);
    u_xlat16_20 = _FalloffSmooth * u_xlat16_22 + u_xlat16_20;
    SV_Target0.xyz = ((float3)(u_xlat16_20)) * u_xlat16_4.xyz + u_xlat16_2.xyz;
    return SV_Target0;

	return SV_Target0;
}
#endif
ENDHLSL
		}
	}
}

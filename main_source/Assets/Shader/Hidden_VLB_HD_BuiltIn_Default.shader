Shader "Hidden/VLB_HD_BuiltIn_Default" {
	Properties {
		_ConeRadius ("Cone Radius", Vector) = (0.0,0.0,0.0,0.0)
		_ConeGeomProps ("Cone Geom Props", Vector) = (0.0,0.0,0.0,0.0)
		_ColorFlat ("Color", Color) = (1.0,1.0,1.0,1.0)
		_HDRPExposureWeight ("HDRP Exposure Weight", Range(0.0, 1.0)) = 0.0
		_DistanceFallOff ("Distance Fall Off", Vector) = (0.0,1.0,1.0,0.0)
		_NoiseVelocityAndScale ("Noise Velocity And Scale", Vector) = (0.0,0.0,0.0,0.0)
		_NoiseParam ("Noise Param", Vector) = (0.0,0.0,0.0,0.0)
		_BlendSrcFactor ("BlendSrcFactor", Float) = 1.0
		_BlendDstFactor ("BlendDstFactor", Float) = 1.0
		_ZTest ("ZTest", Float) = 4.0
		_ConeSlopeCosSin ("Cone Slope Cos Sin", Vector) = (0.0,0.0,0.0,0.0)
		_AlphaInside ("Alpha Inside", Range(0.0, 1.0)) = 1.0
		_AlphaOutside ("Alpha Outside", Range(0.0, 1.0)) = 1.0
		_DistanceCamClipping ("Camera Clipping Distance", Float) = 0.5
		_FadeOutFactor ("FadeOutFactor", Float) = 1.0
		_AttenuationLerpLinearQuad ("Lerp between attenuation linear and quad", Float) = 0.5
		_DepthBlendDistance ("Depth Blend Distance", Float) = 2.0
		_FresnelPow ("Fresnel Pow", Range(0.0, 15.0)) = 1.0
		_GlareFrontal ("Glare Frontal", Range(0.0, 1.0)) = 0.5
		_GlareBehind ("Glare from Behind", Range(0.0, 1.0)) = 0.5
		_DrawCap ("Draw Cap", Float) = 1.0
		_CameraParams ("Camera Params", Vector) = (0.0,0.0,0.0,0.0)
		_DynamicOcclusionClippingPlaneWS ("Dynamic Occlusion Clipping Plane WS", Vector) = (0.0,0.0,0.0,0.0)
		_DynamicOcclusionClippingPlaneProps ("Dynamic Occlusion Clipping Plane Props", Float) = 0.25
		_DynamicOcclusionDepthTexture ("DynamicOcclusionDepthTexture", 2D) = "white" {}
		_DynamicOcclusionDepthProps ("DynamicOcclusionDepthProps", Vector) = (1.0,1.0,0.25,1.0)
		_LocalForwardDirection ("LocalForwardDirection", Vector) = (0.0,0.0,1.0,1.0)
		_TiltVector ("TiltVector", Vector) = (0.0,0.0,0.0,0.0)
		_AdditionalClippingPlaneWS ("AdditionalClippingPlaneWS", Vector) = (0.0,0.0,0.0,0.0)
		_Intensity ("Intensity", Range(0.0, 8.0)) = 1.0
		_SideSoftness ("SideSoftness", Range(0.0, 15.0)) = 1.0
		_Jittering ("Jittering", Vector) = (0.0,0.0,0.0,0.0)
		_CameraForwardOS ("Camera Forward OS", Vector) = (0.0,0.0,0.0,1.0)
		_CameraForwardWS ("Camera Forward WS", Vector) = (0.0,0.0,0.0,1.0)
		_TransformScale ("Transform Scale", Vector) = (0.0,0.0,0.0,1.0)
		_CookieTexture ("CookieTexture", 2D) = "white" {}
		_CookieProperties ("CookieProperties", Vector) = (0.0,0.0,0.0,0.0)
		_CookiePosAndScale ("CookiePosAndScale", Vector) = (0.0,0.0,1.0,1.0)
		_ShadowDepthTexture ("ShadowDepthTexture", 2D) = "white" {}
		_ShadowProps ("ShadowProps", Vector) = (1.0,1.0,1.0,1.0)
	}
	SubShader {
		Tags { "DisableBatching"="true" "IgnoreProjector"="true" "Queue"="Transparent" "RenderType"="Transparent" }
	}
}

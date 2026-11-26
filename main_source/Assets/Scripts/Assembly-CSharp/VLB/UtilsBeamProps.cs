using UnityEngine;

namespace VLB
{
	public static class UtilsBeamProps
	{
		public static bool CanChangeDuringPlaytime(VolumetricLightBeamAbstractBase self)
		{
			return false;
		}

		public static Quaternion GetInternalLocalRotation(VolumetricLightBeamAbstractBase self)
		{
			return default;
		}

		public static void SetIntensityFromLight(VolumetricLightBeamAbstractBase self, bool fromLight)
		{
		}

		public static float GetThickness(VolumetricLightBeamAbstractBase self)
		{
			return 0f;
		}

		public static void SetThickness(VolumetricLightBeamAbstractBase self, float value)
		{
		}

		public static float GetFallOffEnd(VolumetricLightBeamAbstractBase self)
		{
			return 0f;
		}

		public static ColorMode GetColorMode(VolumetricLightBeamAbstractBase self)
		{
			return ColorMode.Flat;
		}

		public static Color GetColorFlat(VolumetricLightBeamAbstractBase self)
		{
			return default;
		}

		public static Gradient GetColorGradient(VolumetricLightBeamAbstractBase self)
		{
			return null;
		}

		public static void SetColorFromLight(VolumetricLightBeamAbstractBase self, bool fromLight)
		{
		}

		public static float GetConeAngle(VolumetricLightBeamAbstractBase self)
		{
			return 0f;
		}

		public static void SetSpotAngleFromLight(VolumetricLightBeamAbstractBase self, bool fromLight)
		{
		}

		public static float GetConeRadiusStart(VolumetricLightBeamAbstractBase self)
		{
			return 0f;
		}

		public static float GetConeRadiusEnd(VolumetricLightBeamAbstractBase self)
		{
			return 0f;
		}

		public static int GetSortingLayerID(VolumetricLightBeamAbstractBase self)
		{
			return 0;
		}

		public static int GetSortingOrder(VolumetricLightBeamAbstractBase self)
		{
			return 0;
		}

		public static bool GetFadeOutEnabled(VolumetricLightBeamAbstractBase self)
		{
			return false;
		}

		public static float GetFadeOutEnd(VolumetricLightBeamAbstractBase self)
		{
			return 0f;
		}

		public static void SetFallOffEndFromLight(VolumetricLightBeamAbstractBase self, bool fromLight)
		{
		}

		public static Dimensions GetDimensions(VolumetricLightBeamAbstractBase self)
		{
			return Dimensions.Dim3D;
		}

		public static int GetGeomSides(VolumetricLightBeamAbstractBase self)
		{
			return 0;
		}

		public static AttenuationEquation ConvertAttenuation(AttenuationEquationHD value)
		{
			return AttenuationEquation.Linear;
		}

		public static AttenuationEquationHD ConvertAttenuation(AttenuationEquation value)
		{
			return AttenuationEquationHD.Linear;
		}
	}
}

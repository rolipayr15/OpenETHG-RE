using System;
using UnityEngine;

namespace VLB
{
	public static class Utils
	{
		public enum FloatPackingPrecision
		{
			High = 64,
			Low = 8,
			Undef = 0
		}

		private const float kEpsilon = 1E-05f;

		private static FloatPackingPrecision ms_FloatPackingPrecision;

		private const int kFloatPackingHighMinShaderLevel = 35;

		public static float ComputeConeRadiusEnd(float fallOffEnd, float spotAngle)
		{
			return 0f;
		}

		public static float ComputeSpotAngle(float fallOffEnd, float coneRadiusEnd)
		{
			return 0f;
		}

		public static void Swap<T>(ref T a, ref T b)
		{
		}

		public static void ResizeArray<T>(ref T[] array, int newSize)
		{
		}

		public static bool IsValidIndex<T>(this T[] array, int idx)
		{
			return false;
		}

		public static string GetPath(Transform current)
		{
			return null;
		}

		public static T NewWithComponent<T>(string name) where T : Component
		{
			return null;
		}

		public static T GetOrAddComponent<T>(this GameObject self) where T : Component
		{
			return null;
		}

		public static T GetOrAddComponent<T>(this MonoBehaviour self) where T : Component
		{
			return null;
		}

		public static void ForeachComponentsInAnyChildrenOnly<T>(this GameObject self, Action<T> lambda, bool includeInactive = false) where T : Component
		{
		}

		public static void ForeachComponentsInDirectChildrenOnly<T>(this GameObject self, Action<T> lambda, bool includeInactive = false) where T : Component
		{
		}

		public static void SetupDepthCamera(Camera depthCamera, float coneApexOffsetZ, float maxGeometryDistance, float coneRadiusStart, float coneRadiusEnd, Vector3 beamLocalForward, Vector3 lossyScale, bool isScalable, Quaternion beamInternalLocalRotation, bool shouldScaleMinNearClipPlane)
		{
		}

		public static bool HasFlag(this Enum mask, Enum flags)
		{
			return false;
		}

		public static Vector3 Divide(this Vector3 aVector, Vector3 scale)
		{
			return default;
		}

		public static Vector2 xy(this Vector3 aVector)
		{
			return default;
		}

		public static Vector2 xz(this Vector3 aVector)
		{
			return default;
		}

		public static Vector2 yz(this Vector3 aVector)
		{
			return default;
		}

		public static Vector2 yx(this Vector3 aVector)
		{
			return default;
		}

		public static Vector2 zx(this Vector3 aVector)
		{
			return default;
		}

		public static Vector2 zy(this Vector3 aVector)
		{
			return default;
		}

		public static bool Approximately(this float a, float b, float epsilon = 1E-05f)
		{
			return false;
		}

		public static bool Approximately(this Vector2 a, Vector2 b, float epsilon = 1E-05f)
		{
			return false;
		}

		public static bool Approximately(this Vector3 a, Vector3 b, float epsilon = 1E-05f)
		{
			return false;
		}

		public static bool Approximately(this Vector4 a, Vector4 b, float epsilon = 1E-05f)
		{
			return false;
		}

		public static Vector4 AsVector4(this Vector3 vec3, float w)
		{
			return default;
		}

		public static Vector4 PlaneEquation(Vector3 normalizedNormal, Vector3 pt)
		{
			return default;
		}

		public static float GetVolumeCubic(this Bounds self)
		{
			return 0f;
		}

		public static float GetMaxArea2D(this Bounds self)
		{
			return 0f;
		}

		public static Color Opaque(this Color self)
		{
			return default;
		}

		public static Color ComputeComplementaryColor(this Color self, bool blackAndWhite)
		{
			return default;
		}

		public static Plane TranslateCustom(this Plane plane, Vector3 translation)
		{
			return default;
		}

		public static Vector3 ClosestPointOnPlaneCustom(this Plane plane, Vector3 point)
		{
			return default;
		}

		public static bool IsAlmostZero(float f)
		{
			return false;
		}

		public static bool IsValid(this Plane plane)
		{
			return false;
		}

		public static void SetKeywordEnabled(this Material mat, string name, bool enabled)
		{
		}

		public static void SetShaderKeywordEnabled(string name, bool enabled)
		{
		}

		public static Matrix4x4 SampleInMatrix(this Gradient self, int floatPackingPrecision)
		{
			return default;
		}

		public static Color[] SampleInArray(this Gradient self, int samplesCount)
		{
			return null;
		}

		private static Vector4 Vector4_Floor(Vector4 vec)
		{
			return default;
		}

		public static float PackToFloat(this Color color, int floatPackingPrecision)
		{
			return 0f;
		}

		public static FloatPackingPrecision GetFloatPackingPrecision()
		{
			return FloatPackingPrecision.Undef;
		}

		public static bool HasAtLeastOneFlag(this Enum mask, Enum flags)
		{
			return false;
		}

		public static void MarkCurrentSceneDirty()
		{
		}

		public static void MarkObjectDirty(UnityEngine.Object obj)
		{
		}

		public static bool IsEditorCamera(Camera cam)
		{
			return false;
		}
	}
}

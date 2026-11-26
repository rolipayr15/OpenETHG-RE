using System;
using UnityEngine;

public static class CustomKrasueMediaStore
{
	[Serializable]
	private class FaceData
	{
		public int version;

		public float xTrans;

		public float yTrans;

		public float xScale;

		public float yScale;
	}

	public const int MaxSampleRate = 96000;

	private const int MaxImageBytes = 4194304;

	private const string AudioFile = "custom_krasue_audio.wav";

	private const string ImageFile = "custom_krasue_face.jpg";

	private const string FaceFile = "custom_krasue_face.json";

	private static bool initialized;

	private static FaceData face;

	public static string RestoreWarning { get; private set; }

	public static bool HasReadyAudio => false;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetForPlaySession()
	{
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void Initialize()
	{
	}

	public static bool IsValidClip(AudioClip clip)
	{
		return false;
	}

	public static bool TrySaveAudio(AudioClip clip, out string error)
	{
		error = null;
		return false;
	}

	public static bool TrySaveImage(Texture2D texture, out string error)
	{
		error = null;
		return false;
	}

	public static bool TrySaveFaceSettings(float xTrans, float yTrans, float xScale, float yScale, out string error)
	{
		error = null;
		return false;
	}

	public static void GetFaceSettings(out float xTrans, out float yTrans, out float xScale, out float yScale)
	{
		xTrans = default;
		yTrans = default;
		xScale = default;
		yScale = default;
	}

	private static bool ValidFace(FaceData value)
	{
		return false;
	}

	private static void ApplyFaceSettings()
	{
	}

	private static bool Finite(float value)
	{
		return false;
	}

	private static string FullPath(string name)
	{
		return null;
	}

	private static bool ValidAudioDimensions(int frames, int channels, int rate)
	{
		return false;
	}

	private static byte[] EncodeWav(AudioClip clip)
	{
		return null;
	}

	private static AudioClip DecodeWav(byte[] bytes)
	{
		return null;
	}

	private static Texture2D DecodeLegacyImage(byte[] bytes)
	{
		return null;
	}

	private static void ValidateImage(byte[] bytes)
	{
	}

	private static string ReadValidated(string name, int? maxBytes, Action<byte[]> validate)
	{
		return null;
	}

	private static void WriteAtomic(string name, byte[] bytes, bool keepBackup = true)
	{
	}

	private static void WarnRestore(string warning, Exception cause)
	{
	}
}

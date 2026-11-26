using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ProbeTeleport : MonoBehaviour
{
	public const KeyCode ReleaseKey = KeyCode.Insert;

	private LightingProbeViews views;

	private bool locked;

	private float timeScaleBefore;

	private readonly List<Canvas> hiddenCanvases;

	public static int ViewIndexFor(KeyCode key)
	{
		return 0;
	}

	public static bool Enabled(bool isDebugBuild)
	{
		return false;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Hook()
	{
	}

	public static bool ShouldCreate(string sceneName, bool isDebugBuild, bool alreadyPresent)
	{
		return false;
	}

	private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
	}

	private void Update()
	{
	}

	private void Teleport(int index)
	{
	}

	private void Release()
	{
	}

	private void OnDestroy()
	{
	}
}

using Rewired;
using EyesAnalytics;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MapManager : CustomSingleton<MapManager>
{

	[SerializeField]
	private Canvas map0;

	[SerializeField]
	private Canvas map1;

	[SerializeField]
	private Canvas map2;

	[SerializeField]
	private Canvas mapCanvas;

	[SerializeField]
	private GameObject cancelButton;

	[SerializeField]
	private float fadeInDuration = 0.15f;

	[SerializeField]
	private float fadeOutDuration = 0.12f;

	private CanvasGroup fadeGroup;

	private Coroutine fade;

	private bool isOpen;

	private bool isGhostVisionActive;

	private GameplayInputContext.Lease inputLease;

	private ControlModeManager controlModes;

	private IDisposable worldPause;

	public GameplayInputContext.Lease InputLease => inputLease;

	private CanvasGroup FadeGroup => GetFadeGroup();

	private void OnEnable()
	{
		controlModes = ControlModeManager.Instance;
		if (controlModes != null)
		{
			controlModes.ModeChanged += OnControlModeChanged;
		}
		RefreshCancelButton();
		MessageKit<bool>.addObserver(7, GhostVision);
		MessageKit.addObserver(135, ShowMap);
		MessageKit.addObserver(64, CloseMap);
	}

	private void GhostVision(bool active)
	{
		isGhostVisionActive = active;
	}

	private void Update()
	{
		ControlModeManager manager = ControlModeManager.Instance;
		Player player = manager != null ? manager.Player : null;
		if (!ReInput.isReady || player == null)
		{
			return;
		}
		if (GameplayInputContext.Shared.TryConsumeAction(31, player.GetButtonDown(31), inputLease))
		{
			if (isOpen)
			{
				CloseMap();
			}
			else
			{
				IconPressFeedback.PlayThen(31, ShowMap);
			}
		}
	}

	private void ShowMap()
	{
		GameplayInputContext shared = GameplayInputContext.Shared;
		if (!isActiveAndEnabled || isOpen || mapCanvas == null || isGhostVisionActive || !shared.Current.StartActions)
		{
			return;
		}
		inputLease = shared.Acquire(this, GameplayInputContext.Kind.Map, GameplayInputContext.WindowInput.PointerOnly, CloseMap);
		worldPause = CustomSingleton<PauseManager>.Instance.AcquirePause(this);
		isOpen = true;
		bool wasEnabled = mapCanvas.enabled;
		RefreshCancelButton();
		mapCanvas.enabled = true;
		SetFloors(CustomSingleton<FloorManager>.Instance.GetPlayerFloor());
		if (!wasEnabled)
		{
			FadeGroup.alpha = 0f;
		}
		Fade(1f, fadeInDuration);
		LevelUI.SkillUsed((LevelUI.Skill)2);
	}

	private void OnControlModeChanged(ControlMode mode)
	{
		RefreshCancelButton();
	}

	private void RefreshCancelButton()
	{
		int mode = controlModes != null ? (int)controlModes.Mode : (int)UserGameSettings.controlMode;
		if (cancelButton != null)
		{
			cancelButton.SetActive(mode != 1);
		}
	}

	private void SetFloors(int floor)
	{
		if (map0 != null)
		{
			map0.enabled = floor == 0;
		}
		if (map1 != null)
		{
			map1.enabled = floor == 1;
		}
		if (map2 != null)
		{
			map2.enabled = floor == 2;
		}
	}

	public void CloseMap()
	{
		isOpen = false;
		if (isActiveAndEnabled && mapCanvas != null && mapCanvas.enabled)
		{
			Fade(0f, fadeOutDuration);
		}
		else
		{
			HideMap();
		}
		inputLease?.Dispose();
		inputLease = null;
		worldPause?.Dispose();
		worldPause = null;
	}

	private void HideMap()
	{
		if (fade != null)
		{
			StopCoroutine(fade);
			fade = null;
		}
		if (mapCanvas != null)
		{
			mapCanvas.enabled = false;
		}
		if (fadeGroup != null)
		{
			fadeGroup.alpha = 1f;
			fadeGroup.blocksRaycasts = true;
		}
		SetFloors(-1);
	}

	private void Fade(float to, float duration)
	{
		if (fade != null)
		{
			StopCoroutine(fade);
		}
		FadeGroup.blocksRaycasts = to > 0f;
		fade = StartCoroutine(FadeRoutine(to, duration));
	}

	private IEnumerator FadeRoutine(float to, float duration)
	{
		float from = fadeGroup.alpha;
		float t = 0f;
		while (t < 1f)
		{
			t = duration > 0f ? Mathf.Min(1f, t + Time.unscaledDeltaTime / duration) : 1f;
			fadeGroup.alpha = Mathf.Lerp(from, to, t);
			if (t < 1f)
			{
				yield return null;
			}
		}
		fade = null;
		if (to == 0f)
		{
			HideMap();
		}
	}

	public bool IsOpen()
	{
		return isOpen;
	}

	private void OnDisable()
	{
		if (controlModes != null)
		{
			controlModes.ModeChanged -= OnControlModeChanged;
		}
		controlModes = null;
		MessageKit<bool>.removeObserver(7, GhostVision);
		MessageKit.removeObserver(135, ShowMap);
		MessageKit.removeObserver(64, CloseMap);
		CloseMap();
	}

	protected override void OnDestroy()
	{
		CloseMap();
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}

	private CanvasGroup GetFadeGroup()
	{
		if (fadeGroup == null && !mapCanvas.TryGetComponent(out fadeGroup))
		{
			fadeGroup = mapCanvas.gameObject.AddComponent<CanvasGroup>();
		}
		return fadeGroup;
	}
}

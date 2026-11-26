using Rewired;
using I2.Loc;
using EyesAnalytics;
using Bibloteka;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Vision : MonoBehaviour
{



	private Camera playerCamera;

	private Coroutine visionCoroutine;

	private Coroutine endingCoroutine;

	private bool isVisionActive;

	private bool visionShown;

	private bool isDead;

	private bool cameraWasEnabled;

	private VisionTransitionOverlay transition;

	[SerializeField]
	private float coverDuration = 0.08f;

	[SerializeField]
	private float revealDuration = 0.2f;

	private GameplayFreezeOwnership.Lease freezeLease;

	private GameplayInputContext.Lease inputLease;

	private GameplayInputContext.Lease deathLease;

	private ControlModeManager controlModes;

	[SerializeField]
	private GameObject cancelButton;

	public GameplayInputContext.Lease InputLease => inputLease;

	private VisionTransitionOverlay Transition => transition != null ? transition : (transition = VisionTransitionOverlay.Create());

	private void OnEnable()
	{
		controlModes = ControlModeManager.Instance;
		if (controlModes != null)
		{
			controlModes.ModeChanged += OnControlModeChanged;
		}
		RefreshCancelButton();
		MessageKit.addObserver(17, TurnOffVisionDeath);
		MessageKit.addObserver(53, Revive);
		if (isDead)
		{
			deathLease = GameplayInputContext.Shared.Acquire(this, GameplayInputContext.Kind.Death);
		}
	}

	private void Start()
	{
		playerCamera = CustomSingleton<PlayerManager>.Instance.PlayerCamera.GetComponent<Camera>();
	}

	private void LateUpdate()
	{
		ControlModeManager manager = ControlModeManager.Instance;
		Player player = manager != null ? manager.Player : null;
		if (ReInput.isReady && player != null && player.GetButtonDown(32))
		{
			IconPressFeedback.PlayThen(32, isVisionActive ? new System.Action(TurnOffVision) : new System.Action(EnemyVision));
		}
	}

	private bool ConsumeVisionAction()
	{
		ControlModeManager manager = ControlModeManager.Instance;
		if (!isActiveAndEnabled || isDead || playerCamera == null || manager == null)
		{
			return false;
		}
		if (!manager.isActiveAndEnabled || !manager.HasFocus || !manager.SelectedSourceAvailable || manager.RequiresExplicitResume)
		{
			return false;
		}
		return GameplayInputContext.Shared.TryConsumeAction(32, true, inputLease);
	}

	public void EnemyVision()
	{
		if (isVisionActive || !ConsumeVisionAction())
		{
			return;
		}
		if ((int)UserGameSettings.currentEnemy == 4)
		{
			MessageKit<string>.post(3, ScriptLocalization.SandboxEye);
			return;
		}
		if (CustomSingleton<EyesManager>.Instance.GetAvailableEyes() < 1)
		{
			LevelUI.SkillUsed((LevelUI.Skill)0, false);
			CustomSingleton<MoreEyesManager>.Instance.OpenWindow();
		}
		else if (CanStartVisionOnCurrentLevel())
		{
			visionCoroutine = StartCoroutine(ChangeVision());
			LevelUI.SkillUsed((LevelUI.Skill)0, true);
			CustomSingleton<EyesManager>.Instance.UseEye();
		}
	}

	private bool CanStartVisionOnCurrentLevel()
	{
		int level = (int)UserGameSettings.currentLevel;
		if (level == 1 || level == 3)
		{
			return true;
		}
		return level == 0 && !CustomSingleton<MapManager>.Instance.IsOpen();
	}

	public void TurnOffVision()
	{
		if (isVisionActive && endingCoroutine == null && ConsumeVisionAction())
		{
			endingCoroutine = StartCoroutine(EndVision());
		}
	}

	private IEnumerator ChangeVision()
	{
		TakeVisionOwnership();
		yield return Cover();
		ShowVision();
		Transition.Reveal(revealDuration);
		yield return Utils.GetDelay(4f);
		visionCoroutine = null;
		endingCoroutine = StartCoroutine(EndVision());
	}

	private IEnumerator EndVision()
	{
		if (visionCoroutine != null)
		{
			StopCoroutine(visionCoroutine);
			visionCoroutine = null;
		}
		yield return Cover();
		endingCoroutine = null;
		FinishVision();
		Transition.Reveal(revealDuration);
	}

	private IEnumerator Cover()
	{
		VisionTransitionOverlay overlay = Transition;
		overlay.Cover(coverDuration);
		while (overlay != null && !overlay.IsCovered)
		{
			yield return null;
		}
	}

	private void BeginVision()
	{
		TakeVisionOwnership();
		ShowVision();
	}

	private void TakeVisionOwnership()
	{
		inputLease = GameplayInputContext.Shared.Acquire(this, GameplayInputContext.Kind.Vision);
		Game.NoteActivationCause((ActivationCause)4);
		MessageKit.post(25);
		FreezePlayer();
		isVisionActive = true;
	}

	private void ShowVision()
	{
		cameraWasEnabled = playerCamera.enabled;
		visionShown = true;
		RefreshCancelButton();
		MessageKit<bool>.post(7, true);
		playerCamera.enabled = false;
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
			cancelButton.SetActive(visionShown && mode != 1);
		}
	}

	private void FinishVision()
	{
		if (visionCoroutine != null)
		{
			StopCoroutine(visionCoroutine);
			visionCoroutine = null;
		}
		if (endingCoroutine != null)
		{
			StopCoroutine(endingCoroutine);
			endingCoroutine = null;
		}
		bool wasShown = visionShown;
		isVisionActive = false;
		visionShown = false;
		if (cancelButton != null)
		{
			cancelButton.SetActive(false);
		}
		if (wasShown)
		{
			if (playerCamera != null)
			{
				playerCamera.enabled = cameraWasEnabled;
			}
			MessageKit<bool>.post(7, false);
		}
		RestorePlayerFreeze();
		inputLease?.Dispose();
		inputLease = null;
	}

	private void FreezePlayer()
	{
		vp_FPPlayerEventHandler handler = CustomSingleton<PlayerManager>.Instance.playerHandler;
		if (handler != null)
		{
			freezeLease = GameplayFreezeOwnership.Acquire(handler);
		}
	}

	private void RestorePlayerFreeze()
	{
		GameplayInputContext shared = GameplayInputContext.Shared;
		bool handoff = isDead || shared.HasActive(GameplayInputContext.Kind.Death) || shared.HasActive(GameplayInputContext.Kind.Transition);
		freezeLease?.Release(handoff);
		freezeLease = null;
	}

	private void TurnOffVisionDeath()
	{
		isDead = true;
		if (deathLease == null)
		{
			deathLease = GameplayInputContext.Shared.Acquire(this, GameplayInputContext.Kind.Death);
		}
		FinishVision();
		if (transition != null)
		{
			transition.Hide();
		}
	}

	private void Revive()
	{
		isDead = false;
		deathLease?.Dispose();
		deathLease = null;
	}

	private void OnDisable()
	{
		if (controlModes != null)
		{
			controlModes.ModeChanged -= OnControlModeChanged;
		}
		controlModes = null;
		MessageKit.removeObserver(17, TurnOffVisionDeath);
		MessageKit.removeObserver(53, Revive);
		FinishVision();
		if (transition != null)
		{
			transition.Hide();
		}
		deathLease?.Dispose();
		deathLease = null;
	}

	private void OnDestroy()
	{
		OnDisable();
		if (transition != null)
		{
			Destroy(transition.gameObject);
		}
	}
}

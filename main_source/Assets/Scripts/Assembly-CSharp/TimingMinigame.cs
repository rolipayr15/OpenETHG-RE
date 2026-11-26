using Bibloteka;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class TimingMinigame : CustomSingleton<TimingMinigame>
{



	[SerializeField]
	private UICircle neutralZoneUI;

	[SerializeField]
	private UICircle successZoneUI;

	[SerializeField]
	private RectTransform pointerUI;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private RectTransform minigameObject;

	[SerializeField]
	private Button solvingButton;

	[SerializeField]
	private Image pointerImage;

	[SerializeField]
	private GameObject tapIcon;

	[SerializeField]
	private GameObject background;

	private float neutralPercentage = 0.8f;

	private float difficulty = 0.8f;

	private bool pointerMovementEnabled;

	private Zone neutralZone;

	private Zone successZone;

	private Action<bool> callback;

	private GameplayInputContext.Lease inputLease;

	private GameplayFreezeOwnership.Lease freezeLease;

	private bool resolving;

	private bool isDead;

	private float pointerRotation;

	private float pointer_t;

	private bool alreadyActive;

	public GameplayInputContext.Lease InputLease => inputLease;

	private void OnEnable()
	{
		isDead = GameplayInputContext.Shared.HasActive(GameplayInputContext.Kind.Death);
		MessageKit.addObserver(17, PlayerIsDead);
		MessageKit.addObserver(53, PlayerRevived);
	}

	private IEnumerator StartMinigameCoroutine()
	{
		background.SetActive(true);
		minigameObject.localRotation = Quaternion.identity;
		ActivateMinigameObjects();
		tapIcon.SetActive(true);
		minigameObject.localScale = Vector3.zero;
		successZoneUI.rectTransform.localRotation = Quaternion.identity;
		solvingButton.enabled = true;
		freezeLease = GameplayFreezeOwnership.Acquire(CustomSingleton<PlayerManager>.Instance.playerHandler);
		pointerMovementEnabled = true;
		switch ((int)UserGameSettings.currentDifficulty)
		{
		case 0:
			difficulty = 0.6f;
			break;
		case 1:
			difficulty = 0.7f;
			break;
		case 2:
			difficulty = 0.8f;
			break;
		case 3:
			difficulty = 0.85f;
			break;
		case 4:
			difficulty = 0.9f;
			break;
		}
		float span = neutralPercentage * 360f;
		neutralZone = new Zone(0f, span);
		float width = span * (1f - difficulty);
		successZone = new Zone(0f, width);
		successZone.Shift(UnityEngine.Random.Range(0f, span - width));
		minigameObject.Rotate(0f, 0f, (neutralZone.start - neutralZone.end + 360f) * 0.5f + 90f);
		neutralZoneUI.FillPercent = neutralZone.GetFillPercentage();
		successZoneUI.FillPercent = successZone.GetFillPercentage();
		successZoneUI.rectTransform.Rotate(0f, 0f, successZone.start);
		pointerImage.color = Color.white;
		yield return Utils.ChangeOverTime(0.2f, delegate(float t)
		{
			minigameObject.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
		});
	}

	private void Update()
	{
		if (!pointerMovementEnabled)
		{
			return;
		}
		pointer_t += Time.deltaTime;
		pointerRotation = Mathf.SmoothStep(0f, neutralZone.end - neutralZone.start, Mathf.PingPong(pointer_t, 1f));
		pointerUI.localRotation = Quaternion.Euler(0f, 0f, pointerRotation);
		TrySolveFromSubmit();
	}

	public void StartMinigame(Action<bool> action)
	{
		if (alreadyActive || isDead || !isActiveAndEnabled)
		{
			return;
		}
		resolving = false;
		alreadyActive = true;
		inputLease = GameplayInputContext.Shared.Acquire(this, GameplayInputContext.Kind.Minigame, GameplayInputContext.WindowInput.Submit);
		callback = action;
		StartCoroutine(StartMinigameCoroutine());
	}

	public void SolveMinigame()
	{
		if (alreadyActive && !resolving && isActiveAndEnabled && GameplayInputContext.Shared.TryConsumePointer(inputLease))
		{
			BeginSolve();
		}
	}

	private void TrySolveFromSubmit()
	{
		if (!alreadyActive || resolving || inputLease == null || !ControlModeManager.HasInstance)
		{
			return;
		}
		Rewired.Player player = ControlModeManager.Instance.Player;
		if (player != null && player.GetButtonDown(47) && GameplayInputContext.Shared.TryConsumeAction(47, true, inputLease))
		{
			BeginSolve();
		}
	}

	private void BeginSolve()
	{
		if (!alreadyActive || resolving)
		{
			return;
		}
		resolving = true;
		tapIcon.SetActive(false);
		background.SetActive(false);
		solvingButton.enabled = false;
		ReleaseFreeze();
		StartCoroutine(SolveMinigameCoroutine());
	}

	private IEnumerator SolveMinigameCoroutine()
	{
		bool success = successZone.start < pointerRotation && pointerRotation < successZone.end;
		pointerMovementEnabled = false;
		yield return HighlightPointerWithColor(0.2f, success ? new Color(0f, 1f, 0f, 1f) : new Color(1f, 0f, 0f, 1f));
		yield return Utils.GetDelay(0.2f);
		yield return Utils.ChangeOverTime(0.2f, delegate(float t)
		{
			minigameObject.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
		});
		try
		{
			callback?.Invoke(success);
		}
		finally
		{
			callback = null;
			DeactivateMinigameObjects();
			alreadyActive = false;
			resolving = false;
			ReleaseInput();
		}
	}

	private void ActivateMinigameObjects()
	{
		minigameObject.gameObject.SetActive(true);
	}

	private void DeactivateMinigameObjects()
	{
		minigameObject.gameObject.SetActive(false);
	}

	private void PlayerIsDead()
	{
		isDead = true;
		if (alreadyActive)
		{
			DeactivateMinigameObjects();
			BeginSolve();
			ReleaseFreeze();
			ReleaseInput();
		}
	}

	private void PlayerRevived()
	{
		isDead = false;
	}

	private void ReleaseFreeze()
	{
		GameplayInputContext shared = GameplayInputContext.Shared;
		bool handoff = isDead || shared.HasActive(GameplayInputContext.Kind.Death) || shared.HasActive(GameplayInputContext.Kind.Transition);
		freezeLease?.Release(handoff);
		freezeLease = null;
	}

	private void ReleaseInput()
	{
		inputLease?.Dispose();
		inputLease = null;
	}

	private void OnDisable()
	{
		MessageKit.removeObserver(17, PlayerIsDead);
		MessageKit.removeObserver(53, PlayerRevived);
		StopAllCoroutines();
		callback = null;
		bool wasActive = alreadyActive;
		pointerMovementEnabled = false;
		resolving = false;
		alreadyActive = false;
		if (wasActive)
		{
			if (minigameObject != null)
			{
				DeactivateMinigameObjects();
			}
			if (tapIcon != null)
			{
				tapIcon.SetActive(false);
			}
			if (background != null)
			{
				background.SetActive(false);
			}
			if (solvingButton != null)
			{
				solvingButton.enabled = false;
			}
		}
		ReleaseFreeze();
		ReleaseInput();
	}

	private IEnumerator HighlightPointerWithColor(float overTime, Color highlightColor)
	{
		pointerImage.color = highlightColor;
		Vector3 start = pointerUI.localScale;
		Vector3 target = start * 1.3f;
		yield return Utils.ChangeOverTime(overTime, delegate(float t)
		{
			pointerUI.localScale = Vector3.Lerp(start, target, Mathf.PingPong(t + t, 1f));
		});
	}

	protected override void OnDestroy()
	{
		OnDisable();
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}

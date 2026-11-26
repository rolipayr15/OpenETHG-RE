using System;
using ExternalApi.Models;
using Multiplayer.ExternalApi;
using Prime31.MessageKit;
using Reflex.Attributes;
using UnityEngine;

public class DistanceTracker : MonoBehaviour
{
	[Inject]
	private IUserApi userApi;

	[Inject]
	private IStatsApi statsApi;

	[Header("Konfiguracja")]
	[Tooltip("Liczyć tylko po płaszczyźnie (X,Z), ignorując zmiany Y.")]
	public bool horizontalOnly = true;

	[Tooltip("Ignoruj pojedynczy skok pozycji większy niż ten próg (m) – zabezpiecza przed teleportami/respawnem).")]
	public float maxStepMeters = 3f;

	[Tooltip("Minimalna prędkość (m/s), poniżej której nie liczymy (filtr drobnych drgań).")]
	public float minSpeedToCount = 0.05f;

	[Tooltip("Co jaki interwał (sek) wysyłać inkrement (np. do serwera). Ustaw 0, aby wyłączyć.")]
	public float reportIntervalSeconds;

	private double currentGameDistanceTmp;

	private int currentGameStepsTmp;

	private Vector3 _lastPos;

	private bool _hasLastPos;

	private CharacterController _cc;

	private Rigidbody _rb;

	private vp_PlayerFootFXHandler _footstepHandler;

	private bool _isMultiplayerSession;

	public uint SessionRevision { get; private set; }

	public bool TryGetCurrentGameSteps(out int steps)
	{
		steps = currentGameStepsTmp;
		if (SessionRevision == 0)
		{
			return false;
		}
		return _footstepHandler != null;
	}

	private void Awake()
	{
		ResetSession();
		_cc = GetComponent<CharacterController>();
		_rb = GetComponent<Rigidbody>();
		_footstepHandler = GetComponent<vp_PlayerFootFXHandler>();
		_isMultiplayerSession = UserGameSettings.IsMultiplayer();
		if (_footstepHandler != null)
		{
			_footstepHandler.StepPerformed += CountStep;
		}
		if (!_isMultiplayerSession)
		{
			MessageKit.addObserver(26, SaveLocal);
			MessageKit.addObserver(90, SaveLocal);
		}
		else
		{
			MessageKit<GameFinishedReason>.addObserver(105, SaveLocal);
		}
	}

	private void OnEnable()
	{
		CaptureStartPosition();
		LoadLocal();
	}

	public void CaptureStartPosition()
	{
		_lastPos = base.transform.position;
		_hasLastPos = true;
	}

	private void Update()
	{
		if (!_hasLastPos)
		{
			CaptureStartPosition();
		}
		Vector3 position = base.transform.position;
		Vector3 step = position - _lastPos;
		if (horizontalOnly)
		{
			step.y = 0f;
		}
		float distance = step.magnitude;
		if (distance <= maxStepMeters && distance / Mathf.Max(Time.deltaTime, 1E-06f) >= minSpeedToCount)
		{
			AddMeters(distance);
		}
		_lastPos = position;
	}

	public void AddMeters(double meters)
	{
		if (meters > 0.0)
		{
			currentGameDistanceTmp = Math.Round(Math.Round(meters, 2, MidpointRounding.AwayFromZero) + currentGameDistanceTmp, 2, MidpointRounding.AwayFromZero);
		}
	}

	public void ResetSession()
	{
		currentGameDistanceTmp = 0.0;
		currentGameStepsTmp = 0;
		SessionRevision++;
		UserGameSettings.currentGameSteps = 0;
	}

	private void CountStep()
	{
		if (currentGameStepsTmp != int.MaxValue)
		{
			currentGameStepsTmp++;
		}
		UserGameSettings.currentGameSteps = currentGameStepsTmp;
	}

	public void NotifyTeleported()
	{
		CaptureStartPosition();
	}

	public void LoadLocal()
	{
		UserGameSettings.totalDistance = (PlayerPrefs.HasKey("WalkedDistance") ? PlayerPrefs.GetInt("WalkedDistance", 0) : 0);
	}

	public async void SaveLocal()
	{
		UserGameSettings.currentDistance = (int)(currentGameDistanceTmp * 0.75);
		UserGameSettings.totalDistance += UserGameSettings.currentDistance;
		PlayerPrefs.SetInt("WalkedDistance", UserGameSettings.totalDistance);
		DistanceData data = new DistanceData
		{
			distance = UserGameSettings.currentDistance
		};
		if (_isMultiplayerSession)
		{
			data.gameMultiId = Uri.UnescapeDataString(UserGameSettings.multiplayerGameId);
		}
		else
		{
			data.gameSingleId = UserGameSettings.singleGameId;
		}
		await userApi.SetDistanceAsync(data);
		statsApi.InvalidateStatsCache();
	}

	public void SaveLocal(GameFinishedReason reason)
	{
		SaveLocal();
	}

	private void OnDestroy()
	{
		if (_footstepHandler != null)
		{
			_footstepHandler.StepPerformed -= CountStep;
		}
		if (!_isMultiplayerSession)
		{
			MessageKit.removeObserver(26, SaveLocal);
			MessageKit.removeObserver(90, SaveLocal);
		}
		else
		{
			MessageKit<GameFinishedReason>.removeObserver(105, SaveLocal);
		}
	}
}

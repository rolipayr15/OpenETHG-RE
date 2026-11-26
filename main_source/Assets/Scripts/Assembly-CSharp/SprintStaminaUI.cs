using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;

public class SprintStaminaUI : MonoBehaviour
{
	[Header("UI")]
	[SerializeField]
	private Image staminaCircle;

	[SerializeField]
	private Image buttonImage;

	private SprintIconPulse sprintIconPulse;

	private Button sprintButton;

	private const float LowStaminaThreshold = 0.1f;

	private const float StandStillDisableSeconds = 2f;

	private const float MoveInputThreshold = 0.1f;

	private float _currentStamina;

	private float standStillTime;

	private float maxStamina;

	private float inverseMaxStamina;

	private float staminaRegenPerSecond;

	private float heavyBreathingThreshold;

	[SerializeField]
	private SprintManager sprintManager;

	public bool IsSprinting { get; private set; }

	private void Awake()
	{
		transform.localScale = Vector3.one;
		float max = UserGameSettings.maxStamina;
		heavyBreathingThreshold = max * 0.25f;
		maxStamina = max;
		inverseMaxStamina = 1f / max;
		_currentStamina = max;
		staminaRegenPerSecond = UserGameSettings.staminaRegenPerSecond;
		if (buttonImage != null)
		{
			sprintButton = buttonImage.GetComponent<Button>();
			sprintIconPulse = buttonImage.GetComponent<SprintIconPulse>();
			if (sprintIconPulse == null)
			{
				sprintIconPulse = buttonImage.gameObject.AddComponent<SprintIconPulse>();
			}
		}
		if (sprintButton == null)
		{
			sprintButton = GetComponent<Button>();
		}
		UpdateUI();
		MessageKit.addObserver(17, StopSprint);
		MessageKit.addObserver(53, Revive);
	}

	private void Revive()
	{
		_currentStamina = maxStamina;
		UpdateUI();
	}

	private void OnEnable()
	{
		GameplayInputContext.Shared.InputResetRequested += OnInputReset;
	}

	private void OnDisable()
	{
		GameplayInputContext.Shared.InputResetRequested -= OnInputReset;
		StopSprint();
	}

	private void OnDestroy()
	{
		MessageKit.removeObserver(17, StopSprint);
		MessageKit.removeObserver(53, Revive);
	}

	private void OnInputReset(GameplayInputContext.ResetReason reason)
	{
		if ((uint)((int)reason - 1) < 4u)
		{
			StopSprint();
		}
	}

	public void StopSprint()
	{
		if (!IsSprinting)
		{
			return;
		}
		IsSprinting = false;
		standStillTime = 0f;
		if (sprintIconPulse != null)
		{
			sprintIconPulse.StopPulse();
		}
		sprintManager.IsSprinting(false);
	}

	private bool IsPlayerMoving()
	{
		vp_FPPlayerEventHandler handler = CustomSingleton<PlayerManager>.Instance.playerHandler;
		if (handler == null)
		{
			return false;
		}
		Vector2 input = handler.InputMoveVector.Get();
		return input.x * input.x + input.y * input.y > 0.010000001f;
	}

	private void Update()
	{
		if (!IsSprinting)
		{
			if (_currentStamina < maxStamina)
			{
				_currentStamina = Mathf.Min(maxStamina, _currentStamina + staminaRegenPerSecond * Time.deltaTime);
				UpdateUI();
			}
			return;
		}
		if (!IsPlayerMoving())
		{
			standStillTime += Time.deltaTime;
			ControlModeManager manager = ControlModeManager.Instance;
			if (standStillTime >= StandStillDisableSeconds && (manager == null || !SprintManager.IsHoldMode(manager.Mode)))
			{
				StopSprint();
			}
		}
		else
		{
			standStillTime = 0f;
			_currentStamina -= UserGameSettings.staminaDrainPerSecond * Time.deltaTime;
			if (_currentStamina <= 0f)
			{
				_currentStamina = 0f;
				StopSprint();
				MasterAudio.PlaySound3DAtTransform("stamina", CustomSingleton<PlayerManager>.Instance.Player.transform, 0.5f, null, 0f, "heavy-breathing");
			}
		}
		UpdateUI();
	}

	private void UpdateUI()
	{
		float fraction = _currentStamina * inverseMaxStamina;
		float low = fraction < LowStaminaThreshold ? Mathf.Clamp01(fraction / -LowStaminaThreshold + 1f) : 0f;
		if (sprintButton != null)
		{
			sprintButton.interactable = fraction >= LowStaminaThreshold || IsSprinting;
		}
		Color color = new Color(low * 0f + 1f, 1f - low, 1f - low, low * 0f + 1f);
		if (staminaCircle != null)
		{
			staminaCircle.fillAmount = fraction;
			staminaCircle.color = color;
		}
		if (buttonImage != null)
		{
			buttonImage.color = color;
		}
	}

	public void OnSprintButtonClicked()
	{
		ControlModeManager manager = ControlModeManager.Instance;
		if (manager == null || SprintManager.IsHoldMode(manager.Mode) || !TryConsumeRunPress(manager))
		{
			return;
		}
		if (IsSprinting || maxStamina * LowStaminaThreshold <= _currentStamina)
		{
			SetSprinting(!IsSprinting);
			return;
		}
		IsSprinting = false;
		if (sprintIconPulse != null)
		{
			sprintIconPulse.StopPulse();
		}
		if (sprintManager != null)
		{
			sprintManager.IsSprinting(false);
		}
	}

	public void StartSprintFromHold()
	{
		ControlModeManager manager = ControlModeManager.Instance;
		if (!IsSprinting && manager != null && SprintManager.IsHoldMode(manager.Mode) && TryConsumeRunPress(manager) && maxStamina * LowStaminaThreshold <= _currentStamina)
		{
			SetSprinting(true);
		}
	}

	private bool TryConsumeRunPress(ControlModeManager controls)
	{
		if (!isActiveAndEnabled || sprintManager == null || !sprintManager.isActiveAndEnabled)
		{
			return false;
		}
		if (!controls.isActiveAndEnabled || !controls.HasFocus || !controls.SelectedSourceAvailable || controls.RequiresExplicitResume)
		{
			return false;
		}
		return GameplayInputContext.Shared.TryConsumeAction(8, true);
	}

	private void SetSprinting(bool sprinting)
	{
		IsSprinting = sprinting;
		standStillTime = 0f;
		if (sprintIconPulse != null)
		{
			sprintIconPulse.SetPulsing(sprinting);
		}
		sprintManager.IsSprinting(IsSprinting);
	}

	public void SetSprintBoost()
	{
		float max = UserGameSettings.maxStaminaAfterBoost;
		maxStamina = max;
		_currentStamina = max;
		inverseMaxStamina = 1f / max;
		staminaRegenPerSecond = UserGameSettings.staminaRegenPerSecondAfterBoost;
		UpdateUI();
	}
}

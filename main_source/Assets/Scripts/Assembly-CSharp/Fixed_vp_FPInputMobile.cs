using Gameplay.Eggs;
using UnityEngine;

public class Fixed_vp_FPInputMobile : vp_FPInputMobile
{
	private GameplayInputContext policy;

	private EggEffects eggEffects;

	private int inputResetFrame = -1;

	private int lookResetFrame = -1;

	protected override Vector2 OnValue_InputMoveVector
	{
		get
		{
			if (!CanReadGameplay(false))
			{
				m_MoveVector = Vector2.zero;
			}
			return m_MoveVector;
		}
		set
		{
			if (value.sqrMagnitude >= 1E-10f && !CanReadGameplay(false))
			{
				value = Vector2.zero;
			}
			base.OnValue_InputMoveVector = value;
		}
	}

	protected override bool OnValue_InputAllowGameplay
	{
		get
		{
			return m_AllowGameplayInput;
		}
		set
		{
			if (m_AllowGameplayInput != value)
			{
				m_AllowGameplayInput = value;
				ResetInput(default(GameplayInputContext.ResetReason));
			}
		}
	}

	protected override float OnValue_InputClimbVector
	{
		get
		{
			if (!CanReadGameplay(false, true))
			{
				return 0f;
			}
			return base.OnValue_InputClimbVector;
		}
	}

	protected override Vector2 OnValue_InputSmoothLook
	{
		get
		{
			Vector2 look = base.OnValue_InputSmoothLook;
			return (eggEffects != null) ? eggEffects.ModifyLookInput(look) : look;
		}
	}

	protected override Vector2 OnValue_InputRawLook
	{
		get
		{
			Vector2 look = base.OnValue_InputRawLook;
			return (eggEffects != null) ? eggEffects.ModifyLookInput(look) : look;
		}
	}

	private static bool MouseLookAllowed => MouseManager.HasConfirmedCapture || MouseManager.UnlockedLook;

	protected override void OnEnable()
	{
		eggEffects = GetComponent<EggEffects>();
		policy = GameplayInputContext.Shared;
		policy.InputResetRequested += ResetInput;
		base.OnEnable();
		ResetInput(default(GameplayInputContext.ResetReason));
	}

	protected override void OnDisable()
	{
		policy.InputResetRequested -= ResetInput;
		ResetInput(default(GameplayInputContext.ResetReason));
		base.OnDisable();
	}

	protected override void Start()
	{
		base.Start();
		UpdateCursorLock();
	}

	private bool CanReadGameplay(bool look, bool climbing = false)
	{
		if (!base.isActiveAndEnabled || inputResetFrame == Time.frameCount)
		{
			return false;
		}
		ControlModeManager manager = ControlModeManager.Instance;
		bool lookAllowed = MouseLookAllowed;
		GameplayInputContext.Permissions state = policy.Current;
		if (manager == null || !manager.isActiveAndEnabled || !manager.HasFocus || manager.RequiresRecoveryUI || base.FPPlayer == null)
		{
			return false;
		}
		if (base.FPPlayer.Pause.Get() || (!look && !climbing && !m_AllowGameplayInput))
		{
			return false;
		}
		bool mouseMode = (int)manager.Mode == 1;
		MouseCursorBlocksMouseLook = !state.Look || (mouseMode && !lookAllowed);
		if (!(look ? state.Look : state.Move))
		{
			return false;
		}
		return !look || !mouseMode || lookAllowed;
	}

	private void ResetInput(GameplayInputContext.ResetReason reason)
	{
		inputResetFrame = Time.frameCount;
		m_MoveVector = Vector2.zero;
		ResetLook();
	}

	private void ResetLook()
	{
		m_CurrentMouseLook = Vector2.zero;
		m_MouseLookRawMove = Vector2.zero;
		m_MouseLookSmoothMove = Vector2.zero;
		m_MouseLookSmoothBuffer.Clear();
		lookResetFrame = Time.frameCount;
		m_LastMouseLookFrame = lookResetFrame;
	}

	protected override void Update()
	{
		UpdateCursorLock();
		if (!CanReadGameplay(false, true))
		{
			ResetInput(default(GameplayInputContext.ResetReason));
		}
		if (m_AllowGameplayInput)
		{
			if (!CanReadGameplay(true))
			{
				ResetLook();
			}
			base.Update();
		}
		else
		{
			base.FPPlayer.InputMoveVector.Set(Vector2.zero);
		}
	}

	protected override void InputMove()
	{
		if (!CanReadGameplay(false))
		{
			ResetInput(default(GameplayInputContext.ResetReason));
		}
		base.InputMove();
	}

	protected override Vector2 GetMouseLook()
	{
		if (CanReadGameplay(true))
		{
			return base.GetMouseLook();
		}
		ResetLook();
		return Vector2.zero;
	}

	protected override Vector2 GetMouseLookRaw()
	{
		if (!CanReadGameplay(true))
		{
			ResetLook();
		}
		else if (lookResetFrame != Time.frameCount)
		{
			return base.GetMouseLookRaw();
		}
		return Vector2.zero;
	}

	protected override void UpdateCursorLock()
	{
		ControlModeManager manager = ControlModeManager.Instance;
		bool lookBlocked = !MouseLookAllowed;
		if (manager != null && policy.Current.Look)
		{
			MouseCursorBlocksMouseLook = (int)manager.Mode == 1 && lookBlocked;
		}
		else
		{
			MouseCursorBlocksMouseLook = true;
		}
	}

	protected override void InputRun()
	{
	}
}

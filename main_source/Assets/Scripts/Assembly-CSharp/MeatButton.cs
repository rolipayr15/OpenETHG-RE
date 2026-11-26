public class MeatButton : HideWhenGamePaused
{
	private bool initialized;

	public override void Start()
	{
		initialized = true;
		if ((int)UserGameSettings.currentEnemy == 2)
		{
			base.Start();
			return;
		}
		isActive = false;
		gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		if (initialized && (int)UserGameSettings.currentEnemy == 2)
		{
			SetPresentationObservers(true);
		}
	}

	private void OnDisable()
	{
		SetPresentationObservers(false);
	}

	private void OnDestroy()
	{
		SetPresentationObservers(false);
	}

	private void LateUpdate()
	{
		ControlModeManager controls = ControlModeManager.Instance;
		Rewired.Player player = controls != null ? controls.Player : null;
		if (Rewired.ReInput.isReady && player != null && player.GetButtonDown(51))
		{
			TryToThrowMeat();
		}
	}

	public void TryToThrowMeat()
	{
		ControlModeManager controls = ControlModeManager.Instance;
		if (initialized && isActiveAndEnabled && (int)UserGameSettings.currentEnemy == 2 && controls != null && controls.isActiveAndEnabled && controls.HasFocus && controls.SelectedSourceAvailable && !controls.RequiresExplicitResume && GameplayInputContext.Shared.TryConsumeAction(51, true))
		{
			Prime31.MessageKit.MessageKit.post(65);
		}
	}
}

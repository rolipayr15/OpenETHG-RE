using Prime31.MessageKit;
public class EyeButton : HideWhenGamePaused
{
	private void Awake()
	{
		if (TutorialManager.ShouldPlayTutorial() && !PlayerPrefz.GetBool("EYE_TUTORIAL_COMPLETED", false))
		{
			MessageKit.addObserver(1, ShowButton);
			isActive = false;
			Hide(true);
		}
	}

	private void ShowButton()
	{
		MessageKit.removeObserver(1, ShowButton);
		isActive = true;
		RefreshVisibility();
	}

	private void OnDestroy()
	{
		MessageKit.removeObserver(1, ShowButton);
	}
}

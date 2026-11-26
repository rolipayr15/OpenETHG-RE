using Prime31.MessageKit;
using UnityEngine;

public class InternetConnectionRequired : CustomSingleton<InternetConnectionRequired>
{
	private float lastTimeCheck;

	private Canvas internetConnectionCanvas;

	private bool isWindowOpen;

	private GameplayInputWindow inputWindow;

	private bool adsEnabled;

	private void Awake()
	{
		internetConnectionCanvas = GetComponent<Canvas>();
	}

	private void OnEnable()
	{
		MessageKit.addObserver(64, CloseWindow);
	}

	private void Start()
	{
		lastTimeCheck = Time.time + 25f;
	}

	public void CloseWindow()
	{
		isWindowOpen = false;
		if (internetConnectionCanvas != null)
		{
			internetConnectionCanvas.enabled = false;
		}
		if (inputWindow != null)
		{
			inputWindow.Close();
		}
	}

	public bool IsInternetConnectionCanvasOpened()
	{
		return isWindowOpen;
	}

	public void ShowWindow()
	{
		if (!UserGameSettings.isImmersiveMode && isActiveAndEnabled && !isWindowOpen && internetConnectionCanvas != null)
		{
			inputWindow = GameplayInputWindow.GetOrAdd(this);
			inputWindow.Open(GameplayInputContext.Kind.Modal, GameplayInputContext.WindowInput.Cancel, CloseWindow, false);
			internetConnectionCanvas.enabled = true;
			isWindowOpen = true;
		}
	}

	private void OnDisable()
	{
		MessageKit.removeObserver(64, CloseWindow);
		CloseWindow();
	}

	protected override void OnDestroy()
	{
		MessageKit.removeObserver(64, CloseWindow);
		CloseWindow();
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}

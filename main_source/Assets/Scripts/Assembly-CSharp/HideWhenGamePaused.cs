using System;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;

public class HideWhenGamePaused : MonoBehaviour
{
	[SerializeField]
	private GameObject panel;

	[SerializeField]
	private Image[] images;

	[SerializeField]
	private Text text;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private UltimateChatAutodisabler ownTextEntry;

	private Button button;

	protected bool isActive = true;

	private bool isPlayerDead;

	private bool isGuestDead;

	private bool multiplayerFinished;

	private bool menuButtonsHidden;

	private bool legacyInputOff;

	private bool legacyPaused;

	private bool legacyVision;

	private bool presentationObservers;

	public virtual void Start()
	{
		button = GetComponent<Button>();
		SetPresentationObservers(true);
	}

	protected void SetPresentationObservers(bool enabled)
	{
		if (presentationObservers == enabled)
		{
			return;
		}
		presentationObservers = enabled;
		if (!enabled)
		{
			PauseManager.InputBlockedChanged -= RefreshVisibility;
			GameplayInputContext.Shared.Changed -= RefreshVisibility;
			MessageKit<bool>.removeObserver(6, Pause);
			MessageKit<bool>.removeObserver(7, GhostVision);
			MessageKit.removeObserver(17, Death);
			MessageKit.removeObserver(92, DeathGuest);
			MessageKit.removeObserver(53, Revive);
			MessageKit<bool>.removeObserver(103, ShowButton);
			MessageKit.removeObserver(105, MultiplayerGameFinished);
			MessageKit.removeObserver(40, InputOff);
			MessageKit.removeObserver(39, InputOn);
			return;
		}
		PauseManager.InputBlockedChanged += RefreshVisibility;
		if (ownTextEntry != null)
		{
			GameplayInputContext.Shared.Changed += RefreshVisibility;
		}
		MessageKit<bool>.addObserver(6, Pause);
		MessageKit<bool>.addObserver(7, GhostVision);
		MessageKit.addObserver(17, Death);
		MessageKit.addObserver(92, DeathGuest);
		MessageKit.addObserver(53, Revive);
		MessageKit<bool>.addObserver(103, ShowButton);
		MessageKit.addObserver(105, MultiplayerGameFinished);
		MessageKit.addObserver(40, InputOff);
		MessageKit.addObserver(39, InputOn);
		RefreshVisibility();
	}

	private bool ShouldShow()
	{
		bool blocked;
		if (CustomSingleton<PauseManager>.HasInstance && CustomSingleton<PauseManager>.Instance.ReportsInputBlocked)
		{
			blocked = CustomSingleton<PauseManager>.Instance.InputBlocked;
		}
		else
		{
			blocked = legacyPaused || legacyVision;
		}
		if (blocked)
		{
			if (ownTextEntry == null)
			{
				return false;
			}
			GameplayInputContext.Lease top = GameplayInputContext.Shared.Current.TopOwner;
			if (top == null || (int)top.Context != 6 || (object)top.Owner != ownTextEntry)
			{
				return false;
			}
		}
		if (!isActive || legacyInputOff || isPlayerDead || isGuestDead || multiplayerFinished)
		{
			return false;
		}
		return !menuButtonsHidden;
	}

	protected void RefreshVisibility()
	{
		if (this == null)
		{
			SetPresentationObservers(false);
		}
		else
		{
			Hide(!ShouldShow());
		}
	}

	public void Hide(bool isPaused)
	{
		if (!isActive && !isPaused)
		{
			return;
		}
		if (panel != null)
		{
			panel.SetActive(!isPaused);
		}
		foreach (Image image in images)
		{
			image.enabled = !isPaused;
		}
		if (text != null)
		{
			text.enabled = !isPaused;
		}
		if (!isPaused && button != null && !button.IsInteractable())
		{
			button.interactable = true;
			button.interactable = false;
		}
		if (canvas != null)
		{
			canvas.enabled = !isPaused;
		}
	}

	private void Pause(bool isPaused)
	{
		legacyPaused = isPaused;
		RefreshVisibility();
	}

	private void InputOff()
	{
		if (ownTextEntry == null || !PauseManager.IsPostingInputMirror)
		{
			legacyInputOff = true;
			RefreshVisibility();
		}
	}

	private void InputOn()
	{
		legacyInputOff = false;
		RefreshVisibility();
	}

	private void ShowButton(bool show)
	{
		if (!show)
		{
			menuButtonsHidden = true;
		}
		RefreshVisibility();
	}

	private void Death()
	{
		isPlayerDead = true;
		RefreshVisibility();
	}

	private void DeathGuest()
	{
		isGuestDead = true;
		RefreshVisibility();
	}

	private void Revive()
	{
		isPlayerDead = false;
		RefreshVisibility();
	}

	private void MultiplayerGameFinished()
	{
		multiplayerFinished = true;
		RefreshVisibility();
	}

	private void GhostVision(bool isVisionActive)
	{
		legacyVision = isVisionActive;
		RefreshVisibility();
	}
}

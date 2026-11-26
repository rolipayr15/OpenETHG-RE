using System.Collections.Generic;
using Rewired;
using UnityEngine.SceneManagement;
using Eyes.DailyChallenges;
using UnityEngine;
using UnityEngine.UI;

public sealed class ContinueKeyAction : MonoBehaviour
{
	[SerializeField]
	private Button button;

	private bool waitForRelease = true;

	private bool fired;

	private DailyChallengeResultView[] challenges;

	private WinResultShare[] shares;

	private WinCoinsSummary[] coins;

	private void Start()
	{
		challenges = InScene<DailyChallengeResultView>();
		shares = InScene<WinResultShare>();
		coins = InScene<WinCoinsSummary>();
	}

	private void Update()
	{
		if (fired || button == null || !ReInput.isReady || InteractionManager.RewiredPlayer == null)
		{
			return;
		}
		if (!OwnsInput() || ChildWindowUp())
		{
			waitForRelease = true;
			return;
		}
		if (!waitForRelease)
		{
			bool pressed = InteractionManager.GetButtonDown(33);
			if (GameplayInputContext.Shared.TryConsumeAction(33, pressed))
			{
				fired = true;
				button.onClick?.Invoke();
			}
		}
		else if (!InteractionManager.RewiredPlayer.GetButton(33))
		{
			waitForRelease = false;
		}
	}

	private bool OwnsInput()
	{
		GameplayInputContext.Permissions current = GameplayInputContext.Shared.Current;
		return current.Context == GameplayInputContext.Kind.MainMenu && current.TopOwner == null && button.isActiveAndEnabled && button.IsInteractable();
	}

	private bool ChildWindowUp()
	{
		if (MessageBox.isMessageBoxActive || QuestionBox.IsRequestActive)
		{
			return true;
		}
		if (CustomSingleton<AdsManager>.HasInstance && CustomSingleton<AdsManager>.Instance.IsFullscreenAdBusy)
		{
			return true;
		}
		foreach (DailyChallengeResultView view in challenges)
		{
			if (view != null && view.IsOpen)
			{
				return true;
			}
		}
		foreach (WinResultShare share in shares)
		{
			if (share != null && share.IsSharing)
			{
				return true;
			}
		}
		foreach (WinCoinsSummary coin in coins)
		{
			if (coin != null && coin.IsWaitingForRewardedAd)
			{
				return true;
			}
		}
		Scene own = gameObject.scene;
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene scene = SceneManager.GetSceneAt(i);
			if (scene.isLoaded && scene != own)
			{
				foreach (GameObject root in scene.GetRootGameObjects())
				{
					foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>())
					{
						if (canvas.isActiveAndEnabled)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private T[] InScene<T>() where T : Component
	{
		List<T> found = new List<T>();
		foreach (GameObject root in gameObject.scene.GetRootGameObjects())
		{
			found.AddRange(root.GetComponentsInChildren<T>(true));
		}
		return found.ToArray();
	}
}

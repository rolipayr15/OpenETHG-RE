using UnityEngine.SceneManagement;
using System;
using Rewired;
using UnityEngine;

public class ATTpreConsent : MonoBehaviour
{
	[SerializeField]
	private GameObject[] panelVariants;

	private static Action yesCallback;

	private static Action laterCallback;

	private static int variant;

	public static void Show(int _variant, Action _yesCallback, Action _laterCallback)
	{
		if (_variant == 0)
		{
			_yesCallback();
			return;
		}
		yesCallback = _yesCallback;
		laterCallback = _laterCallback;
		variant = _variant;
		SceneManager.LoadSceneAsync("ATTpreConsent", LoadSceneMode.Additive);
	}

	public void YesOrLaterButtonClicked(bool yesOrLater)
	{
		SceneManager.UnloadSceneAsync("ATTpreConsent", UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
		(yesOrLater ? yesCallback : laterCallback)();
	}

	private void Awake()
	{
		for (int i = 0; i < panelVariants.Length; i++)
		{
			panelVariants[i].SetActive(i == variant - 1);
		}
		InteractionManager.SetRewiredMapping(YesPreConsentRewired, 33);
		InteractionManager.SetRewiredMapping(LaterPreConsentRewired, 34);
	}

	private void OnDestroy()
	{
		InteractionManager.RemoveRewiredMapping(YesPreConsentRewired, 33);
		InteractionManager.RemoveRewiredMapping(LaterPreConsentRewired, 34);
	}

	private void YesPreConsentRewired(InputActionEventData _not_used)
	{
		YesOrLaterButtonClicked(true);
	}

	private void LaterPreConsentRewired(InputActionEventData _not_used)
	{
		YesOrLaterButtonClicked(false);
	}
}

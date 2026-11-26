using UnityEngine.SceneManagement;
using EyesAnalytics;
using Bibloteka;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : CustomSingleton<TutorialManager>
{



	[SerializeField]
	private GameObject eye;

	[SerializeField]
	private GameObject journal;

	[SerializeField]
	private GameObject moveControls;

	private bool isTipOpen;

	private GameplayInputWindow inputWindow;

	[SerializeField]
	private GameObject sprintButton;

	[SerializeField]
	private Button pauseButton;

	[SerializeField]
	private Image pauseImage;

	private bool isMovementTutotialCompleted;

	private bool isEyeTutorialCompleted;

	private bool isJournalTutorialCompleted;

	private bool isMoneyBagTutorialCompleted;

	private bool isCupboardTutorialCompleted;

	private GameplayInputContext.Lease inputLease => inputWindow != null ? inputWindow.Lease : null;

	private void Update()
	{
		if (isTipOpen && inputLease != null && GameplayInputContext.Shared.TryConsumeAction(47, InteractionManager.GetButtonDown(47), inputLease))
		{
			CloseTip();
		}
	}

	private static bool AllStepsCompleted()
	{
		return PlayerPrefz.GetBool("MOVEMENT_TUTORIAL_COMPLETED", false) && PlayerPrefz.GetBool("EYE_TUTORIAL_COMPLETED", false) && PlayerPrefz.GetBool("MONEY_BAG_TUTORIAL_COMPLETED", false) && PlayerPrefz.GetBool("CUPBOARD_TUTORIAL_COMPLETED", false) && PlayerPrefz.GetBool("JOURNAL_TUTORIAL_COMPLETED", false);
	}

	private void CountStepAsCompletedAndPossiblySendTutorialCompletedEvent()
	{
		if (PlayerPrefz.GetBool("TUTORIAL_COMPLETE_REPORTED", false) || !AllStepsCompleted())
		{
			return;
		}
		if (Tutorial.Completed(Time.timeSinceLevelLoad, PlayerPrefs.GetInt("TUTORIAL_ATTEMPTS", 1)))
		{
			PlayerPrefz.SetBool("TUTORIAL_COMPLETE_REPORTED", true);
		}
		UserProperties.Set(U.TutorialDone, true);
	}

	private void Start()
	{
		if (UserGameSettings.isImmersiveMode || !ShouldPlayTutorial())
		{
			return;
		}
		bool completed = AllStepsCompleted();
		UserProperties.Set(U.TutorialDone, completed);
		if (!completed)
		{
			PlayerPrefs.SetInt("TUTORIAL_ATTEMPTS", PlayerPrefs.GetInt("TUTORIAL_ATTEMPTS", 0) + 1);
			if (!PlayerPrefz.GetBool("TUTORIAL_BEGIN_REPORTED", false) && Tutorial.Begin())
			{
				PlayerPrefz.SetBool("TUTORIAL_BEGIN_REPORTED", true);
			}
		}
		isMovementTutotialCompleted = PlayerPrefz.GetBool("MOVEMENT_TUTORIAL_COMPLETED", false);
		isEyeTutorialCompleted = PlayerPrefz.GetBool("EYE_TUTORIAL_COMPLETED", false);
		if (!isEyeTutorialCompleted)
		{
			MessageKit.addObserver(1, EyeFound);
		}
		isJournalTutorialCompleted = PlayerPrefz.GetBool("JOURNAL_TUTORIAL_COMPLETED", false);
		isMoneyBagTutorialCompleted = PlayerPrefz.GetBool("MONEY_BAG_TUTORIAL_COMPLETED", false);
		if (!isMoneyBagTutorialCompleted)
		{
			MessageKit<string>.addObserver(19, MoneyBagFound);
		}
		isCupboardTutorialCompleted = PlayerPrefz.GetBool("CUPBOARD_TUTORIAL_COMPLETED", false);
		if (!isCupboardTutorialCompleted)
		{
			MessageKit.addObserver(59, CupboardOpend);
		}
		StartCoroutine(ObserveJournalAfter3Secs());
	}

	private IEnumerator ObserveJournalAfter3Secs()
	{
		yield return Utils.GetDelay(1f);
		MoveControls();
		if (!isJournalTutorialCompleted)
		{
			yield return Utils.GetDelay(1f);
			MessageKit<string>.addObserver(41, Journal);
		}
	}

	private void MoveControls()
	{
		if (!isActiveAndEnabled || isMovementTutotialCompleted)
		{
			return;
		}
		pauseButton.interactable = false;
		pauseImage.enabled = false;
		sprintButton.SetActive(false);
		moveControls.SetActive(true);
		isTipOpen = true;
		StartCoroutine(CheckIfPlayerHasMoved());
	}

	private void OpenTip()
	{
		inputWindow = GameplayInputWindow.GetOrAdd(this);
		inputWindow.Open(GameplayInputContext.Kind.Modal, GameplayInputContext.WindowInput.Cancel | GameplayInputContext.WindowInput.Submit, CloseTip, true);
		isTipOpen = true;
	}

	private IEnumerator ShowTipWithDelayCoroutine(GameObject _tip)
	{
		yield return Utils.GetDelay(0.5f);
		OpenTip();
		_tip.SetActive(true);
	}

	private void EyeFound()
	{
		if (!isActiveAndEnabled)
		{
			return;
		}
		PlayerPrefz.SetBool("EYE_TUTORIAL_COMPLETED", true);
		MessageKit.removeObserver(1, EyeFound);
		OpenTip();
		eye.SetActive(true);
		Breadcrumbs.Log("tutorial_step_eye");
		Tutorial.StepCompleted((Tutorial.Step)1, Time.timeSinceLevelLoad);
		CountStepAsCompletedAndPossiblySendTutorialCompletedEvent();
	}

	private void MoneyBagFound(string bagId)
	{
		PlayerPrefz.SetBool("MONEY_BAG_TUTORIAL_COMPLETED", true);
		MessageKit<string>.removeObserver(19, MoneyBagFound);
		Breadcrumbs.Log("tutorial_step_collecting");
		Tutorial.StepCompleted((Tutorial.Step)3, Time.timeSinceLevelLoad);
		CountStepAsCompletedAndPossiblySendTutorialCompletedEvent();
	}

	private void CupboardOpend()
	{
		PlayerPrefz.SetBool("CUPBOARD_TUTORIAL_COMPLETED", true);
		MessageKit.removeObserver(59, CupboardOpend);
		Breadcrumbs.Log("tutorial_step_interaction");
		Tutorial.StepCompleted((Tutorial.Step)4, Time.timeSinceLevelLoad);
		CountStepAsCompletedAndPossiblySendTutorialCompletedEvent();
	}

	private IEnumerator CheckIfPlayerHasMoved()
	{
		Transform playerT = CustomSingleton<PlayerManager>.Instance.Player.transform;
		Vector3 playerStartPosition = playerT.position;
		while ((playerStartPosition - playerT.position).sqrMagnitude < 0.2f)
		{
			yield return Utils.GetDelay(0.3f);
		}
		isTipOpen = false;
		pauseButton.interactable = true;
		pauseImage.enabled = true;
		sprintButton.SetActive(true);
		moveControls.SetActive(false);
		MessageKit.post(56);
		PlayerPrefz.SetBool("MOVEMENT_TUTORIAL_COMPLETED", true);
		CloseTip();
		Breadcrumbs.Log("tutorial_step_moving");
		Tutorial.StepCompleted((Tutorial.Step)0, Time.timeSinceLevelLoad);
		CountStepAsCompletedAndPossiblySendTutorialCompletedEvent();
	}

	private void Journal(string entryId)
	{
		if (!isActiveAndEnabled)
		{
			return;
		}
		PlayerPrefz.SetBool("JOURNAL_TUTORIAL_COMPLETED", true);
		MessageKit<string>.removeObserver(41, Journal);
		OpenTip();
		journal.SetActive(true);
		Breadcrumbs.Log("tutorial_step_journal");
		Tutorial.StepCompleted((Tutorial.Step)5, Time.timeSinceLevelLoad);
		CountStepAsCompletedAndPossiblySendTutorialCompletedEvent();
	}

	public void CloseTip()
	{
		if (inputWindow != null)
		{
			inputWindow.Close();
		}
		if (!isTipOpen)
		{
			return;
		}
		isTipOpen = false;
		eye.SetActive(false);
		journal.SetActive(false);
		moveControls.SetActive(false);
	}

	public bool IsOpen()
	{
		return isTipOpen;
	}

	public static bool ShouldPlayTutorial()
	{
		if (SceneManager.GetActiveScene().name != "Mansion" || UserGameSettings.IsMultiplayer())
		{
			return false;
		}
		return (int)UserGameSettings.currentEnemy == 0;
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		CloseTip();
	}

	protected override void OnDestroy()
	{
		CloseTip();
		MessageKit.removeObserver(1, EyeFound);
		MessageKit<string>.removeObserver(41, Journal);
		MessageKit<string>.removeObserver(19, MoneyBagFound);
		MessageKit.removeObserver(59, CupboardOpend);
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}

using Debug = UnityEngine.Debug;
using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Eyes.DailyChallenges;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

public sealed class WinResultShare : MonoBehaviour
{


	[SerializeField]
	private Button shareButton;

	[SerializeField]
	private Text fontSource;

	[SerializeField]
	private WinResultCard cardPrefab;

	[SerializeField]
	private DailyChallengeShareCard challengeCardPrefab;

	[SerializeField]
	private bool isGameOver;

	private WinResultData result;

	private Coroutine captureRoutine;

	private WinResultCard card;

	private DailyChallengeShareCard challengeCard;

	private ShareSheet shareSheet;

	private bool shareSheetSubmitted;

	private bool imageSaveSubmitted;

	private bool isSharing;

	private Button activeSourceButton;

	private Vector2 shareSheetPosition;

	public bool IsSharing => isSharing;

	private void Awake()
	{
		result = WinResultData.Capture(isGameOver);
	}

	private void OnEnable()
	{
		if (shareButton != null)
		{
			shareButton.interactable = !isSharing;
		}
		if (activeSourceButton != null)
		{
			activeSourceButton.interactable = !isSharing;
		}
	}

	public void ShareResult()
	{
		BeginShare(shareButton, null);
	}

	public void ShareResultFrom(Button sourceButton)
	{
		BeginShare(sourceButton, null);
	}

	public void ShareDailyChallengeFrom(Button sourceButton, DailyChallengeCompletion completion)
	{
		if (!isSharing && isActiveAndEnabled && completion != null && completion.Challenge.IsCompleted)
		{
			BeginShare(sourceButton, new DailyChallengeShareData(completion, PlayerPrefs.GetString("Username", string.Empty)));
		}
	}

	private void BeginShare(Button sourceButton, DailyChallengeShareData challengeData)
	{
		if (isSharing || !isActiveAndEnabled)
		{
			return;
		}
		activeSourceButton = sourceButton != null ? sourceButton : shareButton;
		shareSheetPosition = GetShareSheetPosition(activeSourceButton);
		isSharing = true;
		if (shareButton != null)
		{
			shareButton.interactable = false;
		}
		if (activeSourceButton != null)
		{
			activeSourceButton.interactable = false;
		}
		CustomEvents.FirebaseEventResultShare(result.IsGameOver, CustomEvents.ResultShareAction.Clicked);
		captureRoutine = StartCoroutine(CaptureAndShare(challengeData));
	}

	private static Vector2 GetShareSheetPosition(Button sourceButton)
	{
		RectTransform rect = sourceButton != null ? sourceButton.transform as RectTransform : null;
		Canvas canvas = sourceButton != null ? sourceButton.GetComponentInParent<Canvas>() : null;
		if (rect != null && canvas != null)
		{
			Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
			Rect area = rect.rect;
			return RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(new Vector3(area.x + area.width * 0.5f, area.y + area.height * 0.5f, 0f)));
		}
		return new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
	}

	private IEnumerator CaptureAndShare(DailyChallengeShareData challengeData)
	{
		Font font = fontSource != null ? fontSource.font : null;
		if (challengeData == null)
		{
			card = Instantiate(cardPrefab);
			card.Populate(result, font);
		}
		else
		{
			challengeCard = Instantiate(challengeCardPrefab);
			challengeCard.Populate(challengeData, font);
		}
		yield return null;
		captureRoutine = null;
		Texture2D image = null;
		try
		{
			image = challengeData == null ? card.Render() : challengeCard.Render();
			byte[] png = image.EncodeToPNG();
			if (card != null)
			{
				Destroy(card.gameObject);
			}
			card = null;
			if (challengeCard != null)
			{
				Destroy(challengeCard.gameObject);
			}
			challengeCard = null;
			string fileName = challengeData != null ? challengeData.FileName : "Eyes-result.png";
			if (!AndroidResultImageExport.CanShareImage())
			{
				SaveImage(png, fileName);
			}
			else
			{
				shareSheet = ShareSheet.CreateInstance();
				if (!shareSheet)
				{
					throw new InvalidOperationException("The native share sheet is unavailable.");
				}
				shareSheet.AddImage(png, "image/png");
				shareSheet.AddText(challengeData != null ? challengeData.ShareText : result.ShareText);
				ShareSheet pendingSheet = shareSheet;
				bool sharedGameOver = result.IsGameOver;
				shareSheet.SetCompletionCallback(delegate(ShareSheetResult shareResult, Error error)
				{
					CustomEvents.ResultShareAction action = CustomEvents.ResultShareAction.Error;
					if (error == null)
					{
						int code = shareResult != null ? (int)shareResult.ResultCode : -1;
						action = code == 2 ? CustomEvents.ResultShareAction.Done : (code == 1 ? CustomEvents.ResultShareAction.Cancelled : CustomEvents.ResultShareAction.Unknown);
					}
					CustomEvents.FirebaseEventResultShare(sharedGameOver, action);
					if (this != null && shareSheet == pendingSheet)
					{
						shareSheet = null;
						shareSheetSubmitted = false;
						Finish();
						if (error != null)
						{
							ShowError();
						}
					}
				});
				DontDestroyOnLoad(shareSheet.gameObject);
				shareSheetSubmitted = true;
				shareSheet.Show(shareSheetPosition);
			}
		}
		catch (Exception exception)
		{
			Fail(exception);
		}
		if (image != null)
		{
			Destroy(image);
		}
	}

	private void SaveImage(byte[] png, string fileName)
	{
		imageSaveSubmitted = true;
		bool sharedGameOver = result.IsGameOver;
		AndroidResultImageExport.Save(png, fileName, delegate(AndroidResultImageExport.Outcome outcome, string message)
		{
			CustomEvents.FirebaseEventResultShare(sharedGameOver, (int)outcome == 0 ? CustomEvents.ResultShareAction.Saved : ((int)outcome == 1 ? CustomEvents.ResultShareAction.Cancelled : CustomEvents.ResultShareAction.Error));
			if (this != null)
			{
				imageSaveSubmitted = false;
				Finish();
				if ((int)outcome == 2)
				{
					Debug.LogWarning("[ResultShare] Could not save image: " + message, this);
					ShowError();
				}
			}
		});
	}

	private void Fail(Exception exception)
	{
		Debug.LogException(exception, this);
		CustomEvents.FirebaseEventResultShare(result.IsGameOver, CustomEvents.ResultShareAction.Error);
		Cleanup();
		ShowError();
	}

	private void ShowError()
	{
		if (isActiveAndEnabled)
		{
			MessageBox.Show(ScriptLocalization.Error);
		}
	}

	private void Finish()
	{
		isSharing = false;
		if (shareButton != null)
		{
			shareButton.interactable = true;
		}
		if (activeSourceButton != null)
		{
			activeSourceButton.interactable = true;
		}
		activeSourceButton = null;
	}

	private void Cleanup()
	{
		if (captureRoutine != null)
		{
			StopCoroutine(captureRoutine);
		}
		captureRoutine = null;
		if (card != null)
		{
			Destroy(card.gameObject);
		}
		card = null;
		if (challengeCard != null)
		{
			Destroy(challengeCard.gameObject);
		}
		challengeCard = null;
		if (!shareSheetSubmitted && !imageSaveSubmitted)
		{
			if (shareSheet != null)
			{
				Destroy(shareSheet.gameObject);
			}
			shareSheet = null;
			Finish();
		}
	}

	private void OnDisable()
	{
		Cleanup();
	}
}

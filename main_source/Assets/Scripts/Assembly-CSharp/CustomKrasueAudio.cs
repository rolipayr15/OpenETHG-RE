using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DarkTonic.MasterAudio;
using I2.Loc;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Custom Krasue scream: record it from the microphone or import a sound file, preview it, and register it
// with MasterAudio's "CustomKrasue" group for the game.
public class CustomKrasueAudio : CustomSingleton<CustomKrasueAudio>
{
	private enum MicrophonePermissionResult
	{
		Granted = 0,
		Denied = 1,
		Dismissed = 2,
		Unavailable = 3
	}

	// Outlives this component, so a permission dialog answered after the scene changed doesn't leak callbacks.
	private sealed class MicrophonePermissionRequest
	{
		public readonly TaskCompletionSource<MicrophonePermissionResult> Completion = new TaskCompletionSource<MicrophonePermissionResult>();

		public readonly PermissionCallbacks Callbacks = new PermissionCallbacks();
	}

	private const string MicrophonePermission = "android.permission.RECORD_AUDIO";

	private const string PickerCacheName = "pickedMedia";

	private AudioClip audioClip;

	[SerializeField]
	private Button playStopButton;

	[SerializeField]
	private Button recordButton;

	[SerializeField]
	private Button importButton;

	[SerializeField]
	private Image play;

	[SerializeField]
	private Image stop;

	[SerializeField]
	private Image record;

	[SerializeField]
	private Image recordingInProgress;

	[SerializeField]
	private Button playButton;

	[SerializeField]
	private Text playButtonText;

	[SerializeField]
	private Text soundReady;

	private int recordingTime = 3;

	private float lastBlink;

	private bool isPreviewPlaying;

	private bool isRecordingInProgress;

	[SerializeField]
	private GameObject progressBar;

	private string soundGroupName = "CustomKrasue";

	private string soundVartiationName = "customSound";

	[SerializeField]
	private Image progressBarImage;

	private float timeWhenRecordingStarted;

	private float timeWhenPreviewStarted;

	private bool isAudioBusy;

	private bool isPermissionRequested;

	private bool isAudioRegistered;

	private bool hasStarted;

	private bool isDisabling;

	private bool ownsMicrophone;

	private bool isRecordingOperation;

	private bool isApplicationFocused = true;

	private bool isApplicationPaused;

	private bool permissionWasPending;

	// Bumped by every new operation and by cancellation; late callbacks compare against it and bail out.
	private int operationGeneration;

	private readonly List<AudioClip> retiredClips = new List<AudioClip>();

	private bool pickerPending;

	private UnityWebRequest activeImportRequest;

	private string activeImportPath;

	private AudioClip pendingClip;

	[SerializeField]
	private GameObject audioAndroid;

	[FormerlySerializedAs("audioIOS")]
	[SerializeField]
	private GameObject audioRecordPanel;

	private static MicrophonePermissionRequest microphonePermissionRequest;

	private bool HasPendingMicrophonePermission => microphonePermissionRequest != null && !microphonePermissionRequest.Completion.Task.IsCompleted;

	private bool IsBusy => isAudioBusy || isPermissionRequested || HasPendingMicrophonePermission;

	private bool IsForeground => isApplicationFocused && !isApplicationPaused;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetMicrophonePermissionRequest()
	{
		microphonePermissionRequest = null;
	}

	private static string AudioText(string key, string fallback)
	{
		string translation = LocalizationManager.GetTranslation(key);
		return string.IsNullOrEmpty(translation) ? fallback : translation;
	}

	private bool IsCurrentOperation(int generation)
	{
		return this != null && !isDisabling && operationGeneration == generation;
	}

	private bool CanBeginAudioOperation()
	{
		return !isDisabling && IsForeground && !IsBusy && !pickerPending;
	}

	private void Start()
	{
		if (audioAndroid != null)
		{
			audioAndroid.SetActive(true);
		}
		if (audioRecordPanel != null)
		{
			audioRecordPanel.SetActive(true);
		}
		hasStarted = true;
		RefreshSavedAudio();
	}

	private void OnEnable()
	{
		isDisabling = false;
		if (hasStarted)
		{
			RefreshSavedAudio();
		}
	}

	public void RefreshSavedAudio()
	{
		if (isDisabling)
		{
			return;
		}
		if (!IsBusy)
		{
			CustomKrasueMediaStore.Initialize();
			audioClip = UserGameSettings.customKrasueSound;
			if (!CustomKrasueMediaStore.HasReadyAudio)
			{
				isAudioRegistered = false;
			}
			else
			{
				isAudioRegistered = CustomKrasueAudioBinding.TryRegister();
				if (isAudioRegistered)
				{
					ReleaseRetiredClips();
					if (soundReady != null)
					{
						soundReady.text = AudioText("CustomKrasue_SoundReady", "Sound is ready!");
					}
				}
			}
			if (progressBar != null)
			{
				progressBar.SetActive(false);
			}
			if (recordingInProgress != null)
			{
				recordingInProgress.enabled = false;
			}
		}
		RefreshControls();
	}

	public bool CanStartGame(out string error)
	{
		if (IsBusy)
		{
			error = "Please wait until your sound is ready.";
			return false;
		}
		if (UserGameSettings.customKrasueSound == null || !CustomKrasueMediaStore.HasReadyAudio)
		{
			error = "Add a sound for your monster before starting the game.";
			return false;
		}
		if (!CustomKrasueAudioBinding.TryRegister())
		{
			error = "Your sound could not be prepared. Please select or record it again.";
			return false;
		}
		ReleaseRetiredClips();
		error = null;
		return true;
	}

	// Old clips are only destroyed once the new one is registered, so MasterAudio never points at a dead clip.
	private void ReleaseRetiredClips()
	{
		foreach (AudioClip clip in retiredClips)
		{
			if (clip != null && clip != UserGameSettings.customKrasueSound)
			{
				Destroy(clip);
			}
		}
		retiredClips.Clear();
	}

	private void RefreshControls()
	{
		if (isDisabling)
		{
			return;
		}
		permissionWasPending = HasPendingMicrophonePermission;
		bool busy = IsBusy;
		if (importButton != null)
		{
			importButton.interactable = !busy;
		}
		if (recordButton != null)
		{
			recordButton.interactable = !busy;
		}
		if (record != null)
		{
			record.enabled = !isRecordingInProgress;
		}
		ActivatePreviewButton(!busy && isAudioRegistered && CustomKrasueMediaStore.HasReadyAudio);
		if (playButton != null)
		{
			playButton.interactable = !busy;
		}
		if (playButtonText != null)
		{
			playButtonText.color = busy ? Color.grey : Color.white;
		}
	}

	private void ShowError(string message)
	{
		if (this == null || isDisabling)
		{
			return;
		}
		if (soundReady != null)
		{
			soundReady.text = message;
		}
		MessageBox.Show(message);
	}

	private bool AcceptClip(AudioClip candidate)
	{
		AudioClip previous = UserGameSettings.customKrasueSound;
		if (!CustomKrasueMediaStore.TrySaveAudio(candidate, out string error))
		{
			ShowError(error);
			return false;
		}
		audioClip = UserGameSettings.customKrasueSound;
		if (previous != null && previous != audioClip)
		{
			retiredClips.Add(previous);
		}
		isAudioRegistered = CustomKrasueAudioBinding.TryRegister();
		if (!isAudioRegistered)
		{
			ShowError("Your sound was saved, but playback could not be prepared. Please reopen this screen.");
			return false;
		}
		ReleaseRetiredClips();
		if (soundReady != null)
		{
			soundReady.text = AudioText("CustomKrasue_SoundReady", "Sound is ready!");
		}
		return true;
	}

	// NativeGallery copies picked files into the cache; remove copies left by a crash or kill.
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void CleanupPickerCacheOnStartup()
	{
		if (!Directory.Exists(Application.temporaryCachePath))
		{
			return;
		}
		string[] files = Directory.GetFiles(Application.temporaryCachePath, PickerCacheName + "*");
		for (int i = 0; i < files.Length; i++)
		{
			TryDeletePickerCacheCopy(files[i]);
		}
	}

	// Deletes only NativeGallery's own temp copy, never a user file picked in place.
	private static void TryDeletePickerCacheCopy(string path)
	{
		if (string.IsNullOrEmpty(path) || !Path.IsPathRooted(path))
		{
			return;
		}
		string fullPath = Path.GetFullPath(path);
		string cacheDirectory = Path.GetFullPath(Application.temporaryCachePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		if (Path.GetDirectoryName(fullPath) == cacheDirectory && Path.GetFileNameWithoutExtension(fullPath) == PickerCacheName)
		{
			File.Delete(fullPath);
		}
	}

	public void RecordVoice()
	{
		if (!CanBeginAudioOperation())
		{
			return;
		}
		int generation = ++operationGeneration;
		TurnOffPreview();
		if (MasterAudio.SafeInstance != null && MasterAudio.SoundsReady)
		{
			MasterAudio.StopAllOfSound("Menu");
		}
		if (HasMicrophonePermission())
		{
			StartCoroutine(RecordCoroutine(generation));
			return;
		}
		if (!TryGetMicrophoneRationale(out bool _))
		{
			ShowError(AudioText("CustomKrasue_PermissionUnavailable", "Microphone permission could not be checked. Please try again."));
			return;
		}
		MicrophonePermissionRequest request = new MicrophonePermissionRequest();
		microphonePermissionRequest = request;
		request.Callbacks.PermissionGranted += delegate(string permission)
		{
			if (permission == MicrophonePermission)
			{
				request.Completion.TrySetResult(MicrophonePermissionResult.Granted);
			}
		};
		request.Callbacks.PermissionDenied += delegate(string permission)
		{
			if (permission == MicrophonePermission)
			{
				request.Completion.TrySetResult(MicrophonePermissionResult.Denied);
			}
		};
		request.Callbacks.PermissionRequestDismissed += delegate(string permission)
		{
			if (permission == MicrophonePermission)
			{
				request.Completion.TrySetResult(MicrophonePermissionResult.Dismissed);
			}
		};
		isPermissionRequested = true;
		RefreshControls();
		StartCoroutine(WaitForMicrophonePermission(request, generation));
		Permission.RequestUserPermission(MicrophonePermission, request.Callbacks);
	}

	// The answer is applied only once the app is back in the foreground.
	private IEnumerator WaitForMicrophonePermission(MicrophonePermissionRequest request, int generation)
	{
		while (true)
		{
			if (request.Completion.Task.IsCompleted && IsForeground)
			{
				OnMicrophonePermissionResult(request.Completion.Task.Result, generation);
				yield break;
			}
			if (!IsCurrentOperation(generation))
			{
				yield break;
			}
			yield return null;
		}
	}

	private void OnMicrophonePermissionResult(MicrophonePermissionResult result, int generation)
	{
		if (this == null || isDisabling || operationGeneration != generation || !isPermissionRequested)
		{
			return;
		}
		isPermissionRequested = false;
		RefreshControls();
		switch (result)
		{
		case MicrophonePermissionResult.Granted:
			if (IsForeground && HasMicrophonePermission())
			{
				StartCoroutine(RecordCoroutine(generation));
				return;
			}
			ShowError(AudioText("CustomKrasue_MicrophoneUnavailable", "The microphone is unavailable. Check microphone access and try again."));
			break;
		case MicrophonePermissionResult.Denied:
			ShowMicrophonePermissionDenied(generation);
			break;
		case MicrophonePermissionResult.Unavailable:
			ShowError(AudioText("CustomKrasue_PermissionUnavailable", "Microphone permission could not be checked. Please try again."));
			break;
		}
	}

	private static bool TryGetMicrophoneRationale(out bool rationale)
	{
		rationale = false;
		using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
		if (activity == null)
		{
			return false;
		}
		rationale = activity.Call<bool>("shouldShowRequestPermissionRationale", new object[1] { MicrophonePermission });
		return true;
	}

	// No rationale after a denial means "don't ask again": the only way back is the app settings.
	private void ShowMicrophonePermissionDenied(int generation)
	{
		if (!TryGetMicrophoneRationale(out bool rationale))
		{
			ShowError(AudioText("CustomKrasue_PermissionUnavailable", "Microphone permission could not be checked. Please try again."));
			return;
		}
		if (rationale)
		{
			ShowError(AudioText("CustomKrasue_MicrophoneReason", "Allow microphone access to record your monster. You can also choose a sound file."));
			return;
		}
		string message = AudioText("CustomKrasue_MicrophoneSettings", "Microphone access is off. Enable it in app settings to record your monster, or choose a sound file.");
		if (soundReady != null)
		{
			soundReady.text = message;
		}
		QuestionBox.TryShow(message, AudioText("CustomKrasue_OpenSettings", "Open settings"), AudioText("Cancel", "Cancel"), delegate
		{
			if (this != null && !isDisabling && generation == operationGeneration)
			{
				NativeGallery.OpenSettings();
			}
		}, null, null, false, false, GameplayInputContext.Kind.Modal, false);
	}

	public void ImportSound()
	{
		if (!CanBeginAudioOperation())
		{
			return;
		}
		int generation = ++operationGeneration;
		TurnOffPreview();
		MasterAudio.StopAllOfSound("Menu");
		isAudioBusy = true;
		pickerPending = true;
		RefreshControls();
		NativeGallery.GetAudioFromGallery(delegate(string path)
		{
			pickerPending = false;
			if (this == null || isDisabling || generation != operationGeneration)
			{
				TryDeletePickerCacheCopy(path);
				return;
			}
			if (!gameObject.activeInHierarchy || string.IsNullOrEmpty(path))
			{
				TryDeletePickerCacheCopy(path);
				isAudioBusy = false;
				RefreshControls();
				if (gameObject.activeInHierarchy)
				{
					ShowError("No sound was selected. If access was denied, allow it in Settings and try again.");
				}
				return;
			}
			StartCoroutine(PickSoundCoroutine(path));
		}, "Select a WAV, OGG or MP3 sound", "audio/*");
	}

	private static bool TryGetAudioType(string ext, out AudioType audioType)
	{
		switch (ext)
		{
		case ".ogg":
			audioType = AudioType.OGGVORBIS;
			return true;
		case ".mp3":
			audioType = AudioType.MPEG;
			return true;
		case ".wav":
			audioType = AudioType.WAV;
			return true;
		default:
			audioType = AudioType.UNKNOWN;
			return false;
		}
	}

	private bool TryCreateRequest(string path, out UnityWebRequest request)
	{
		request = null;
		if (!TryGetAudioType(Path.GetExtension(path).ToLowerInvariant(), out AudioType audioType))
		{
			ShowError("This sound format is not supported. AAC/M4A recordings cannot be imported yet. Export or convert your recording to WAV, OGG or MP3, then select that file.");
			return false;
		}
		if (!File.Exists(path))
		{
			ShowError("This sound file is no longer accessible. Please select it again.");
			return false;
		}
		request = UnityWebRequestMultimedia.GetAudioClip(new System.Uri(path).AbsoluteUri, audioType);
		request.timeout = 30;
		DownloadHandlerAudioClip handler = (DownloadHandlerAudioClip)request.downloadHandler;
		handler.streamAudio = false;
		handler.compressed = false;
		return true;
	}

	private IEnumerator PickSoundCoroutine(string path)
	{
		activeImportPath = path;
		UnityWebRequest request = null;
		try
		{
			if (!TryCreateRequest(path, out request))
			{
				yield break;
			}
			activeImportRequest = request;
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogWarning(string.Format("[CustomKrasue] Import failed: {0}; {1}", request.result, request.error));
				ShowError("This sound could not be decoded. Please try a different WAV, OGG or MP3 file.");
				yield break;
			}
			pendingClip = DownloadHandlerAudioClip.GetContent(request);
			yield return null;
			AcceptClip(pendingClip);
			request = null;
		}
		finally
		{
			ReleasePendingClip();
			CleanupActiveImport();
			isAudioBusy = false;
			if (this != null)
			{
				RefreshControls();
			}
		}
	}

	private void CleanupActiveImport()
	{
		UnityWebRequest request = activeImportRequest;
		activeImportRequest = null;
		if (request != null)
		{
			if (!request.isDone)
			{
				request.Abort();
			}
			request.Dispose();
		}
		TryDeletePickerCacheCopy(activeImportPath);
		activeImportPath = null;
	}

	private bool HasMicrophonePermission()
	{
		return Permission.HasUserAuthorizedPermission(MicrophonePermission);
	}

	private bool TryStartMicrophone()
	{
		if (!HasMicrophonePermission() || Microphone.devices.Length == 0)
		{
			return false;
		}
		ownsMicrophone = true;
		pendingClip = Microphone.Start(null, false, recordingTime + 1, 22050);
		if (pendingClip != null)
		{
			return true;
		}
		StopMicrophone();
		return false;
	}

	private bool TryReadMicrophoneCapture(out int position, out bool recording)
	{
		position = Microphone.GetPosition(null);
		recording = Microphone.IsRecording(null);
		return true;
	}

	private IEnumerator RecordCoroutine(int generation)
	{
		if (!IsCurrentOperation(generation) || !IsForeground)
		{
			yield break;
		}
		isRecordingOperation = true;
		isAudioBusy = true;
		RefreshControls();
		try
		{
			// Let the "Menu" music fade out so it isn't captured.
			yield return new WaitForSecondsRealtime(0.5f);
			if (!IsCurrentOperation(generation) || !IsForeground)
			{
				yield break;
			}
			if (!TryStartMicrophone())
			{
				ShowError(AudioText("CustomKrasue_MicrophoneUnavailable", "The microphone is unavailable. Check microphone access and try again."));
				yield break;
			}
			float deadline = Time.realtimeSinceStartup + 2f;
			int position;
			bool recording;
			// Wait for the first samples; some devices take a moment to start.
			while (true)
			{
				if (!TryReadMicrophoneCapture(out position, out recording))
				{
					yield break;
				}
				if (position > 0)
				{
					break;
				}
				if (Time.realtimeSinceStartup >= deadline)
				{
					ShowError(AudioText("CustomKrasue_NoSoundCaptured", "No sound was captured. Check your microphone and try again."));
					yield break;
				}
				yield return null;
			}
			isRecordingInProgress = true;
			timeWhenRecordingStarted = Time.realtimeSinceStartup;
			lastBlink = timeWhenRecordingStarted;
			if (progressBar != null)
			{
				progressBar.SetActive(true);
			}
			RefreshControls();
			int requiredSamples = recordingTime * pendingClip.frequency;
			deadline = Time.realtimeSinceStartup + recordingTime + 1f;
			while (position < requiredSamples && recording && Time.realtimeSinceStartup < deadline)
			{
				yield return null;
				if (!TryReadMicrophoneCapture(out position, out recording))
				{
					yield break;
				}
			}
			StopMicrophone();
			if (position < requiredSamples)
			{
				ShowError(AudioText("CustomKrasue_RecordingInterrupted", "Recording was interrupted. Please record your sound again."));
				yield break;
			}
			// Trim the extra second of the looping buffer.
			float[] samples = new float[pendingClip.channels * requiredSamples];
			if (!pendingClip.GetData(samples, 0))
			{
				ShowError(AudioText("CustomKrasue_RecordingFailed", "The recording could not be prepared. Please try again."));
				yield break;
			}
			AudioClip trimmed = AudioClip.Create(soundVartiationName, requiredSamples, pendingClip.channels, pendingClip.frequency, false);
			bool copied = trimmed.SetData(samples, 0);
			Destroy(pendingClip);
			pendingClip = trimmed;
			if (!copied)
			{
				ShowError(AudioText("CustomKrasue_RecordingFailed", "The recording could not be prepared. Please try again."));
				yield break;
			}
			AcceptClip(pendingClip);
		}
		finally
		{
			StopMicrophone();
			ReleasePendingClip();
			isAudioBusy = false;
			isRecordingInProgress = false;
			isRecordingOperation = false;
			if (IsCurrentOperation(generation))
			{
				if (progressBar != null)
				{
					progressBar.SetActive(false);
				}
				if (recordingInProgress != null)
				{
					recordingInProgress.enabled = false;
				}
				RefreshControls();
			}
		}
	}

	private void StopMicrophone()
	{
		if (ownsMicrophone)
		{
			ownsMicrophone = false;
			Microphone.End(null);
		}
	}

	private void OnApplicationFocus(bool focused)
	{
		isApplicationFocused = focused;
		if (!focused && isRecordingOperation)
		{
			CancelOperations();
		}
	}

	private void OnApplicationPause(bool paused)
	{
		isApplicationPaused = paused;
		if (paused && isRecordingOperation)
		{
			CancelOperations();
		}
	}

	private void ReleasePendingClip()
	{
		if (pendingClip != null && pendingClip != UserGameSettings.customKrasueSound)
		{
			Destroy(pendingClip);
		}
		pendingClip = null;
	}

	private void OnDisable()
	{
		isDisabling = true;
		CancelOperations();
	}

	public void CancelOperations()
	{
		operationGeneration++;
		StopAllCoroutines();
		CleanupActiveImport();
		StopMicrophone();
		isRecordingOperation = false;
		ReleasePendingClip();
		isAudioBusy = false;
		isPermissionRequested = false;
		isPreviewPlaying = false;
		isRecordingInProgress = false;
		if (MasterAudio.SafeInstance != null && MasterAudio.SoundsReady)
		{
			MasterAudio.StopAllOfSound(soundGroupName);
		}
		if (!hasStarted || isDisabling)
		{
			return;
		}
		if (progressBar != null)
		{
			progressBar.SetActive(false);
		}
		if (recordingInProgress != null)
		{
			recordingInProgress.enabled = false;
		}
		RefreshControls();
	}

	private void Update()
	{
		if (isDisabling)
		{
			return;
		}
		if (permissionWasPending != HasPendingMicrophonePermission)
		{
			RefreshControls();
			if (!IsBusy)
			{
				RefreshSavedAudio();
			}
		}
		float now = Time.realtimeSinceStartup;
		if (isRecordingInProgress)
		{
			if (progressBarImage != null)
			{
				progressBarImage.fillAmount = (now - timeWhenRecordingStarted) / recordingTime;
			}
			if (lastBlink + 0.3f < now)
			{
				if (recordingInProgress != null)
				{
					recordingInProgress.enabled = !recordingInProgress.enabled;
				}
				lastBlink = now;
			}
		}
		else if (isPreviewPlaying)
		{
			float length = audioClip != null ? audioClip.length : 0f;
			if (progressBarImage != null)
			{
				progressBarImage.fillAmount = length > 0f ? (now - timeWhenPreviewStarted) / length : 1f;
			}
			if (now >= length + timeWhenPreviewStarted)
			{
				TurnOffPreview();
			}
		}
	}

	public void ToggleSound()
	{
		if (isDisabling || IsBusy || !CustomKrasueMediaStore.HasReadyAudio)
		{
			return;
		}
		if (isPreviewPlaying)
		{
			TurnOffPreview();
			return;
		}
		if (!CustomKrasueAudioBinding.TryRegister())
		{
			ShowError("Your sound could not be prepared. Please select or record it again.");
			return;
		}
		ReleaseRetiredClips();
		MasterAudio.StopAllOfSound("Menu");
		PlaySoundResult result = MasterAudio.PlaySound(soundGroupName);
		if (result == null || !result.SoundPlayed)
		{
			ShowError("Your sound could not be played. Please select or record it again.");
			return;
		}
		audioClip = UserGameSettings.customKrasueSound;
		if (progressBar != null)
		{
			progressBar.SetActive(true);
		}
		timeWhenPreviewStarted = Time.realtimeSinceStartup;
		isPreviewPlaying = true;
		if (play != null)
		{
			play.enabled = false;
		}
		if (stop != null)
		{
			stop.enabled = true;
		}
	}

	private void TurnOffPreview()
	{
		isPreviewPlaying = false;
		if (MasterAudio.SafeInstance != null && MasterAudio.SoundsReady)
		{
			MasterAudio.StopAllOfSound(soundGroupName);
		}
		if (play != null)
		{
			play.enabled = true;
		}
		if (stop != null)
		{
			stop.enabled = false;
		}
		if (progressBar != null)
		{
			progressBar.SetActive(false);
		}
	}

	private void ActivatePreviewButton(bool isActive)
	{
		Color tint = isActive ? Color.white : Color.gray;
		if (play != null)
		{
			play.enabled = true;
			play.color = tint;
		}
		if (stop != null)
		{
			stop.enabled = false;
			stop.color = tint;
		}
		if (playStopButton != null)
		{
			playStopButton.interactable = isActive;
		}
	}

	protected override void OnDestroy()
	{
		ReleaseRetiredClips();
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}

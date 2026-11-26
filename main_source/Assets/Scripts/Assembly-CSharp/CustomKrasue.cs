using EyesAnalytics;
using Firebase.Crashlytics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomKrasue : CustomSingleton<CustomKrasue>
{
	[SerializeField]
	private Camera customKrasuePreview;

	[SerializeField]
	private Material maskMaterial;

	private Texture2D ownedPreviewTexture;

	private bool imageImportInProgress;

	private int imageGeneration;

	private bool faceSettingsDirty;

	private Vector2 oldPos;

	private float _xScale = 1f;

	private float _yScale = 1f;

	private float _xTrans;

	private float _yTrans;

	private void Start()
	{
		CustomKrasueMediaStore.Initialize();
		CustomKrasueMediaStore.GetFaceSettings(out _xTrans, out _yTrans, out _xScale, out _yScale);
		if (!string.IsNullOrEmpty(CustomKrasueSettings.Path))
		{
			Texture2D texture = NativeGallery.LoadImageAtPath(CustomKrasueSettings.Path, CustomKrasueSettings.maxSize, true, true, false);
			if (texture != null)
			{
				SetPreviewTexture(texture);
			}
		}
		updateTextureParams();
		faceSettingsDirty = false;
	}

	private void SetPreviewTexture(Texture2D texture)
	{
		Texture2D previous = ownedPreviewTexture;
		ownedPreviewTexture = texture;
		maskMaterial.mainTexture = texture;
		if (previous != null && previous != texture)
		{
			Destroy(previous);
		}
	}

	private bool SaveFaceSettings(bool showError)
	{
		if (!faceSettingsDirty)
		{
			return true;
		}
		if (!CustomKrasueMediaStore.TrySaveFaceSettings(_xTrans, _yTrans, _xScale, _yScale, out string error))
		{
			if (showError)
			{
				MessageBox.Show(error);
			}
			return false;
		}
		faceSettingsDirty = false;
		return true;
	}

	private void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			SaveFaceSettings(false);
		}
	}

	// One finger drags the face around the mask, two fingers pinch to scale it.
	private void Update()
	{
		if (!customKrasuePreview.enabled)
		{
			return;
		}
		if (Input.touchCount == 1)
		{
			Touch touch = Input.GetTouch(0);
			if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Ended)
			{
				oldPos = touch.position;
			}
			if (touch.phase == TouchPhase.Moved)
			{
				Vector2 delta = touch.position - oldPos;
				_xTrans -= delta.x * Time.deltaTime * 2f * 0.03f;
				_yTrans -= delta.y * Time.deltaTime * 2f * 0.03f;
				oldPos = touch.position;
			}
		}
		else if (Input.touchCount == 2)
		{
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);
			if (touchZero.phase == TouchPhase.Moved && touchOne.phase == TouchPhase.Moved)
			{
				Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
				Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
				float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
				float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
				_xScale += (touchDeltaMag - prevTouchDeltaMag) * Time.deltaTime * -0.2f;
				_yScale = _xScale;
			}
		}
		if (_xTrans > 1f)
		{
			_xTrans -= 1f;
		}
		else if (_xTrans < 0f)
		{
			_xTrans += 1f;
		}
		if (_yTrans > 1f)
		{
			_yTrans -= 1f;
		}
		else if (_yTrans < 0f)
		{
			_yTrans += 1f;
		}
		float aspect = 1f;
		Texture texture = maskMaterial.mainTexture;
		if (texture != null && texture.height != 0)
		{
			aspect = (float)texture.width / texture.height;
		}
		_xScale = Mathf.Clamp(_xScale, 0.2f, 5f);
		_yScale = aspect * _xScale;
		updateTextureParams();
		if (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled))
		{
			SaveFaceSettings(true);
		}
	}

	public void ShowCustomKrasue()
	{
		CustomKrasueAudio customAudio = FindFirstObjectByType<CustomKrasueAudio>();
		if (customAudio != null)
		{
			customAudio.RefreshSavedAudio();
		}
		customKrasuePreview.enabled = true;
		MenuContentWindow.Instance.ShowCustomKrasue();
	}

	public void CloseWindow()
	{
		SaveFaceSettings(true);
		// Invalidates a picker result that arrives after the window was closed.
		imageGeneration++;
		CustomKrasueAudio customAudio = FindFirstObjectByType<CustomKrasueAudio>();
		if (customAudio != null)
		{
			customAudio.CancelOperations();
		}
		customKrasuePreview.enabled = false;
	}

	private void PickImage()
	{
		if (imageImportInProgress)
		{
			return;
		}
		imageImportInProgress = true;
		int generation = ++imageGeneration;
		ReportImageImportStage("picker");
		NativeGallery.GetImageFromGallery(path =>
		{
			if (this == null)
			{
				return;
			}
			Texture2D loaded = null;
			if (generation == imageGeneration && !string.IsNullOrEmpty(path) && gameObject.activeInHierarchy)
			{
				ReportImageImportStage("decode");
				loaded = NativeGallery.LoadImageAtPath(path, CustomKrasueSettings.maxSize, false, true, false);
				if (loaded == null)
				{
					ReportImageImportStage("decode_failed");
					MessageBox.Show("This picture could not be opened. Please select another picture.");
				}
				else
				{
					ReportImageImportStage("save");
					if (!CustomKrasueMediaStore.TrySaveImage(loaded, out string error))
					{
						ReportImageImportStage("save_failed");
						MessageBox.Show(error);
					}
					else
					{
						SetPreviewTexture(loaded);
						loaded = null;
						_yScale = (float)ownedPreviewTexture.width / ownedPreviewTexture.height * _xScale;
						updateTextureParams();
						SaveFaceSettings(true);
						ReportImageImportStage("ready");
					}
				}
			}
			else
			{
				ReportImageImportStage("canceled");
			}
			if (loaded != null)
			{
				Destroy(loaded);
			}
			imageImportInProgress = false;
		}, "Select an image", "image/*");
	}

	private static void ReportImageImportStage(string stage)
	{
		Debug.Log("[CustomKrasue] Photo import: " + stage);
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.SetCustomKey("custom_krasue_photo_stage", stage);
			Crashlytics.Log("CustomKrasue photo: " + stage);
		}
	}

	public void CheckPermissionsAndPickImage()
	{
		PickImage();
	}

	public void ChooseFromGallery()
	{
		PickImage();
	}

	public void Play()
	{
		if (imageImportInProgress)
		{
			MessageBox.Show("Please wait until your picture is ready.");
			return;
		}
		CustomKrasueAudio customAudio = FindFirstObjectByType<CustomKrasueAudio>();
		if (customAudio == null)
		{
			MessageBox.Show("Your monster sound is not ready. Please reopen this screen.");
			return;
		}
		if (!customAudio.CanStartGame(out string error))
		{
			MessageBox.Show(error);
			return;
		}
		if (SaveFaceSettings(true))
		{
			UserGameSettings.expertMode = true;
			Ads.SetNextInterstitialTrigger(AdTrigger.Play);
#if RECOVERY_PC_MENU
			if (PcMenuInput.BlockMissingScene("PreGameAd"))
			{
				return;
			}
#endif
			SceneManager.LoadScene("PreGameAd");
		}
	}

	public void xScale(float param)
	{
		_xScale = param;
		updateTextureParams();
	}

	public void xTrans(float param)
	{
		_xTrans = param;
		updateTextureParams();
	}

	public void yScale(float param)
	{
		_yScale = param;
		updateTextureParams();
	}

	public void yTrans(float param)
	{
		_yTrans = param;
		updateTextureParams();
	}

	private void updateTextureParams()
	{
		Vector2 previousOffset = CustomKrasueSettings.TextureUVConfig.GetOffset();
		Vector2 previousScale = CustomKrasueSettings.TextureUVConfig.GetScale();
		CustomKrasueSettings.TextureUVConfig.Set(_xTrans, _yTrans, _xScale, _yScale);
		Vector2 offset = CustomKrasueSettings.TextureUVConfig.GetOffset();
		Vector2 scale = CustomKrasueSettings.TextureUVConfig.GetScale();
		if (offset != previousOffset || scale != previousScale)
		{
			faceSettingsDirty = true;
		}
		maskMaterial.SetTextureOffset("_MainTex", offset);
		maskMaterial.SetTextureScale("_MainTex", scale);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}

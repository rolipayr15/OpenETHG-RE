using System;
using I2.Loc;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Eyes.DailyChallenges
{
	[RequireComponent(typeof(Canvas))]
	[DisallowMultipleComponent]
	public sealed class DailyChallengeShareCard : MonoBehaviour
	{
		public const int ImageSize = 1080;

		[SerializeField]
		private Text heading;

		[SerializeField]
		private Text challengeTitle;

		[SerializeField]
		private Text objective;

		[SerializeField]
		private Text progress;

		[SerializeField]
		private Text binaryChallengeTitle;

		[SerializeField]
		private Text binaryObjective;

		[SerializeField]
		private Text challengeDate;

		[SerializeField]
		private Text playerName;

		[SerializeField]
		private Text invitation;

		private readonly Dictionary<Text, Font> authoredFonts = new Dictionary<Text, Font>();

		public void Populate(DailyChallengeShareData data, Font localizedFont)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			transform.position = new Vector3(0f, -10000f, 0f);
			SetLocalizedText(heading, data.Heading, localizedFont);
			SetLocalizedText(challengeTitle, data.Title, localizedFont);
			SetLocalizedText(objective, data.Objective, localizedFont);
			progress.text = data.ProgressText;
			SetLocalizedText(binaryChallengeTitle, data.Title, localizedFont);
			SetLocalizedText(binaryObjective, data.Objective, localizedFont);
			progress.gameObject.SetActive(data.ShowNumericProgress);
			challengeTitle.gameObject.SetActive(data.ShowNumericProgress);
			objective.gameObject.SetActive(data.ShowNumericProgress);
			binaryChallengeTitle.gameObject.SetActive(!data.ShowNumericProgress);
			binaryObjective.gameObject.SetActive(!data.ShowNumericProgress);
			SetLocalizedText(challengeDate, data.DateText, localizedFont);
			SetLocalizedText(invitation, data.Invitation, localizedFont);
			playerName.supportRichText = false;
			SetLocalizedText(playerName, data.PlayerName, localizedFont);
			playerName.gameObject.SetActive(!string.IsNullOrWhiteSpace(data.PlayerName));
		}

		private void SetLocalizedText(Text label, string value, Font localizedFont)
		{
			label.text = LocalizationManager.FixRTL_IfNeeded(value ?? string.Empty);
			if (!authoredFonts.TryGetValue(label, out Font authored))
			{
				authored = label.font;
				authoredFonts.Add(label, authored);
			}
			label.font = localizedFont != null ? localizedFont : authored;
			LocalizedFontLayout.Apply(label);
		}

		public Texture2D Render()
		{
			GameObject holder = new GameObject("Daily challenge capture camera", typeof(Camera));
			Camera camera = holder.GetComponent<Camera>();
			RenderTexture target = new RenderTexture(1080, 1080, 24, RenderTextureFormat.ARGB32);
			RenderTexture previous = RenderTexture.active;
			Canvas canvas = GetComponent<Canvas>();
			Camera previousCamera = canvas.worldCamera;
			try
			{
				camera.enabled = false;
				camera.transform.SetPositionAndRotation(transform.position + Vector3.back * 10f, Quaternion.identity);
				camera.orthographic = true;
				camera.orthographicSize = 540f;
				camera.aspect = 1f;
				camera.nearClipPlane = 0.1f;
				camera.farClipPlane = 20f;
				camera.clearFlags = CameraClearFlags.SolidColor;
				camera.backgroundColor = Color.black;
				camera.cullingMask = 1 << gameObject.layer;
				camera.allowHDR = false;
				camera.allowMSAA = false;
				camera.targetTexture = target;
				canvas.worldCamera = camera;
				Canvas.ForceUpdateCanvases();
				camera.Render();
				RenderTexture.active = target;
				Texture2D image = new Texture2D(1080, 1080, TextureFormat.RGB24, false);
				image.ReadPixels(new Rect(0f, 0f, 1080f, 1080f), 0, 0);
				image.Apply(false, false);
				return image;
			}
			finally
			{
				canvas.worldCamera = previousCamera;
				RenderTexture.active = previous;
				camera.targetTexture = null;
				Release(target);
				Release(holder);
			}
		}

		private static void Release(UnityEngine.Object value)
		{
			if (value != null)
			{
				if (Application.isPlaying)
				{
					Destroy(value);
				}
				else
				{
					DestroyImmediate(value);
				}
			}
		}
	}
}

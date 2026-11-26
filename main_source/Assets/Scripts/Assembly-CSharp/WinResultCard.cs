using I2.Loc;
using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public sealed class WinResultCard : MonoBehaviour
{
	[Serializable]
	private struct PortraitLayout
	{
		public Enemy enemy;

		[Min(0.01f)]
		public float scale;

		public Vector2 offset;

		[Range(-180f, 180f)]
		public float rotation;
	}

	public const int ImageSize = 1080;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Image portrait;

	[SerializeField]
	private Image doubleCharliePortrait;

	[FormerlySerializedAs("goodBoyBackdrop")]
	[SerializeField]
	private RectTransform portraitFog;

	[SerializeField]
	private Material witchFogMaterial;

	[SerializeField]
	private Text heading;

	[SerializeField]
	private Text duration;

	[SerializeField]
	private Text durationCaption;

	[SerializeField]
	private Text levelLabel;

	[SerializeField]
	private Text enemyLabel;

	[SerializeField]
	private Text details;

	[SerializeField]
	private Text difficultyLabel;

	[SerializeField]
	private StarsForLevelCompletion difficultyStars;

	[SerializeField]
	private Font metadataFont;

	[SerializeField]
	private Sprite mansion;

	[SerializeField]
	private Sprite hospital;

	[SerializeField]
	private Sprite school;

	[SerializeField]
	private Sprite krasue;

	[SerializeField]
	private Sprite charlie;

	[SerializeField]
	private Sprite goodBoy;

	[SerializeField]
	private Sprite witch;

	[SerializeField]
	private Sprite goose;

	[SerializeField]
	private PortraitLayout[] portraitLayouts;

	private bool hasPortraitBaseline;

	private Vector2 portraitBaseSize;

	private Vector2 portraitBasePosition;

	private Quaternion portraitBaseRotation;

	private Vector2 portraitFogBasePosition;

	private Vector2 portraitFogBaseSize;

	private Graphic portraitFogGraphic;

	private Material portraitFogBaseMaterial;

	private Color portraitFogBaseColor;

	public void Populate(WinResultData result, Font localizedFont)
	{
		transform.position = new Vector3(0f, -10000f, 0f);
		background.sprite = GetBackground(result.Level);
		background.enabled = background.sprite != null;
		if (background.sprite != null)
		{
			Rect area = background.sprite.rect;
			float factor = 1080f / Mathf.Min(area.width, area.height);
			background.rectTransform.sizeDelta = new Vector2(area.width * factor, area.height * factor);
		}
		portrait.sprite = GetPortrait(result.Enemy);
		portrait.enabled = portrait.sprite != null;
		ApplyPortraitLayout(result.Enemy);
		doubleCharliePortrait.sprite = charlie;
		doubleCharliePortrait.gameObject.SetActive((int)result.Enemy == 5 && charlie != null);
		if (portraitFog != null)
		{
			int enemy = (int)result.Enemy;
			portraitFog.gameObject.SetActive((enemy == 2 || enemy == 6) && portrait.enabled);
			portraitFog.anchoredPosition = portraitFogBasePosition + portrait.rectTransform.anchoredPosition - portraitBasePosition;
			float scale = portrait.rectTransform.sizeDelta.x / portraitBaseSize.x;
			portraitFog.sizeDelta = new Vector2(portraitFogBaseSize.x * scale, portraitFogBaseSize.y * scale);
			if (portraitFogGraphic != null)
			{
				bool witchFog = enemy == 6 && witchFogMaterial != null;
				portraitFogGraphic.material = witchFog ? witchFogMaterial : portraitFogBaseMaterial;
				portraitFogGraphic.color = new Color(portraitFogBaseColor.r, portraitFogBaseColor.g, portraitFogBaseColor.b, witchFog ? 1f : portraitFogBaseColor.a);
			}
		}
		if (result.Heading != null)
		{
			heading.text = result.Heading.ToUpperInvariant();
		}
		duration.text = result.DurationText;
		durationCaption.text = result.DurationCaption;
		SetLocalizedText(levelLabel, result.LevelText, localizedFont);
		SetLocalizedText(enemyLabel, result.EnemyText, localizedFont);
		enemyLabel.gameObject.SetActive(!string.IsNullOrEmpty(result.EnemyText));
		SetLocalizedText(details, result.DetailsText, localizedFont);
		details.gameObject.SetActive(!string.IsNullOrEmpty(result.DetailsText));
		SetLocalizedText(difficultyLabel, result.DifficultyText, localizedFont);
		difficultyLabel.gameObject.SetActive(result.ShowDifficultyStars);
		difficultyStars.gameObject.SetActive(result.ShowDifficultyStars);
		if (result.ShowDifficultyStars)
		{
			difficultyStars.SetHighestDifficultyCompleted(result.Difficulty);
		}
	}

	private void SetLocalizedText(Text label, string value, Font localizedFont)
	{
		label.text = LocalizationManager.FixRTL_IfNeeded(value);
		label.font = localizedFont != null ? localizedFont : (metadataFont != null ? metadataFont : label.font);
		LocalizedFontLayout.Apply(label);
	}

	public Texture2D Render()
	{
		GameObject holder = new GameObject("Win result capture camera", typeof(Camera));
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

	private Sprite GetBackground(Level level)
	{
		switch ((int)level)
		{
		case 0:
			return mansion;
		case 1:
			return hospital;
		case 3:
			return school;
		default:
			return null;
		}
	}

	private Sprite GetPortrait(Enemy enemy)
	{
		switch ((int)enemy)
		{
		case 0:
		case 3:
		case 5:
			return krasue;
		case 1:
			return charlie;
		case 2:
			return goodBoy;
		case 6:
			return witch;
		case 7:
			return goose != null ? goose : krasue;
		default:
			return null;
		}
	}

	private void ApplyPortraitLayout(Enemy enemy)
	{
		RectTransform rect = portrait.rectTransform;
		if (!hasPortraitBaseline)
		{
			portraitBaseSize = rect.sizeDelta;
			portraitBasePosition = rect.anchoredPosition;
			portraitBaseRotation = rect.localRotation;
			if (portraitFog != null)
			{
				portraitFogBasePosition = portraitFog.anchoredPosition;
				portraitFogBaseSize = portraitFog.sizeDelta;
				portraitFogGraphic = portraitFog.GetComponent<Graphic>();
				if (portraitFogGraphic != null)
				{
					portraitFogBaseMaterial = portraitFogGraphic.material;
					portraitFogBaseColor = portraitFogGraphic.color;
				}
			}
			hasPortraitBaseline = true;
		}
		rect.sizeDelta = portraitBaseSize;
		rect.anchoredPosition = portraitBasePosition;
		rect.localRotation = portraitBaseRotation;
		if (portraitLayouts == null)
		{
			return;
		}
		// The "your own ghost" enemy shares the Krasue portrait layout.
		Enemy key = (int)enemy == 3 ? (Enemy)0 : enemy;
		foreach (PortraitLayout layout in portraitLayouts)
		{
			if (layout.enemy == key)
			{
				float scale = Mathf.Max(0.01f, layout.scale);
				rect.sizeDelta = portraitBaseSize * scale;
				rect.anchoredPosition = portraitBasePosition + layout.offset;
				rect.localRotation = portraitBaseRotation * Quaternion.Euler(0f, 0f, layout.rotation);
			}
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

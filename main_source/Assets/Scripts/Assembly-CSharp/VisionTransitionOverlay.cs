using System;
using UnityEngine;
using UnityEngine.UI;

public class VisionTransitionOverlay : MonoBehaviour
{
	private const float LidHeight = 0.75f;

	private const float LidSoftEdge = 0.3f;

	private const int SortingOrder = 500;

	private static readonly Color LidColor = new Color(0.05f, 0f, 0f, 1f);

	private static readonly Color TintColor = new Color(0.45f, 0f, 0f, 0.55f);

	private Canvas canvas;

	private RectTransform topLid;

	private RectTransform bottomLid;

	private Image tint;

	private float level;

	private float target;

	private float speed;

	public bool IsCovered => level >= 1f;

	public static VisionTransitionOverlay Create()
	{
		GameObject holder = new GameObject("VisionTransition", new Type[] { typeof(RectTransform) });
		VisionTransitionOverlay overlay = holder.AddComponent<VisionTransitionOverlay>();
		overlay.Build();
		return overlay;
	}

	public void Cover(float duration)
	{
		MoveTo(1f, duration);
	}

	public void Reveal(float duration)
	{
		MoveTo(0f, duration);
	}

	public void Hide()
	{
		level = 0f;
		Apply();
	}

	private void MoveTo(float value, float duration)
	{
		target = value;
		speed = duration <= 0f ? float.PositiveInfinity : 1f / duration;
		if (speed == float.PositiveInfinity)
		{
			level = value;
		}
		Apply();
	}

	private void Update()
	{
		if (level != target)
		{
			level = Mathf.MoveTowards(level, target, speed * Time.unscaledDeltaTime);
			Apply();
		}
	}

	private void Apply()
	{
		canvas.enabled = level > 0f;
		if (!canvas.enabled)
		{
			return;
		}
		float eased = level * level * (3f - (level + level));
		float lid = Mathf.Clamp01(eased) * LidHeight;
		topLid.anchorMin = new Vector2(0f, 1f - lid);
		topLid.anchorMax = new Vector2(1f, 1f - lid + LidHeight);
		bottomLid.anchorMin = new Vector2(0f, lid + 0f - LidHeight);
		bottomLid.anchorMax = new Vector2(1f, lid + 0f);
		tint.color = new Color(TintColor.r, TintColor.g, TintColor.b, eased * TintColor.a);
	}

	private void Build()
	{
		canvas = gameObject.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = SortingOrder;
		tint = CreateImage("Tint", null);
		Sprite lidSprite = CreateLidSprite();
		topLid = CreateImage("TopLid", lidSprite).rectTransform;
		bottomLid = CreateImage("BottomLid", lidSprite).rectTransform;
		bottomLid.localRotation = Quaternion.Euler(0f, 0f, 180f);
		Apply();
	}

	private Image CreateImage(string imageName, Sprite sprite)
	{
		GameObject holder = new GameObject(imageName, new Type[] { typeof(RectTransform) });
		RectTransform rect = (RectTransform)holder.transform;
		rect.SetParent(transform, false);
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
		Image image = holder.AddComponent<Image>();
		image.raycastTarget = false;
		image.sprite = sprite;
		image.color = sprite != null ? LidColor : default(Color);
		return image;
	}

	private static Sprite CreateLidSprite()
	{
		Texture2D texture = new Texture2D(1, 64, TextureFormat.RGBA32, false);
		texture.wrapMode = TextureWrapMode.Clamp;
		for (int y = 0; y < 64; y++)
		{
			float fade = Mathf.Clamp01(y / 18.900002f);
			texture.SetPixel(0, y, new Color(1f, 1f, 1f, fade * fade * (3f - (fade + fade))));
		}
		texture.Apply(false, true);
		return Sprite.Create(texture, new Rect(0f, 0f, 1f, 64f), new Vector2(0.5f, 0.5f));
	}

	private void OnDestroy()
	{
		Sprite sprite = topLid != null ? topLid.GetComponent<Image>().sprite : null;
		if (sprite != null)
		{
			Destroy(sprite.texture);
			Destroy(sprite);
		}
	}
}

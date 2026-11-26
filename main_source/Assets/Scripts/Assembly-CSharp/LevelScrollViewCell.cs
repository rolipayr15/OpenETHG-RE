using System.Collections.Generic;
using EyesAnalytics;
using I2.Loc;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class LevelScrollViewCell : FancyScrollViewCell<LevelCellData>
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Image image;

	[SerializeField]
	private string elemName;

	[SerializeField]
	private Text label;

	[SerializeField]
	private GameObject padlock;

	[SerializeField]
	private ContentType contentType;

	[SerializeField]
	private Level level;

	[SerializeField]
	private Enemy enemy;

	[SerializeField]
	private Mode mode;

	private readonly int scrollTriggerHash = Animator.StringToHash("scroll");

	private LevelCellData currentData;

	private Dictionary<string, StoreItems> elemNameToStoreItems = new Dictionary<string, StoreItems>
	{
		{ "endless", StoreItems.ENDLESS },
		{ "trickortreat", StoreItems.TRICK_OR_TREAT },
		{ "PIXEL", StoreItems.PIXEL },
		{ "charlie", StoreItems.CHARLIE },
		{ "dog", StoreItems.GOOD_BOY },
		{ "doubleenemy", StoreItems.DOUBLE_TROUBLE },
		{ "witch", StoreItems.WITCH },
		{ "HOSPITAL", StoreItems.HOSPITAL },
		{ "school", StoreItems.SCHOOL }
	};

	private void OnEnable()
	{
		MessageKit<string>.addObserver(34, UnlockProduct);
		RefreshPadlock();
	}

	private void OnDisable()
	{
		MessageKit<string>.removeObserver(34, UnlockProduct);
	}

	private void Start()
	{
		RectTransform rectTransform = transform as RectTransform;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchoredPosition3D = Vector3.zero;
		UpdatePosition(0f);
	}

	public override void UpdateContent(LevelCellData itemData)
	{
		currentData = itemData;
		if (itemData.sprite != null)
		{
			image.sprite = itemData.sprite;
		}
		elemName = itemData.elemName;
		contentType = itemData.contentType;
		string text = null;
		switch (contentType)
		{
		case ContentType.Level:
			switch (itemData.level)
			{
			case Level.School:
				text = ScriptLocalization.SchoolLevelName;
				break;
			case Level.Hospital:
				text = ScriptLocalization.Hospital;
				break;
			case Level.Mansion:
				text = ScriptLocalization.Mansion;
				break;
			}
			break;
		case ContentType.Enemy:
			switch (elemName)
			{
			case "CUSTOM_KRASUE":
				text = ScriptLocalization.KrasueYourself;
				break;
			case "witch":
				text = ScriptLocalization.witch;
				break;
			case "SANDBOX":
				text = ScriptLocalization.SandboxMode;
				break;
			case "doubleenemy":
				text = ScriptLocalization.DoubleEnemyModeName;
				break;
			case "goose":
				text = LocalizationManager.GetTranslation("Goose");
				break;
			case "charlie":
				text = ScriptLocalization.Charlie;
				break;
			case "dog":
				text = ScriptLocalization.GoodBoy;
				break;
			case "KRASUE":
				text = ScriptLocalization.Krasue;
				break;
			}
			break;
		case ContentType.Mode:
			if (label != null)
			{
				switch (elemName)
				{
				case "MODE_STANDARD":
					text = ScriptLocalization.Standard;
					break;
				case "endless":
					text = ScriptLocalization.Endless;
					break;
				case "trickortreat":
					text = ScriptLocalization.trickOrTreat;
					break;
				case "PIXEL":
					text = ScriptLocalization.PixelMode;
					break;
				}
			}
			break;
		}
		if (text != null)
		{
			label.text = text;
		}
		RefreshPadlock();
	}

	public void OnPadlockClick()
	{
		LevelScrollViewCell cell = gameObject.GetComponentInParent<LevelScrollViewCell>();
		if (cell != null)
		{
			MenuPurchase.Show(elemNameToStoreItems[cell.elemName], CartType.ExpertShop);
		}
	}

	public override void UpdatePosition(float position)
	{
		animator.Play(scrollTriggerHash, -1, position);
		animator.speed = 0f;
	}

	private void UnlockProduct(string productId)
	{
		RefreshPadlock();
	}

	private void RefreshPadlock()
	{
		if (currentData == null || padlock == null)
		{
			return;
		}
		bool unlocked;
		switch (currentData.contentType)
		{
		case ContentType.Level:
			unlocked = ChooseLevelManager.Instance.IsLevelUnlocked(currentData.level);
			break;
		case ContentType.Enemy:
			unlocked = ChooseLevelManager.Instance.IsEnemyUnlocked(currentData.enemy);
			break;
		case ContentType.Mode:
			unlocked = ChooseLevelManager.Instance.IsModeUnlocked(currentData.mode);
			break;
		default:
			return;
		}
		padlock.SetActive(!unlocked);
	}
}

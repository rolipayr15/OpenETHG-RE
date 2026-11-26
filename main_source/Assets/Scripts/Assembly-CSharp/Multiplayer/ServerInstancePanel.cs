using I2.Loc;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Recovery addition: the switch between the official multiplayer and a custom server, inside the menu settings.
public class ServerInstancePanel : MonoBehaviour
{
	private Font font;

	private Sprite buttonSprite;

	private GameObject window;

	private InputField hostInput;

	private InputField portInput;

	private InputField nameInput;

	private Text status;

	private Image officialImage;

	private Image customImage;

	private GameObject customFields;

	private bool customSelected;

	private static bool Ru => LocalizationManager.CurrentLanguageCode == "ru";

	private static string T(string ru, string en)
	{
		return Ru ? ru : en;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Hook()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name != "Menu")
		{
			return;
		}
		foreach (GameObject root in scene.GetRootGameObjects())
		{
			foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
			{
				if (canvas.name == "CanvasSettings")
				{
					Transform settings = canvas.transform.Find("Settings");
					(settings != null ? settings : canvas.transform).gameObject.AddComponent<ServerInstancePanel>();
					return;
				}
			}
		}
	}

	private void Start()
	{
		Button styled = GetComponentInChildren<Button>(true);
		buttonSprite = styled != null && styled.image != null ? styled.image.sprite : null;
		Text sample = GetComponentInChildren<Text>(true);
		font = sample != null ? sample.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		Button entry = MakeButton(transform, T("СЕРВЕР", "SERVER"), new Vector2(1f, 0f), new Vector2(-40f, 118f), new Vector2(240f, 64f), OpenWindow);
		entry.name = "ServerInstanceButton";
		BuildWindow();
		window.SetActive(false);
		StartCoroutine(ServerNews.Fetch());
		StartCoroutine(OnlineConfig.AutoSignIn(() => this != null));
	}

	private void BuildWindow()
	{
		window = new GameObject("ServerInstanceWindow", typeof(RectTransform), typeof(Image), typeof(Canvas), typeof(GraphicRaycaster));
		window.transform.SetParent(transform, false);
		Stretch((RectTransform)window.transform);
		window.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.96f);
		Canvas canvas = window.GetComponent<Canvas>();
		canvas.overrideSorting = true;
		canvas.sortingOrder = 500;
		Transform t = window.transform;
		MakeText(t, T("СЕРВЕР МУЛЬТИПЛЕЕРА", "MULTIPLAYER SERVER"), new Vector2(0f, 230f), new Vector2(900f, 60f), 42);
		officialImage = MakeButton(t, T("ОФИЦИАЛЬНЫЙ", "OFFICIAL"), new Vector2(0.5f, 0.5f), new Vector2(-190f, 130f), new Vector2(340f, 64f), delegate
		{
			SetSelection(false);
		}).GetComponent<Image>();
		customImage = MakeButton(t, T("СВОЙ СЕРВЕР", "CUSTOM SERVER"), new Vector2(0.5f, 0.5f), new Vector2(190f, 130f), new Vector2(340f, 64f), delegate
		{
			SetSelection(true);
		}).GetComponent<Image>();
		customFields = new GameObject("CustomFields", typeof(RectTransform));
		customFields.transform.SetParent(t, false);
		Stretch((RectTransform)customFields.transform);
		MakeText(customFields.transform, T("IP или адрес", "IP or host"), new Vector2(-190f, 50f), new Vector2(340f, 40f), 26);
		MakeText(customFields.transform, T("Порт", "Port"), new Vector2(190f, 50f), new Vector2(340f, 40f), 26);
		hostInput = MakeInput(customFields.transform, new Vector2(-190f, 0f), InputField.ContentType.Standard);
		portInput = MakeInput(customFields.transform, new Vector2(190f, 0f), InputField.ContentType.IntegerNumber);
		MakeText(customFields.transform, T("Имя игрока", "Player name"), new Vector2(0f, -50f), new Vector2(340f, 36f), 26);
		nameInput = MakeInput(customFields.transform, new Vector2(0f, -94f), InputField.ContentType.Standard);
		nameInput.characterLimit = 15;
		status = MakeText(t, "", new Vector2(0f, -145f), new Vector2(900f, 40f), 26);
		MakeButton(t, T("СОХРАНИТЬ", "SAVE"), new Vector2(0.5f, 0.5f), new Vector2(190f, -210f), new Vector2(340f, 64f), Save);
		MakeButton(t, T("НАЗАД", "BACK"), new Vector2(0.5f, 0.5f), new Vector2(-190f, -210f), new Vector2(340f, 64f), delegate
		{
			window.SetActive(false);
		});
	}

	private void OpenWindow()
	{
		hostInput.text = OnlineConfig.CustomHost;
		portInput.text = OnlineConfig.CustomPort.ToString();
		nameInput.text = UserGameSettings.multiplayerUsername;
		status.text = "";
		SetSelection(OnlineConfig.IsCustomServer);
		window.SetActive(true);
	}

	private void SetSelection(bool custom)
	{
		customSelected = custom;
		Color idle = buttonSprite != null ? new Color(0.45f, 0.45f, 0.45f, 1f) : new Color(0.15f, 0.15f, 0.15f, 1f);
		officialImage.color = custom ? idle : new Color(1f, 0.82f, 0.3f, 1f);
		customImage.color = custom ? new Color(1f, 0.82f, 0.3f, 1f) : idle;
		customFields.SetActive(custom);
	}

	private void Save()
	{
		int.TryParse(portInput.text, out int port);
		string playerName = nameInput.text.Trim();
		if (customSelected && (playerName.Length < 3 || playerName.Length > 15))
		{
			status.text = T("Имя: от 3 до 15 символов", "Name: 3 to 15 characters");
			return;
		}
		if (customSelected)
		{
		}
		bool saved = OnlineConfig.Select(customSelected, hostInput.text, port);
		if (saved && customSelected)
		{
			OnlineConfig.SavePlayerName(playerName);
			Prime31.MessageKit.MessageKit.post(128);
		}
		if (saved)
		{
			window.SetActive(false);
		}
		else
		{
			status.text = T("Проверьте адрес и порт (1-65535)", "Check the address and port (1-65535)");
		}
	}

	private static void Stretch(RectTransform rect)
	{
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
	}

	private Text MakeText(Transform parent, string value, Vector2 position, Vector2 size, int fontSize)
	{
		GameObject go = new GameObject("Text", typeof(RectTransform), typeof(Text));
		go.transform.SetParent(parent, false);
		RectTransform rect = (RectTransform)go.transform;
		rect.anchoredPosition = position;
		rect.sizeDelta = size;
		Text text = go.GetComponent<Text>();
		text.font = font;
		text.fontSize = fontSize;
		text.alignment = TextAnchor.MiddleCenter;
		text.color = Color.white;
		text.text = value;
		text.raycastTarget = false;
		return text;
	}

	private Button MakeButton(Transform parent, string label, Vector2 anchor, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClick)
	{
		GameObject go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
		go.transform.SetParent(parent, false);
		RectTransform rect = (RectTransform)go.transform;
		rect.anchorMin = anchor;
		rect.anchorMax = anchor;
		rect.pivot = anchor == new Vector2(0.5f, 0.5f) ? new Vector2(0.5f, 0.5f) : anchor;
		rect.anchoredPosition = position;
		rect.sizeDelta = size;
		Image image = go.GetComponent<Image>();
		image.sprite = buttonSprite;
		image.type = Image.Type.Sliced;
		image.color = buttonSprite != null ? Color.white : new Color(0.15f, 0.15f, 0.15f, 1f);
		Button button = go.GetComponent<Button>();
		button.onClick.AddListener(onClick);
		Text text = MakeText(go.transform, label, Vector2.zero, size, 30);
		Stretch(text.rectTransform);
		return button;
	}

	private InputField MakeInput(Transform parent, Vector2 position, InputField.ContentType type)
	{
		GameObject go = new GameObject("Input", typeof(RectTransform), typeof(Image), typeof(InputField));
		go.transform.SetParent(parent, false);
		RectTransform rect = (RectTransform)go.transform;
		rect.anchoredPosition = position;
		rect.sizeDelta = new Vector2(340f, 60f);
		go.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 1f);
		Text text = MakeText(go.transform, "", Vector2.zero, new Vector2(320f, 56f), 30);
		text.supportRichText = false;
		InputField input = go.GetComponent<InputField>();
		input.textComponent = text;
		input.contentType = type;
		input.characterLimit = type == InputField.ContentType.IntegerNumber ? 5 : 80;
		return input;
	}
}

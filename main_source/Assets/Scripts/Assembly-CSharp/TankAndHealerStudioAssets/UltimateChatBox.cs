using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TankAndHealerStudioAssets
{
	[RequireComponent(typeof(CanvasGroup))]
	public class UltimateChatBox : MonoBehaviour
	{
		[Serializable]
		public class ChatInformation
		{
			public UltimateChatBox chatBox;

			public TextMeshProUGUI chatText;

			public Rect usernameRect;

			public float contentSpace;

			public Vector2 anchoredPosition;

			public float usernameWidth;

			public float lineHeight;

			public int lineCount;

			public ChatStyle chatBoxStyle;

			public bool IsVisible { get; private set; }

			public string Username { get; set; }

			public string DisplayUsername { get; set; }

			public string Message { get; set; }

			public string DisplayMessage { get; set; }

			public void UpdateText()
			{
			}

			public void UpdateVisibility()
			{
			}

			public void RemoveChat()
			{
			}
		}

		[Serializable]
		public class ChatStyle
		{
			public bool usernameBold;

			public bool usernameItalic;

			public bool usernameUnderlined;

			public Color usernameColor;

			public bool disableInteraction;

			public bool noUsernameFollowupText;

			public bool messageBold;

			public bool messageItalic;

			public bool messageUnderlined;

			public Color messageColor;

			public string FormatMessage(string username, string message)
			{
				return null;
			}

			public string FormatUsernameOnly(string username)
			{
				return null;
			}
		}

		private List<TextMeshProUGUI> UnusedTextPool;

		private RectTransform parentCanvasRectTrans;

		private Vector3 parentCanvasScale;

		private Vector2 parentCanvasSize;

		private float totalContentSpace;

		private bool isDraggingScrollHandle;

		private Vector3 previousInputPosition;

		private float scrollValue;

		private int inputFieldStringPosition;

		private bool isDirty;

		[SerializeField]
		[HideInInspector]
		private CanvasGroup chatBoxCanvasGroup;

		[SerializeField]
		[Tooltip("Should the chat box calculate any touch input on it?")]
		private bool allowTouchInput;

		private bool isDraggingWithTouch;

		private int currentTouchId;

		private bool customInputRecieved;

		private Vector2 customScreenPosition;

		private bool customGetButtonDown;

		private bool customGetButton;

		private RectTransform baseTransform;

		[SerializeField]
		[Tooltip("The ratio of the chat box.")]
		private Vector2 chatBoxSizeRatio;

		[SerializeField]
		[Tooltip("The overall size of the chat box.")]
		private float chatBoxSize;

		[SerializeField]
		[Tooltip("The position of the chat box on the screen. These values are calculated as percentages, so they are divided by 100 and calculated off the canvas size so that it will be consistent across all screen sizes.")]
		private Vector2 chatBoxPosition;

		[SerializeField]
		[Tooltip("The visible bounding box for the chat in the chat box.")]
		private RectTransform visibleChatBoundingBox;

		[SerializeField]
		[Tooltip("The horizontal bounds for the content of the chat box.")]
		private RectTransform chatContentBox;

		[SerializeField]
		[Tooltip("The horizontal spacing for the left and right of the content box.")]
		[Range(0f, 50f)]
		private float horizontalSpacing;

		[SerializeField]
		[Tooltip("The vertical spacing for the top and bottom of the bounding box.")]
		[Range(0f, 50f)]
		private float verticalSpacing;

		[SerializeField]
		[Tooltip("The position of the content within the chat box.")]
		private Vector2 contentPosition;

		[SerializeField]
		[HideInInspector]
		private Rect chatBoxScreenRect;

		private Rect chatBoxVisiblityRect;

		private bool chatBoxPositionCustom;

		private Vector2 bottomContentPosition;

		[SerializeField]
		[Tooltip("The TextMeshPro GameObject to use as base settings for all the chats in the chat box.")]
		private TextMeshProUGUI textObject;

		[SerializeField]
		[Tooltip("The color of the text in the chat box.")]
		private Color textColor;

		[SerializeField]
		[Tooltip("The maximum number of chats in the chat box before the old ones will be filtered out.")]
		private int maxTextInChatBox;

		[SerializeField]
		[Tooltip("Determines if the players should be able to use custom rich text in the input field or if the chat box should filter out the rich text.")]
		private bool disableRichTextFromPlayers;

		[SerializeField]
		[Tooltip("The string to add at the end of a username.")]
		private string usernameFollowup;

		[SerializeField]
		[Tooltip("The space between each chat registered to the chat box.")]
		[Range(0f, 1f)]
		private float spaceBetweenChats;

		[SerializeField]
		[Range(0f, 0.2f)]
		[Tooltip("The relative size of the font to the chat box height.")]
		private float smartFontSize;

		[SerializeField]
		[Tooltip("Should the usernames of the chats registered by interactable?")]
		private bool useInteractableUsername;

		[SerializeField]
		[Tooltip("The image used in association with highlighting the username.")]
		private Image interactableUsernameImage;

		[SerializeField]
		[Tooltip("The color of the highlight image when the input is hovering over the username.")]
		private Color interactableUsernameColor;

		[SerializeField]
		[Tooltip("The percentage of the line height to add as a modifier to the username highlight image.")]
		[Range(0f, 1f)]
		private float interactableUsernameWidthModifier;

		[SerializeField]
		[Tooltip("Should the chat box fade the alpha of the CanvasGroup when enabling/disabling the chat box?")]
		private bool fadeWhenDisabled;

		[SerializeField]
		[Tooltip("The speed for the chat box to fade in.")]
		private float fadeInSpeed;

		[SerializeField]
		[Tooltip("The speed for the chat box to fade out.")]
		private float fadeOutSpeed;

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("The alpha to apply to the chat box when it is disabled.")]
		private float toggledAlpha;

		private bool fadeIn;

		private bool fadeOut;

		private float fadeLerpValue;

		[SerializeField]
		[Tooltip("Should the chat box collapse when enabling/disabling the chat box?")]
		private bool collapseWhenDisabled;

		[SerializeField]
		[Tooltip("The speed for the chat box to expand.")]
		private float expandSpeed;

		[SerializeField]
		[Tooltip("The speed for the chat box to collapse.")]
		private float collapseSpeed;

		[SerializeField]
		[Tooltip("How many lines of chat should be visible when the chat box is in a collapsed state?")]
		private int visibleLineCount;

		private Vector2 baseTransformSize;

		private Vector2 baseTransformCollapsedSize;

		private Vector2 visibleBoundingBoxSize;

		private Vector2 boundingBoxCollapsedSize;

		private bool expandChatBox;

		private bool collapseChatBox;

		private float collapseLerpValue;

		[SerializeField]
		[Tooltip("Should emojis be allowed in chat?")]
		private bool useTextEmoji;

		[SerializeField]
		[Tooltip("The emoji asset to assign to all the chats.")]
		private TMP_SpriteAsset emojiAsset;

		[SerializeField]
		[Tooltip("Should the chat box display a vertical scrollbar to help navigate chat?")]
		private bool useScrollbar;

		[SerializeField]
		private RectTransform scrollbarBase;

		[SerializeField]
		private RectTransform scrollbarHandle;

		[SerializeField]
		private Image scrollbarHandleImage;

		[SerializeField]
		[Tooltip("The default color for the scrollbar handle.")]
		private Color scrollbarHandleNormalColor;

		[SerializeField]
		[Tooltip("The color for when the input is hovering the scrollbar handle.")]
		private Color scrollbarHandleHoverColor;

		[SerializeField]
		[Tooltip("The color of the scrollbar handle when the input is active.")]
		private Color scrollbarHandleActiveColor;

		[SerializeField]
		[Tooltip("Should the player be able to use the scroll wheel on their mouse to navigate the chat box?")]
		private bool useScrollWheel;

		[SerializeField]
		[Tooltip("The speed that the chat box will navigate using the scroll wheel.")]
		private float mouseScrollSpeed;

		[SerializeField]
		[Tooltip("The minimum size that the scrollbar handle will get when the chat box is filled with chats.")]
		[Range(0.01f, 0.25f)]
		private float scrollbarMinimumSize;

		[SerializeField]
		[Tooltip("The width of the scrollbar relative to the chat box width.")]
		[Range(0.01f, 0.25f)]
		private float scrollbarWidth;

		[SerializeField]
		[Tooltip("The horizontal position of the scrollbar relative to the chat box center.")]
		private float scrollbarHorizontalPosition;

		[SerializeField]
		[Tooltip("Should the scrollbar only be visible when hovering input over the chat box?")]
		private bool visibleOnlyOnHover;

		[SerializeField]
		[Tooltip("The speed to apply for the scrollbar to toggle visually.")]
		private float scrollbarToggleSpeed;

		[SerializeField]
		[Tooltip("The time in seconds that the input has NOT been in the chat box for the scrollbar to disable itself.")]
		private float scrollbarInactiveTime;

		[SerializeField]
		[Tooltip("Should clicking on the base of the scrollbar to navigate be disabled?")]
		private bool disableBaseNavigation;

		[SerializeField]
		[HideInInspector]
		private Rect scrollbarBaseRect;

		[SerializeField]
		[HideInInspector]
		private Rect scrollbarHandleRect;

		[SerializeField]
		[HideInInspector]
		private CanvasGroup scrollbarCanvasGroup;

		private Vector2 scrollbarHandleInputStart;

		private Vector2 scrollbarBottomPosition;

		private float scrollbarToggleLerpValue;

		private float _inactiveTime;

		private bool scrollbarToggle;

		private bool scrollbarHandleHovered;

		private bool scrollbarHandleActive;

		[SerializeField]
		[Tooltip("Determines if you want to have navigation arrows at the top and bottom of the scrollbar to help navigate the chat box.")]
		private bool useNavigationArrows;

		[SerializeField]
		[Tooltip("The image component to use for the up arrow.")]
		private Image navigationArrowUp;

		[SerializeField]
		[Tooltip("The image component to use for the down arrow.")]
		private Image navigationArrowDown;

		[SerializeField]
		[Tooltip("The default color for the navigation arrows to be.")]
		private Color navigationNormalColor;

		[SerializeField]
		[Tooltip("The color to apply when the navigation arrows are hovered.")]
		private Color navigationHoverColor;

		[SerializeField]
		[Tooltip("The color to apply when the input is active on the navigation arrows.")]
		private Color navigationActiveColor;

		[SerializeField]
		[Tooltip("The time in seconds before the navigation arrows will repeat the navigation when the input is held.")]
		private float navigationInitialHoldDelay;

		[SerializeField]
		[Tooltip("The time in seconds between repeating navigation from holding the input on the navigation arrows.")]
		private float navigationIntervalDelay;

		[SerializeField]
		[HideInInspector]
		private Rect scrollbarNavigationArrowUpRect;

		[SerializeField]
		[HideInInspector]
		private Rect scrollbarNavigationArrowDownRect;

		private bool scrollbarNavigationArrowUpHovered;

		private bool scrollbarNavigationArrowUpActivated;

		private bool scrollbarNavigationArrowDownHovered;

		private bool scrollbarNavigationArrowDownActivated;

		private float navigationIntervalTime;

		private float navigationInitialHoldTime;

		[SerializeField]
		[Tooltip("Should an input field be available for players to use?")]
		private bool useInputField;

		[SerializeField]
		[Tooltip("The input field component to use for the chat box.")]
		private TMP_InputField inputField;

		[SerializeField]
		[Tooltip("The size of the input field relative to the chat box.")]
		private Vector2 inputFieldSize;

		[SerializeField]
		[Tooltip("The position of the input field relative to the chat box.")]
		private Vector2 inputFieldPosition;

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("The relative size of the font to the input field height.")]
		private float inputFieldSmartFontSize;

		[SerializeField]
		[Tooltip("The size of the input field text area relative to the input field transform.")]
		private Vector2 inputFieldTextAreaSize;

		[SerializeField]
		[Range(-50f, 50f)]
		private float inputFieldTextHorizontalPosition;

		[SerializeField]
		[HideInInspector]
		private RectTransform inputFieldTransform;

		[SerializeField]
		[HideInInspector]
		private Rect inputFieldRect;

		private string inputFieldValue;

		[SerializeField]
		[Tooltip("Should an extra image be used inside the input field?")]
		private bool useExtraImage;

		[SerializeField]
		[Tooltip("The image component to use as the extra image.")]
		private Image extraImage;

		[SerializeField]
		[HideInInspector]
		private Rect extraImageRect;

		[SerializeField]
		[Range(0f, 100f)]
		[Tooltip("The height of the input field extra image.")]
		private float extraImageHeight;

		[SerializeField]
		[Range(0f, 100f)]
		[Tooltip("The width of the input field extra image.")]
		private float extraImageWidth;

		[SerializeField]
		[Range(-50f, 50f)]
		[Tooltip("The horizontal position in relation to the input field.")]
		private float extraImageHorizontalPosition;

		[SerializeField]
		[Tooltip("Should the players be able to add emojis through a provided window?")]
		private bool useEmojiWindow;

		[SerializeField]
		[Tooltip("The image component to be used as a button to open the emoji window.")]
		private Image emojiButtonImage;

		[SerializeField]
		[Tooltip("The size of the button.")]
		private float emojiButtonSize;

		[SerializeField]
		[Range(0f, 150f)]
		[Tooltip("The horizontal position of the emoji button in relation to the left side of the input field.")]
		private float emojiButtonHorizontalPosition;

		[SerializeField]
		[Tooltip("The image component used as the background of the emoji window.")]
		private Image emojiWindowImage;

		[SerializeField]
		[Tooltip("The overall size of the emoji window.")]
		private Vector2 emojiWindowSize;

		[SerializeField]
		[Tooltip("The position of the emoji window in relation to the bottom right of the input field.")]
		private Vector2 emojiWindowPosition;

		[SerializeField]
		[Tooltip("The text to display all the available emojis.")]
		private TextMeshProUGUI emojiText;

		[SerializeField]
		[Range(0f, 0.2f)]
		[Tooltip("The padding to add to the edges of the emoji window text.")]
		private float emojiTextEdgePadding;

		[SerializeField]
		[Tooltip("How many emojis should be displayed in a row inside the window?")]
		private int emojiPerRow;

		[SerializeField]
		[HideInInspector]
		private List<Rect> emojiRects;

		[SerializeField]
		[HideInInspector]
		private Rect emojiButtonRect;

		[SerializeField]
		[HideInInspector]
		private Rect emojiWindowRect;

		[SerializeField]
		[HideInInspector]
		private CanvasGroup emojiWindowCanvasGroup;

		[CompilerGenerated]
		private Action<ChatInformation> m_OnChatRegistered;

		[CompilerGenerated]
		private Action<Vector2, ChatInformation> m_OnUsernameHover;

		[CompilerGenerated]
		private Action<Vector2, ChatInformation> m_OnUsernameInteract;

		[CompilerGenerated]
		private Action m_OnInputFieldEnabled;

		[CompilerGenerated]
		private Action m_OnInputFieldDisabled;

		[CompilerGenerated]
		private Action<string> m_OnInputFieldUpdated;

		[CompilerGenerated]
		private Action<string> m_OnInputFieldSubmitted;

		[CompilerGenerated]
		private Action<string, string> m_OnInputFieldCommandUpdated;

		[CompilerGenerated]
		private Action<string, string> m_OnInputFieldCommandSubmitted;

		[CompilerGenerated]
		private Action m_OnExtraImageInteract;

		public List<ChatInformation> ChatInformations { get; private set; }

		public List<TextMeshProUGUI> AllTextObjects { get; private set; }

		public Canvas ParentCanvas { get; private set; }

		public bool Interactable { get; set; }

		public bool IsEnabled { get; private set; }

		public bool InputOnChatBox { get; private set; }

		public float LineHeight { get; private set; }

		public Vector3 InputPosition { get; private set; }

		public bool GetButtonDown { get; private set; }

		public bool GetButton { get; private set; }

		public bool UseExternalScrollInput { get; set; }

		public float ScrollValue
		{
			set
			{
			}
		}

		public Vector2 TotalChatBoxSize { get; private set; }

		public RectTransform BaseTransform => null;

		public RectTransform VisibleChatBoundingBox => null;

		public RectTransform ChatContentBox => null;

		public TextMeshProUGUI TextObject => null;

		public Color TextColor
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		public float CalculatedFontSize => 0f;

		public Image InteractableUsernameImage => null;

		public Color InteractableUsernameColor
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		public bool UsernameHighlighted { get; private set; }

		public int CurrentHoveredChatIndex { get; private set; }

		public bool LeaveTextVisible
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public TMP_SpriteAsset EmojiAsset => null;

		public bool ScrollbarActive { get; private set; }

		public TMP_InputField InputField => null;

		public bool InputFieldEnabled { get; private set; }

		public string InputFieldValue
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public bool InputFieldContainsCommand { get; private set; }

		public Sprite ExtraImageSprite
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public Color ExtraImageColor
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		public bool EmojiWindowEnabled { get; private set; }

		public Sprite EmojiButtonSprite
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public Color EmojiButtonColor
		{
			get
			{
				return default;
			}
			set
			{
			}
		}

		public event Action<ChatInformation> OnChatRegistered
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<Vector2, ChatInformation> OnUsernameHover
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<Vector2, ChatInformation> OnUsernameInteract
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action OnInputFieldEnabled
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action OnInputFieldDisabled
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<string> OnInputFieldUpdated
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<string> OnInputFieldSubmitted
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<string, string> OnInputFieldCommandUpdated
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<string, string> OnInputFieldCommandSubmitted
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action OnExtraImageInteract
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		private void Start()
		{
		}

		private void Update()
		{
		}

		private void OnEnable()
		{
		}

		private void ProcessNavigationSections()
		{
		}

		private void ProcessToggle()
		{
		}

		private void ProcessCollapse()
		{
		}

		private void ProcessScrollbarToggle()
		{
		}

		private void DisableScrollbar()
		{
		}

		private void ProcessInputField()
		{
		}

		private void UpdateChatBoxComponentSizes()
		{
		}

		private Rect CalculateRect(RectTransform rectTransform)
		{
			return default;
		}

		private void AdjustChatBoxScreenRect()
		{
		}

		private void CalculateUsernameRect(ChatInformation chatInfo)
		{
		}

		private void ConstrainContentBox()
		{
		}

		private void ConstrainScrollbarHandle()
		{
		}

		private void UpdateChatBoxVisibility()
		{
		}

		private void RepositionAllChats()
		{
		}

		private void OnTransformParentChanged()
		{
		}

		private TextMeshProUGUI GetTextFromPool()
		{
			return null;
		}

		private void SendTextToPool(TextMeshProUGUI chatText)
		{
		}

		private void RegisterChatInternal(string username, string message, ChatStyle style)
		{
		}

		private static string FormatDebug(string error, string solution, string objectName)
		{
			return null;
		}

		public void UpdatePositioning()
		{
		}

		public void RegisterChat(string message)
		{
		}

		public void RegisterChat(string message, ChatStyle style)
		{
		}

		public void RegisterChat(string username, string message)
		{
		}

		public void RegisterChat(string username, string message, ChatStyle style)
		{
		}

		public void Enable()
		{
		}

		public void Disable(bool instant = false)
		{
		}

		public void ClearChat()
		{
		}

		public void EnableInputField(string inputFieldValue = "")
		{
		}

		public void ToggleInputField()
		{
		}

		public void DisableInputField()
		{
		}

		public void SendCustomInput(Vector2 screenPosition, bool getButtonDown, bool getButton)
		{
		}

		public List<ChatInformation> FindChatsFromUser(string username)
		{
			return null;
		}

		public List<ChatInformation> FindChatsOfStyle(ChatStyle style)
		{
			return null;
		}

		public void TriggerExtraImage()
		{
		}
	}
}

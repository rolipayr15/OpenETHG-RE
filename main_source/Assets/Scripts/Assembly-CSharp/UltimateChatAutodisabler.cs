using TankAndHealerStudioAssets;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-30250)]
public class UltimateChatAutodisabler : MonoBehaviour
{
	[SerializeField]
	private UltimateChatBox chat;

	[SerializeField]
	private Button overlayButton;

	[Tooltip("In seconds")]
	[SerializeField]
	private float hideChatAfter;

	private float hideChatTimestamp;

	private bool serviceInteractable;

	private bool initialized;

	private bool closing;

	private bool subscribed;

	private bool fieldFocusSeen;

	private bool nativePointerOwned;

	private bool nativeTouchActive;

	private bool previousExternalScrollInput;

	private static readonly Vector2 NoPointerPosition;

	private int returnFrame;

	private GameplayInputContext policy;

	private GameplayInputContext.Lease textLease;

	public GameplayInputContext.Lease InputLease => null;

	private void Awake()
	{
	}

	private void OnEnable()
	{
	}

	public static void SetServiceInteractable(UltimateChatBox target, bool value)
	{
		if (target != null)
		{
			UltimateChatAutodisabler autodisabler = target.GetComponent<UltimateChatAutodisabler>();
			if (autodisabler != null)
			{
				autodisabler.serviceInteractable = value;
				autodisabler.initialized = true;
				autodisabler.RefreshPermission();
			}
		}
	}

	private bool CanOpen()
	{
		return false;
	}

	private bool CanEdit()
	{
		return false;
	}

	private void RefreshPermission()
	{
	}

	public void ProcessReturn(bool pressed)
	{
	}

	public void SubmitFromPointer()
	{
	}

	private void EnableWhenInputFieldEnabled()
	{
	}

	private void InputFieldDisabled()
	{
	}

	private void ReleaseWhenFieldLostFocus()
	{
	}

	private void SoftKeyboardStatusChanged(TouchScreenKeyboard.Status status)
	{
	}

	public void CancelInput()
	{
	}

	private void EnableWhenChatRegistered(UltimateChatBox.ChatInformation info)
	{
	}

	private void ShowChat()
	{
	}

	private void HideChat()
	{
	}

	private void Update()
	{
	}

	private void UpdateNativePointer()
	{
	}

	private void RouteNativePointer(PointerFrame pointer, bool pointerAllowed, bool touchActive)
	{
	}

	private void ClearNativePointer()
	{
	}

	private void OnApplicationFocus(bool focus)
	{
	}

	private void OnApplicationPause(bool paused)
	{
	}

	private void OnDisable()
	{
	}

	private void OnDestroy()
	{
	}
}

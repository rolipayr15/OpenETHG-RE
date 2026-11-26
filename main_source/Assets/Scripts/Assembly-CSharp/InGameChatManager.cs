using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Multiplayer.Chat;
using Reflex.Attributes;
using TMPro;
using TankAndHealerStudioAssets;
using UnityEngine;

[DefaultExecutionOrder(-30100)]
public class InGameChatManager : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass7_0
	{
		public bool chatDisabled;

		public InGameChatManager _003C_003E4__this;

		public bool userBanned;

		internal void _003CStart_003Eb__0(bool isDisabled)
		{
		}

		internal void _003CStart_003Eb__1(UserBannedResponse banResponse)
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003CCheckIfUserIsTheOnlyChatMember_003Ed__24 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public InGameChatManager _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003CCheckIfUserIsTheOnlyChatMember_003Ed__24(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003CStart_003Ed__7 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public InGameChatManager _003C_003E4__this;

		private _003C_003Ec__DisplayClass7_0 _003C_003E8__1;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003CStart_003Ed__7(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	[SerializeField]
	private UltimateChatBox ultimateChatBox;

	[Inject]
	private IChatClient chatClient;

	[SerializeField]
	private TMP_InputField inputField;

	[SerializeField]
	[Min(0f)]
	private float frequentErrorWindowSeconds;

	private float lastErrorTime;

	private float nextAllowedUnstableMessageTime;

	private bool chatEventsSubscribed;

	internal bool SettingsAvailable => false;

	private IEnumerator Start()
	{
		UltimateChatAutodisabler.SetServiceInteractable(ultimateChatBox, false);
		if (!UserGameSettings.IsMultiplayer())
		{
			if (ultimateChatBox != null)
			{
				DisableChat();
			}
			yield break;
		}
		// Online chat is restored together with the server stage.
	}

	private void InitializeOnlineChat()
	{
	}

	private void SubscribeToChatEvents()
	{
	}

	private void UnsubscribeFromChatEvents()
	{
	}

	private void SendImageClicked()
	{
	}

	private void OnDisable()
	{
	}

	private void OnDestroy()
	{
	}

	private void OnOpenHandler()
	{
	}

	private void OnClosedHandler(ushort code, string reason)
	{
	}

	private void OnErrorHandler(string err)
	{
	}

	public void LocalMsg(string message)
	{
	}

	private void OnMuteSuccessHandler(string userPublicId)
	{
	}

	private void OnMuteErrorHandler(string userPublicId, string error)
	{
	}

	private void OnUnmuteSuccessHandler(string userPublicId)
	{
	}

	private void OnUnmuteErrorHandler(string userPublicId, string error)
	{
	}

	private string GetDisplayName(string userPublicId)
	{
		return null;
	}

	private string GetDisplayNameFromRoomMessage(RoomChatMessage roomMessage)
	{
		return null;
	}

	[IteratorStateMachine(typeof(_003CCheckIfUserIsTheOnlyChatMember_003Ed__24))]
	private IEnumerator CheckIfUserIsTheOnlyChatMember()
	{
		return null;
	}

	private void OnInitialStateHandler(InitialChatMessage initial)
	{
	}

	private void OnUserChatMessageHandler(UserChatMessage chatMessage)
	{
	}

	private void PrintMutedByUserMessage(string mutedByPublicId)
	{
	}

	private void PrintUserMuted(string userPublicId)
	{
	}

	private void OnRoomMessageHandler(RoomChatMessage roomMessage)
	{
	}

	private void OnServerErrorMessageHandler(ErrorChatMessage errorMessage)
	{
	}

	private bool IsLocalUser(string userPublicId)
	{
		return false;
	}

	private void RegisterChat(string username, string msg, bool isLocalPlayer)
	{
	}

	private void OnUsernameInteract(Vector2 v, UltimateChatBox.ChatInformation chatInformation)
	{
	}

	private void DisableChat()
	{
		if (ultimateChatBox != null)
		{
			ultimateChatBox.gameObject.SetActive(false);
		}
		ChatSettings.Dismiss(this);
	}

	private bool CanOpenSettingsFromKeyboard(ControlMode mode, bool sourceAvailable, bool hasFocus, bool requiresResume)
	{
		return false;
	}

	private void ProcessSettingsShortcut(bool pressed, ControlMode mode, bool sourceAvailable, bool hasFocus, bool requiresResume)
	{
	}

	private void LateUpdate()
	{
	}

	public void OpenChatSettings()
	{
	}

	private void SendServerMessage(string msg)
	{
	}

	private void SendServerErrorMessage(string err)
	{
	}
}

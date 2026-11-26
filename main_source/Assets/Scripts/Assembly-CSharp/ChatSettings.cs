using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Multiplayer.Chat;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChatSettings : MonoBehaviour
{
	private sealed class Request
	{
		internal InGameChatManager Owner;

		internal ChatSettings Window;

		internal GameplayInputContext.Lease Block;

		internal bool UsesInput;

		internal bool Loading;

		internal bool Closing;

		internal bool Invalidated;

		internal bool Retired;
	}

	[StructLayout((LayoutKind)3)]
	[CompilerGenerated]
	private struct _003CMute_003Ed__41 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncVoidMethodBuilder _003C_003Et__builder;

		public ChatSettings _003C_003E4__this;

		private string _003CtargetUserId_003E5__2;

		private TaskAwaiter _003C_003Eu__1;

		private void MoveNext()
		{
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[StructLayout((LayoutKind)3)]
	[CompilerGenerated]
	private struct _003CReportUser_003Ed__42 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncVoidMethodBuilder _003C_003Et__builder;

		public ChatSettings _003C_003E4__this;

		public string reason;

		private string _003CtargetUserId_003E5__2;

		private TaskAwaiter _003C_003Eu__1;

		private void MoveNext()
		{
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	[Inject]
	private IChatClient chatClient;

	[SerializeField]
	private Text buttonLabel;

	[SerializeField]
	private Text muteInfo;

	[SerializeField]
	private Text reportInfo;

	private bool subscriptionsActive;

	private GameplayInputWindow inputWindow;

	private Request operation;

	private Canvas windowCanvas;

	private static Request current;

	private static Func<Action<ChatSettings>, bool> loadScene;

	private static Func<Scene, Action, bool> unloadScene;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetSession()
	{
	}

	public static void Show(InGameChatManager owner = null)
	{
	}

	internal static void Dismiss(InGameChatManager owner)
	{
	}

	private static void Protect(Request request)
	{
	}

	private static void Loaded(Request request, ChatSettings window)
	{
	}

	public void Close()
	{
	}

	private static void Close(Request request)
	{
	}

	private static void FailedClose(Request request)
	{
	}

	private static void Finish(Request request)
	{
	}

	private static bool LoadScene(Action<ChatSettings> completed)
	{
		return false;
	}

	private static bool UnloadScene(Scene scene, Action completed)
	{
		return false;
	}

	private static ChatSettings FindWindow(Scene scene)
	{
		return null;
	}

	private void Hide()
	{
	}

	private void OnEnable()
	{
	}

	private void Update()
	{
	}

	private void OnDisable()
	{
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	private void Subscribe()
	{
	}

	private void Unsubscribe()
	{
	}

	private void OnChatUsersChanged(IReadOnlyList<ChatUser> _)
	{
	}

	private void OnMutedUsersChanged(IReadOnlyCollection<string> _)
	{
	}

	private void OnMutedByChanged(IReadOnlyCollection<string> _)
	{
	}

	private void OnMuteSuccess(string userPublicId)
	{
	}

	private void OnMuteError(string userPublicId, string error)
	{
	}

	private void OnUnmuteSuccess(string userPublicId)
	{
	}

	private void OnUnmuteError(string userPublicId, string error)
	{
	}

	private void OnUserReportSuccess(string userPublicId, string room, string reason)
	{
	}

	private void OnUserReportError(string userPublicId, string room, string error)
	{
	}

	[AsyncStateMachine(typeof(_003CMute_003Ed__41))]
	public void Mute()
	{
	}

	[AsyncStateMachine(typeof(_003CReportUser_003Ed__42))]
	public void ReportUser(string reason)
	{
	}

	private void UpdateUi()
	{
	}

	private string GetPrimaryTargetUserId()
	{
		return null;
	}

	private string GetDisplayName(string userPublicId)
	{
		return null;
	}

	private void SetInfo(string message)
	{
	}
}

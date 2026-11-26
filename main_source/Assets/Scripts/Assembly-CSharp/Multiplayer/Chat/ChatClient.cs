using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BestHTTP;
using BestHTTP.WebSocket;

namespace Multiplayer.Chat
{
	public class ChatClient : IChatClient
	{
		[Serializable]
		private class ChatUserMuteActionPayload
		{
			public string userPublicId;

			public string room;
		}

		private class ChatReportActionPayload
		{
			public string userPublicId;

			public string room;

			public string reason;
		}

		private struct IsChatDisabledResponse
		{
			public bool isDisabled;
		}

		private class ToggleChatEnabledPayload
		{
			public bool enable;
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass103_0
		{
			public string userPublicId;

			public string room;

			public ChatClient _003C_003E4__this;

			public string reason;

			internal void _003CReportUserAsync_003Eb__1(string errorMessage)
			{
			}

			internal void _003CReportUserAsync_003Eb__0()
			{
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass104_0
		{
			public ChatClient _003C_003E4__this;

			public string userPublicId;

			internal void _003CMuteUserAsync_003Eb__1(string errorMessage)
			{
			}

			internal void _003CMuteUserAsync_003Eb__0()
			{
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass105_0
		{
			public ChatClient _003C_003E4__this;

			public string userPublicId;

			internal void _003CUnmuteUserAsync_003Eb__1(string errorMessage)
			{
			}

			internal void _003CUnmuteUserAsync_003Eb__0()
			{
			}
		}

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CIsChatDisabledAsync_003Ed__106 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder<bool> _003C_003Et__builder;

			public CancellationToken cancellationToken;

			public ChatClient _003C_003E4__this;

			private TaskAwaiter<HTTPResponse> _003C_003Eu__1;

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
		private struct _003CIsUserBannedAsync_003Ed__107 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder<UserBannedResponse> _003C_003Et__builder;

			public CancellationToken cancellationToken;

			public ChatClient _003C_003E4__this;

			private TaskAwaiter<HTTPResponse> _003C_003Eu__1;

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
		private struct _003CMuteUserAsync_003Ed__104 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder _003C_003Et__builder;

			public ChatClient _003C_003E4__this;

			public string userPublicId;

			public CancellationToken cancellationToken;

			public string room;

			private _003C_003Ec__DisplayClass104_0 _003C_003E8__1;

			private TaskAwaiter<HTTPResponse> _003C_003Eu__1;

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
		private struct _003CReportUserAsync_003Ed__103 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder _003C_003Et__builder;

			public string userPublicId;

			public string room;

			public ChatClient _003C_003E4__this;

			public string reason;

			public CancellationToken cancellationToken;

			private _003C_003Ec__DisplayClass103_0 _003C_003E8__1;

			private TaskAwaiter<HTTPResponse> _003C_003Eu__1;

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
		private struct _003CToggleChatEnabledAsync_003Ed__108 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder _003C_003Et__builder;

			public CancellationToken cancellationToken;

			public bool enable;

			public ChatClient _003C_003E4__this;

			private TaskAwaiter<HTTPResponse> _003C_003Eu__1;

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
		private struct _003CUnmuteUserAsync_003Ed__105 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder _003C_003Et__builder;

			public ChatClient _003C_003E4__this;

			public string userPublicId;

			public CancellationToken cancellationToken;

			public string room;

			private _003C_003Ec__DisplayClass105_0 _003C_003E8__1;

			private TaskAwaiter<HTTPResponse> _003C_003Eu__1;

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

		[CompilerGenerated]
		private Action m_OnOpen;

		[CompilerGenerated]
		private Action<ushort, string> m_OnClosed;

		[CompilerGenerated]
		private Action<string> m_OnError;

		[CompilerGenerated]
		private Action<string> m_OnTextMessage;

		[CompilerGenerated]
		private Action<byte[]> m_OnBinaryMessage;

		[CompilerGenerated]
		private Action<UserChatMessage> m_OnUserChatMessage;

		[CompilerGenerated]
		private Action<InitialChatMessage> m_OnInitialStateReceived;

		[CompilerGenerated]
		private Action<RoomChatMessage> m_OnRoomMessageReceived;

		[CompilerGenerated]
		private Action<ErrorChatMessage> m_OnServerErrorMessage;

		[CompilerGenerated]
		private Action<IReadOnlyList<ChatUser>> m_OnChatUsersChanged;

		[CompilerGenerated]
		private Action<IReadOnlyCollection<string>> m_OnMutedByChanged;

		[CompilerGenerated]
		private Action<IReadOnlyCollection<string>> m_OnMutedUsersChanged;

		[CompilerGenerated]
		private Action<string> m_OnMuteSuccess;

		[CompilerGenerated]
		private Action<string, string> m_OnMuteError;

		[CompilerGenerated]
		private Action<string> m_OnUnmuteSuccess;

		[CompilerGenerated]
		private Action<string, string> m_OnUnmuteError;

		[CompilerGenerated]
		private Action<string, string, string> m_OnUserReportSuccess;

		[CompilerGenerated]
		private Action<string, string, string> m_OnUserReportError;

		private const int MaxRestErrorMessageLength = 256;

		private string _url;

		private readonly bool _autoReconnect;

		private readonly float _reconnectBaseDelay;

		private readonly float _reconnectMaxDelay;

		private readonly bool _enablePing;

		private readonly float _pingInterval;

		private readonly List<ChatUser> _chatUsers;

		private readonly ReadOnlyCollection<ChatUser> _chatUsersReadOnly;

		private readonly Dictionary<string, ChatUser> _chatUsersById;

		private readonly HashSet<string> _mutedByUserIds;

		private readonly HashSet<string> _mutedUserIds;

		private string _myPublicUserId;

		private WebSocket _ws;

		private int _reconnectAttempts;

		private bool _manuallyClosed;

		private bool _isDisabled;

		private bool _isBanned;

		private readonly ConcurrentQueue<Action> _mainThreadQueue;

		private readonly SynchronizationContext _unityContext;

		private CancellationTokenSource _reconnectCts;

		public bool IsDisabled => false;

		public bool IsBanned => false;

		public IReadOnlyList<ChatUser> ChatUsers => null;

		public IReadOnlyCollection<string> MutedByUserIds => null;

		public IReadOnlyCollection<string> MutedUserIds => null;

		public string MyPublicUserId => null;

		public string EndpointUrl => null;

		public bool IsConnected => false;

		public bool IsConfigured => false;

		public event Action OnOpen
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

		public event Action<ushort, string> OnClosed
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

		public event Action<string> OnError
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

		public event Action<string> OnTextMessage
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

		public event Action<byte[]> OnBinaryMessage
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

		public event Action<UserChatMessage> OnUserChatMessage
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

		public event Action<InitialChatMessage> OnInitialStateReceived
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

		public event Action<RoomChatMessage> OnRoomMessageReceived
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

		public event Action<ErrorChatMessage> OnServerErrorMessage
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

		public event Action<IReadOnlyList<ChatUser>> OnChatUsersChanged
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

		public event Action<IReadOnlyCollection<string>> OnMutedByChanged
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

		public event Action<IReadOnlyCollection<string>> OnMutedUsersChanged
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

		public event Action<string> OnMuteSuccess
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

		public event Action<string, string> OnMuteError
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

		public event Action<string> OnUnmuteSuccess
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

		public event Action<string, string> OnUnmuteError
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

		public event Action<string, string, string> OnUserReportSuccess
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

		public event Action<string, string, string> OnUserReportError
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

		public void Connect()
		{
		}

		public void Disconnect(ushort code = 1000, string reason = "Client requested")
		{
		}

		public void Configure(string url)
		{
		}

		public bool TryGetChatUser(string publicUserId, out ChatUser user)
		{
			user = null;
			return false;
		}

		public string GetDisplayName(string publicUserId)
		{
			return null;
		}

		public bool IsMutedBy(string publicUserId)
		{
			return false;
		}

		public bool IsMuting(string publicUserId)
		{
			return false;
		}

		public void SendText(string message)
		{
		}

		public void SendBinary(byte[] data)
		{
		}

		[AsyncStateMachine(typeof(_003CReportUserAsync_003Ed__103))]
		public Task ReportUserAsync(string userPublicId, string room, string reason, CancellationToken cancellationToken = default(CancellationToken))
		{
			return null;
		}

		[AsyncStateMachine(typeof(_003CMuteUserAsync_003Ed__104))]
		public Task MuteUserAsync(string userPublicId, string room, CancellationToken cancellationToken = default(CancellationToken))
		{
			return null;
		}

		[AsyncStateMachine(typeof(_003CUnmuteUserAsync_003Ed__105))]
		public Task UnmuteUserAsync(string userPublicId, string room, CancellationToken cancellationToken = default(CancellationToken))
		{
			return null;
		}

		[AsyncStateMachine(typeof(_003CIsChatDisabledAsync_003Ed__106))]
		public Task<bool> IsChatDisabledAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return null;
		}

		[AsyncStateMachine(typeof(_003CIsUserBannedAsync_003Ed__107))]
		public Task<UserBannedResponse> IsUserBannedAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return null;
		}

		[AsyncStateMachine(typeof(_003CToggleChatEnabledAsync_003Ed__108))]
		public Task ToggleChatEnabledAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken))
		{
			return null;
		}

		public void Tick()
		{
		}

		public void Dispose()
		{
		}

		private void InternalConnect()
		{
		}

		private void AuthorizeRequest(HTTPRequest request)
		{
		}

		private void TryClose(ushort code, string reason)
		{
		}

		private void ScheduleReconnect()
		{
		}

		private bool CanScheduleReconnect()
		{
			return false;
		}

		private void EnqueueMain(Action a)
		{
		}

		private void HandleIncomingMessage(string rawMessage)
		{
		}

		private void HandleInitialMessage(InitialChatMessage payload)
		{
		}

		private void HandleRoomMessage(RoomChatMessage roomMessage)
		{
		}

		private void ReplaceChatUsers(IEnumerable<ChatUser> users)
		{
		}

		private void ReplaceMutedUsers(IEnumerable<ChatUser> users)
		{
		}

		private bool AddOrUpdateChatUser(ChatUser user)
		{
			return false;
		}

		private bool RemoveChatUser(string publicUserId)
		{
			return false;
		}

		private void ReplaceMutedBy(IEnumerable<ChatUser> users)
		{
		}

		private void HandleMutedByChange(string targetPublicId, string actorPublicId, bool add)
		{
		}

		private void HandleMutedUsersChange(string targetPublicId, string actorPublicId, bool add)
		{
		}

		private void AddMutedUserInternal(string publicUserId)
		{
		}

		private void RemoveMutedUserInternal(string publicUserId)
		{
		}

		private static string TruncateRestMessage(string message)
		{
			return null;
		}
	}
}

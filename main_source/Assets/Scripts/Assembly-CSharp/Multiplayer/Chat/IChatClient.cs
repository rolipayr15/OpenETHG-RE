using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Multiplayer.Chat
{
	public interface IChatClient
	{
		IReadOnlyList<ChatUser> ChatUsers { get; }

		IReadOnlyCollection<string> MutedByUserIds { get; }

		IReadOnlyCollection<string> MutedUserIds { get; }

		string MyPublicUserId { get; }

		string EndpointUrl { get; }

		bool IsConnected { get; }

		bool IsConfigured { get; }

		bool IsDisabled { get; }

		event Action OnOpen;

		event Action<ushort, string> OnClosed;

		event Action<string> OnError;

		event Action<string> OnTextMessage;

		event Action<byte[]> OnBinaryMessage;

		event Action<UserChatMessage> OnUserChatMessage;

		event Action<InitialChatMessage> OnInitialStateReceived;

		event Action<RoomChatMessage> OnRoomMessageReceived;

		event Action<ErrorChatMessage> OnServerErrorMessage;

		event Action<IReadOnlyList<ChatUser>> OnChatUsersChanged;

		event Action<IReadOnlyCollection<string>> OnMutedByChanged;

		event Action<IReadOnlyCollection<string>> OnMutedUsersChanged;

		event Action<string> OnMuteSuccess;

		event Action<string, string> OnMuteError;

		event Action<string> OnUnmuteSuccess;

		event Action<string, string> OnUnmuteError;

		event Action<string, string, string> OnUserReportSuccess;

		event Action<string, string, string> OnUserReportError;

		void Configure(string url);

		void Connect();

		void Disconnect(ushort code = 1000, string reason = "Client requested");

		void SendText(string message);

		void SendBinary(byte[] data);

		void Tick();

		Task ReportUserAsync(string userPublicId, string room, string reason, CancellationToken cancellationToken = default(CancellationToken));

		Task MuteUserAsync(string userPublicId, string room, CancellationToken cancellationToken = default(CancellationToken));

		Task UnmuteUserAsync(string userPublicId, string room, CancellationToken cancellationToken = default(CancellationToken));

		Task<bool> IsChatDisabledAsync(CancellationToken cancellationToken = default(CancellationToken));

		Task<UserBannedResponse> IsUserBannedAsync(CancellationToken cancellationToken = default(CancellationToken));

		Task ToggleChatEnabledAsync(bool enable, CancellationToken cancellationToken = default(CancellationToken));

		bool TryGetChatUser(string publicUserId, out ChatUser user);

		string GetDisplayName(string publicUserId);

		bool IsMutedBy(string publicUserId);

		bool IsMuting(string publicUserId);
	}
}

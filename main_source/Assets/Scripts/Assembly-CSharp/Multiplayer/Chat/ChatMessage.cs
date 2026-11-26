namespace Multiplayer.Chat
{
	public class ChatMessage
	{
		public ChatMessageType type;

		public UserChatMessage chat;

		public ErrorChatMessage error;

		public InitialChatMessage initial;

		public RoomChatMessage room;
	}
}

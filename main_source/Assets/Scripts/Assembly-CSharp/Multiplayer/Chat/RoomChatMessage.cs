namespace Multiplayer.Chat
{
	public class RoomChatMessage
	{
		public RoomMessageCode code;

		public ChatUser user;

		public ChatUser by;

		public string reason;

		public Mutes mutes;

		public string message;
	}
}

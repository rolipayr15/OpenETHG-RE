namespace Multiplayer.Chat
{
	public class UserBannedResponse
	{
		public bool isBanned;

		public long? banExpiryTs { get; set; }
	}
}

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Multiplayer.Chat
{
	public class InitialChatMessage
	{
		public string publicUserId;

		public string userName;

		public List<ChatUser> chatMembers;

		[JsonProperty("mutedBy")]
		public List<ChatUser> mutedByUsers;

		[JsonProperty("mutedUsers")]
		public List<ChatUser> mutedUsers;
	}
}

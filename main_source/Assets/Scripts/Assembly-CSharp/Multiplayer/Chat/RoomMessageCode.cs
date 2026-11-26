using System.Runtime.Serialization;

namespace Multiplayer.Chat
{
	public enum RoomMessageCode
	{
		[EnumMember(Value = "userJoined")]
		UserJoined = 0,
		[EnumMember(Value = "userLeft")]
		UserLeft = 1,
		[EnumMember(Value = "userMuted")]
		UserMuted = 2,
		[EnumMember(Value = "userUnmuted")]
		UserUnmuted = 3,
		[EnumMember(Value = "userReported")]
		UserReported = 4,
		[EnumMember(Value = "userBanned7d")]
		UserBanned7d = 5,
		[EnumMember(Value = "userBanned")]
		UserBanned = 6,
		[EnumMember(Value = "userWarning")]
		UserWarning = 7,
		[EnumMember(Value = "custom")]
		Custom = 8
	}
}

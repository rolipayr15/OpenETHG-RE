using System.Runtime.Serialization;

namespace Multiplayer.Chat
{
	public enum ErrorMessageCode
	{
		[EnumMember(Value = "messageBlocked")]
		MessageBlocked = 0,
		[EnumMember(Value = "unexpectedError")]
		UnexpectedError = 1,
		[EnumMember(Value = "roomNotFound")]
		RoomNotFound = 2,
		[EnumMember(Value = "joinFailed")]
		JoinFailed = 3,
		[EnumMember(Value = "userNotFound")]
		UserNotFound = 4,
		[EnumMember(Value = "rateLimitExceeded")]
		RateLimitExceeded = 5,
		[EnumMember(Value = "messageTooLong")]
		MessageTooLong = 6
	}
}

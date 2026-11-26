using System.Runtime.Serialization;

namespace Multiplayer.Chat
{
	public enum ChatMessageType
	{
		[EnumMember(Value = "chat")]
		Chat = 0,
		[EnumMember(Value = "initial")]
		Initial = 1,
		[EnumMember(Value = "error")]
		Error = 2,
		[EnumMember(Value = "room")]
		Room = 3
	}
}

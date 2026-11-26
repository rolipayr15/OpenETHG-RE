using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum Difficulty
{
	[EnumMember(Value = "CA")]
	Casual = 0,
	[EnumMember(Value = "EA")]
	Easy = 1,
	[EnumMember(Value = "NO")]
	Normal = 2,
	[EnumMember(Value = "HA")]
	Hard = 3,
	[EnumMember(Value = "NI")]
	Nightmare = 4
}

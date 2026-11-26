using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum Enemy
{
	[EnumMember(Value = "KR")]
	Krasue = 0,
	[EnumMember(Value = "CH")]
	Charlie = 1,
	[EnumMember(Value = "DG")]
	Dog = 2,
	[EnumMember(Value = "CU")]
	Custom = 3,
	[EnumMember(Value = "SB")]
	Sandbox = 4,
	[EnumMember(Value = "DB")]
	Double = 5,
	[EnumMember(Value = "WT")]
	Witch = 6,
	[EnumMember(Value = "GS")]
	Goose = 7
}

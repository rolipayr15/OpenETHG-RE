using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum Mode
{
	[EnumMember(Value = "S")]
	STANDARD = 0,
	[EnumMember(Value = "E")]
	ENDLESS = 1,
	[EnumMember(Value = "H")]
	HALLOWEEN = 2,
	[EnumMember(Value = "P")]
	PIXEL = 3
}

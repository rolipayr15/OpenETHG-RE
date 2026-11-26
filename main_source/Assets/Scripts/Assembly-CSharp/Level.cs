using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum Level
{
	[EnumMember(Value = "M")]
	Mansion = 0,
	[EnumMember(Value = "H")]
	Hospital = 1,
	[EnumMember(Value = "T")]
	Metro = 2,
	[EnumMember(Value = "S")]
	School = 3
}

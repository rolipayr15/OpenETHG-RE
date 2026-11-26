using System;

namespace Multiplayer
{
	[Serializable]
	public struct ErrorResponse
	{
		public string code;

		public string reason;

		public ErrorResponse(string _code, string _reason)
		{
			code = _code;
			reason = _reason;
		}
	}
}

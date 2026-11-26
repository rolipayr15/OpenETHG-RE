using System;
using System.Threading;
using System.Threading.Tasks;
using BestHTTP;

namespace Multiplayer
{
	public static class ChatApiClient
	{
		static ChatApiClient()
		{
		}

		public static Task<HTTPResponse> PostJsonAsync(string path, string json, CancellationToken cancellationToken = default(CancellationToken), Action<string> onError = null)
		{
			return null;
		}

		public static Task<HTTPResponse> GetAsync(string path, CancellationToken cancellationToken = default(CancellationToken), Action<string> onError = null)
		{
			return null;
		}
	}
}

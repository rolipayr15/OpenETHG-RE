using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BestHTTP;

namespace Multiplayer
{
	public static class CiekawostkomierzApiClient
	{
		static CiekawostkomierzApiClient()
		{
		}

		public static Task<HTTPResponse> GetAsync(string path)
		{
			return ApiClientBase.SendAsync(ApiConfiguration.GetCiekawostkomierzBaseUrl(), HTTPMethods.Get, path, null);
		}

		public static Task<HTTPResponse> GetAsyncUnauthenticated(string path)
		{
			return ApiClientBase.SendAsync(ApiConfiguration.GetCiekawostkomierzBaseUrl(), HTTPMethods.Get, path, null, default(System.Threading.CancellationToken), null, false);
		}

		public static Task<HTTPResponse> PostJsonAsync(string path, string json)
		{
			return ApiClientBase.SendAsync(ApiConfiguration.GetCiekawostkomierzBaseUrl(), HTTPMethods.Post, path, json);
		}
	}
}

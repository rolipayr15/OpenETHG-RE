using System;
using UnityEngine;

// Backend, chat and "Ciekawostkomierz" (game news/trivia) hosts. Production has a primary and an alternate region;
// development/local hosts are intentionally scrubbed in release builds.
public static class ApiConfiguration
{
	private static class BackendHosts
	{
		public static string PRODUCTION = "backend.ethg-api.online";

		public const string DEVELOPMENT = "ohnononono";

		public const string LOCAL = "ohnononono";
	}

	private static class CiekawostkomierzHosts
	{
		public static string PRODUCTION = "ext.ethg-api.online";

		public const string DEVELOPMENT = "ohnononono";

		public const string LOCAL = "ohnononono";
	}

	private static class ChatHosts
	{
		public static string PRODUCTION = "chat.ethg-api.online";

		public const string DEVELOPMENT = "ohnononono";

		public const string LOCAL = "ohnononono";
	}

	private static ApiEnvironment currentEnvironment = ApiEnvironment.Production;

	private static bool insecureServer;

	private static string[] backendUrls = new string[2] { "backend.ethg-api.online", "backend-alt.ethg-api.online" };

	private static string[] chatUrls = new string[2] { "chat.ethg-api.online", "chat-alt.ethg-api.online" };

	public static string[] ChatUrls => chatUrls;

	public static string[] BackendUrls => backendUrls;

	// Region index chosen by latency probing; out-of-range values fall back to the primary hosts.
	public static void SetUrls(int region)
	{
		if (region < 0 || region >= chatUrls.Length)
		{
			region = 0;
		}
		BackendHosts.PRODUCTION = backendUrls[region];
		ChatHosts.PRODUCTION = chatUrls[region];
	}

	public static string GetBackendHttpUrl(string path)
	{
		return ((currentEnvironment == ApiEnvironment.Local || insecureServer) ? "http" : "https") + "://" + GetBackendHost() + path;
	}

	public static string GetBackendWebSocketUrl(string gameId, int playerId)
	{
		return string.Format("{0}://{1}/ws/v1/{2}/join/{3}", (currentEnvironment == ApiEnvironment.Local || insecureServer) ? "ws" : "wss", GetBackendHost(), gameId, playerId);
	}

	public static string GetCiekawostkomierzBaseUrl()
	{
		return ((currentEnvironment == ApiEnvironment.Local || insecureServer) ? "http" : "https") + "://" + GetCiekawostkomierzHost() + "/api/v1";
	}

	public static string GetChatHttpUrl()
	{
		return ((currentEnvironment == ApiEnvironment.Local || insecureServer) ? "http" : "https") + "://" + GetChatHost() + "/api/v1";
	}

	public static string GetChatWebSocketUrl()
	{
		return ((currentEnvironment == ApiEnvironment.Local || insecureServer) ? "ws" : "wss") + "://" + GetChatHost() + "/ws/v1";
	}

	// Recovery addition: points every backend at the restored server (see OnlineConfig).
	public static void SetServer(string url)
	{
		Uri uri = new Uri(url);
		string host = uri.IsDefaultPort ? uri.Host : uri.Host + ":" + uri.Port;
		BackendHosts.PRODUCTION = host;
		ChatHosts.PRODUCTION = host;
		CiekawostkomierzHosts.PRODUCTION = host;
		backendUrls = new string[1] { host };
		chatUrls = new string[1] { host };
		insecureServer = uri.Scheme == "http";
	}

	public static void UseOfficialServers()
	{
		BackendHosts.PRODUCTION = "backend.ethg-api.online";
		ChatHosts.PRODUCTION = "chat.ethg-api.online";
		CiekawostkomierzHosts.PRODUCTION = "ext.ethg-api.online";
		backendUrls = new string[2] { "backend.ethg-api.online", "backend-alt.ethg-api.online" };
		chatUrls = new string[2] { "chat.ethg-api.online", "chat-alt.ethg-api.online" };
		insecureServer = false;
	}

	public static void SetEnvironment(ApiEnvironment env)
	{
		currentEnvironment = env;
		Debug.Log(string.Format("API Environment set to: {0}", env));
	}

	public static ApiEnvironment GetEnvironment()
	{
		return currentEnvironment;
	}

	private static string GetBackendHost()
	{
		return currentEnvironment == ApiEnvironment.Development || currentEnvironment == ApiEnvironment.Local ? BackendHosts.DEVELOPMENT : BackendHosts.PRODUCTION;
	}

	private static string GetCiekawostkomierzHost()
	{
		return currentEnvironment == ApiEnvironment.Development || currentEnvironment == ApiEnvironment.Local ? CiekawostkomierzHosts.DEVELOPMENT : CiekawostkomierzHosts.PRODUCTION;
	}

	private static string GetChatHost()
	{
		return currentEnvironment == ApiEnvironment.Development || currentEnvironment == ApiEnvironment.Local ? ChatHosts.DEVELOPMENT : ChatHosts.PRODUCTION;
	}
}

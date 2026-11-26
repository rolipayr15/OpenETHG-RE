using Debug = UnityEngine.Debug;
using Firebase.Crashlytics;
using EyesAnalytics;
using System.Text;
using UnityEngine;
using Unity.Services.Authentication;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BestHTTP;

namespace Multiplayer
{
	public static class ApiClientBase
	{

		private static int connectTimeoutMs = 10000;

		private static int requestTimeoutMs = 15000;

		private static int maxRetryAttempts = 3;

		private static int retryBaseDelayMs = 500;

		private static string requestIdHeaderName = "req";

		public static void AuthorizeRequest(HTTPRequest request)
		{
			string credential = MultiplayerSettings.GetReviewerBasicAuthCredential();
			request.AddHeader("Authorization", (UserGameSettings.IsSignedInWithBasicAuth && !string.IsNullOrEmpty(credential)) ? ("Basic " + credential) : AuthenticationService.Instance.AccessToken);
			request.AddHeader(requestIdHeaderName, Guid.NewGuid().ToString());
			request.AddHeader("dev", SystemInfo.deviceUniqueIdentifier);
			request.AddHeader("X-Game-User", UserGameSettings.multiplayerUsername);
			request.AddHeader("Client-Version", "4");
			request.AddHeader("Game-Version", Application.version);
		}

		public async static Task<HTTPResponse> SendAsync(string baseUrl, HTTPMethods verb, string path, string bodyJsonOrNull, CancellationToken cancellationToken = default(CancellationToken), Action<string> onError = null, bool requireAuthentication = true)
		{
			if (requireAuthentication && !UserGameSettings.isSignedIn)
			{
				Debug.LogWarning(string.Format("User tried to send unauthenticated request: {0} {1}", verb, path));
				return null;
			}
			Uri uri = new Uri(baseUrl + path);
			for (int attempt = 1; attempt <= maxRetryAttempts; attempt++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				HTTPRequest request = new HTTPRequest(uri, verb);
				request.ConnectTimeout = TimeSpan.FromMilliseconds(connectTimeoutMs);
				request.Timeout = TimeSpan.FromMilliseconds(requestTimeoutMs);
				if (requireAuthentication)
				{
					AuthorizeRequest(request);
				}
				if (bodyJsonOrNull != null)
				{
					request.AddHeader("Content-Type", "application/json");
					request.RawData = Encoding.UTF8.GetBytes(bodyJsonOrNull);
				}
				bool retry = false;
				using (CancellationTokenRegistration registration = cancellationToken.CanBeCanceled ? cancellationToken.Register(delegate
				{
					if (request.State == (HTTPRequestStates)0 || request.State == (HTTPRequestStates)1 || request.State == (HTTPRequestStates)2)
					{
						request.Abort();
					}
				}) : default(CancellationTokenRegistration))
				{
					if (attempt == 1)
					{
						Debug.Log(string.Format("sending verb {0} request to {1}", verb, uri));
					}
					else
					{
						Debug.LogWarning(string.Format("Retrying API {0} {1}, attempt {2}.", verb, path, attempt));
					}
					HTTPResponse response = await request.GetHTTPResponseAsync();
					if (response != null && response.IsSuccess)
					{
						return response;
					}
					if (attempt != maxRetryAttempts && ShouldRetryResponse(response, request.State))
					{
						Debug.LogWarning(string.Format("API {0} {1} failed on attempt {2}: {3}. Retrying...", verb, path, attempt, response != null ? string.Format("{0} {1}", response.StatusCode, response.Message) : "no response"));
						retry = true;
					}
					else
					{
						string message = response == null ? string.Format("API {0} {1} failed with no response. State: {2}", verb, path, request.State) : string.Format("API {0} {1} failed: {2} {3}", verb, path, response.StatusCode, response.Message);
						Debug.Log(message);
						ReportFinalError("response", response != null ? (int?)response.StatusCode : null, request.State.ToString());
						onError?.Invoke(message);
						return response;
					}
				}
				if (retry)
				{
					await Task.Delay(GetRetryDelay(attempt), cancellationToken);
				}
			}
			return null;
		}

		private static void ReportFinalError(string errorKind, int? statusCode, string requestState, string exceptionType = null)
		{
			AnalyticsDispatcher.RunWhenReady(delegate
			{
				MultiplayerServer.ApiError(errorKind, statusCode);
			});
			AnalyticsDispatcher.RunOnMainThread(delegate
			{
				if (FirebaseState.IsInitialized)
				{
					Crashlytics.Log("Multiplayer API " + errorKind + "; status=" + (statusCode?.ToString() ?? "none") + "; state=" + requestState + "; exception=" + (exceptionType ?? "none"));
				}
			});
		}

		private static bool ShouldRetryResponse(HTTPResponse response, HTTPRequestStates state)
		{
			if (response != null)
			{
				if (response.IsSuccess)
				{
					return false;
				}
				if (response.StatusCode == 0)
				{
					return true;
				}
			}
			return IsTransientState(state);
		}

		private static bool ShouldRetryException(HTTPRequestStates state, Exception ex)
		{
			return IsTransientState(state) || ex is TimeoutException;
		}

		private static bool IsTransientState(HTTPRequestStates state)
		{
			int value = (int)state;
			return value < 8 && ((0xD0 >> value) & 1) != 0;
		}

		private static TimeSpan GetRetryDelay(int attempt)
		{
			return TimeSpan.FromMilliseconds(Math.Min(4000, retryBaseDelayMs << (attempt - 1)));
		}
	}
}

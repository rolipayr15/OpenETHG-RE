using System;
using UnityEngine;

namespace Balaso
{
	public class AppTrackingTransparency
	{
		public enum AuthorizationStatus
		{
			NOT_DETERMINED,
			RESTRICTED,
			DENIED,
			AUTHORIZED
		}

		public static Action<AuthorizationStatus> OnAuthorizationRequestDone;

		public static AuthorizationStatus TrackingAuthorizationStatus => AuthorizationStatus.NOT_DETERMINED;

		public static void UpdateConversionValue(int value)
		{
		}

		public static void RegisterAppForAdNetworkAttribution()
		{
		}

		public static void RequestTrackingAuthorization()
		{
			Debug.Log($"Platform '{Application.platform}' not supported");
		}

		public static string IdentifierForAdvertising()
		{
			Debug.Log($"Platform '{Application.platform}' not supported");
			return null;
		}
	}
}

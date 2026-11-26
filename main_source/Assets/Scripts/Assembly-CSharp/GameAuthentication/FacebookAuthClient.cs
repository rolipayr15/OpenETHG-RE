using System;
using System.Collections.Generic;

namespace GameAuthentication
{
	internal sealed class FacebookAuthClient : IFacebookAuthClient
	{
		public bool IsInitialized => false;

		public bool IsLoggedIn => false;

		public string AccessToken => null;

		public string UserId => null;

		public void ActivateApp()
		{
		}

		public void Init(Action<bool> onInitComplete)
		{
		}

		public void LogInWithReadPermissions(IEnumerable<string> permissions, Action<string, string, bool> onComplete)
		{
		}

		public void LogOut()
		{
		}
	}
}

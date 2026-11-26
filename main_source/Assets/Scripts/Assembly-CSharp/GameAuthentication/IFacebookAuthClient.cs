using System;
using System.Collections.Generic;

namespace GameAuthentication
{
	internal interface IFacebookAuthClient
	{
		bool IsInitialized { get; }

		bool IsLoggedIn { get; }

		string AccessToken { get; }

		string UserId { get; }

		void ActivateApp();

		void Init(Action<bool> onInitComplete);

		void LogInWithReadPermissions(IEnumerable<string> permissions, Action<string, string, bool> onComplete);

		void LogOut();
	}
}

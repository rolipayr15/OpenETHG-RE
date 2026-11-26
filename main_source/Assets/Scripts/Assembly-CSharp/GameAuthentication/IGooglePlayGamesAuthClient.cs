using System;

namespace GameAuthentication
{
	internal interface IGooglePlayGamesAuthClient
	{
		bool IsAuthenticated { get; }

		string LocalPlayerId { get; }

		event Action<bool, string> AuthenticationChanged;

		void Authenticate(bool interactive);

		void SignOut();

		void LoadServerAuthCode(Action<string, string> onComplete);
	}
}

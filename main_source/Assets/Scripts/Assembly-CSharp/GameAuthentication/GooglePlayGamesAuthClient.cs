using System;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

namespace GameAuthentication
{
	internal sealed class GooglePlayGamesAuthClient : IGooglePlayGamesAuthClient
	{
		private static bool startupSignInAttempted;

		private static bool signInInProgress;

		private readonly object gate;

		private EventCallback<GameServicesAuthStatusChangeResult> authHandler;

		private Action<bool, string> authenticationChanged;

		public bool IsAuthenticated => false;

		public string LocalPlayerId => null;

		public event Action<bool, string> AuthenticationChanged
		{
			add
			{
			}
			remove
			{
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetSharedSignIn()
		{
		}

		public static void TrySignInOnStartup()
		{
		}

		public static void AuthenticateShared(bool interactive)
		{
		}

		private static void OnSharedSignInCompleted(GameServicesAuthStatusChangeResult result, Error error)
		{
		}

		public void Authenticate(bool interactive)
		{
		}

		public void SignOut()
		{
		}

		public void LoadServerAuthCode(Action<string, string> onComplete)
		{
		}

		private void AttachAuthHandlerUnlocked()
		{
		}
	}
}

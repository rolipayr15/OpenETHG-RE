using System;
using System.Threading.Tasks;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;

namespace GameAuthentication
{
	internal sealed class UnityPlayerAccountClient : IUnityPlayerAccountClient
	{
		private readonly object gate;

		private Action signedIn;

		private Action<RequestFailedException> signInFailed;

		private bool serviceHandlersAttached;

		public bool IsSignedIn => false;

		public string AccessToken => null;

		public string IdentitySubject => null;

		public event Action SignedIn
		{
			add
			{
			}
			remove
			{
			}
		}

		public event Action<RequestFailedException> SignInFailed
		{
			add
			{
			}
			remove
			{
			}
		}

		public Task StartSignInAsync()
		{
			return null;
		}

		public void SignOut()
		{
		}

		private void EnsureServiceHandlersAttached()
		{
		}

		private void OnServiceSignedIn()
		{
		}

		private void OnServiceSignInFailed(RequestFailedException exception)
		{
		}

		private static bool TryGetService(out IPlayerAccountService service)
		{
			service = null;
			return false;
		}
	}
}

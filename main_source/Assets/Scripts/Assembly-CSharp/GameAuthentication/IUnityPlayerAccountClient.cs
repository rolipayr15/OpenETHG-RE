using System;
using System.Threading.Tasks;
using Unity.Services.Core;

namespace GameAuthentication
{
	internal interface IUnityPlayerAccountClient
	{
		bool IsSignedIn { get; }

		string AccessToken { get; }

		string IdentitySubject { get; }

		event Action SignedIn;

		event Action<RequestFailedException> SignInFailed;

		Task StartSignInAsync();

		void SignOut();
	}
}

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Services.Authentication;

namespace GameAuthentication
{
	public sealed class UgsGameAccountGateway : IUgsGameAccountGateway, IDisposable
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CGetCurrentAccountInfoAsync_003Ed__19 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder<GameAccountInfo> _003C_003Et__builder;

			public UgsGameAccountGateway _003C_003E4__this;

			private PlayerInfo _003Cinfo_003E5__2;

			private TaskAwaiter<PlayerInfo> _003C_003Eu__1;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CInitializeAsync_003Ed__16 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder _003C_003Et__builder;

			public UgsGameAccountGateway _003C_003E4__this;

			private TaskAwaiter _003C_003Eu__1;

			private YieldAwaitable.YieldAwaiter _003C_003Eu__2;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		private bool callbacksRegistered;

		[CompilerGenerated]
		private Action m_SessionExpired;

		public bool IsInitialized => false;

		public bool IsAuthorized => false;

		public bool IsSignedIn => false;

		public bool IsExpired => false;

		public bool SessionTokenExists => false;

		public string PlayerId => null;

		public event Action SessionExpired
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		[AsyncStateMachine(typeof(_003CInitializeAsync_003Ed__16))]
		public Task InitializeAsync()
		{
			return null;
		}

		public Task RestoreSessionAsync()
		{
			return null;
		}

		public Task CreateGuestAsync()
		{
			return null;
		}

		[AsyncStateMachine(typeof(_003CGetCurrentAccountInfoAsync_003Ed__19))]
		public Task<GameAccountInfo> GetCurrentAccountInfoAsync()
		{
			return null;
		}

		public Task LinkProviderAsync(ProviderCredential credential)
		{
			return null;
		}

		public Task SignInWithProviderAsync(ProviderCredential credential, bool createAccount)
		{
			return null;
		}

		public Task UnlinkProviderAsync(GameAuthProvider provider)
		{
			return null;
		}

		public void SignOut(bool clearCredentials)
		{
		}

		public Task DeleteGameAccountAsync()
		{
			return null;
		}

		public void Dispose()
		{
		}

		private void OnSessionExpired()
		{
		}

		private static GameAuthProvider? TryMapProvider(string typeId)
		{
			return null;
		}
	}
}

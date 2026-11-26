using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AppleAuth;

namespace GameAuthentication
{
	public sealed class AppleAuthProviderAdapter : IAuthProviderAdapter
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CAcquireCredentialAsync_003Ed__7 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder<ProviderCredentialResult> _003C_003Et__builder;

			public CancellationToken cancellationToken;

			public AppleAuthProviderAdapter _003C_003E4__this;

			private TaskAwaiter<ProviderCredentialResult> _003C_003Eu__1;

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

		private IAppleAuthManager appleAuthManager;

		public GameAuthProvider Provider => GameAuthProvider.UnityPlayerAccount;

		public bool HasActiveProviderSession => false;

		public AppleAuthProviderAdapter()
		{
		}

		internal AppleAuthProviderAdapter(IAppleAuthManager appleAuthManager)
		{
		}

		[AsyncStateMachine(typeof(_003CAcquireCredentialAsync_003Ed__7))]
		public Task<ProviderCredentialResult> AcquireCredentialAsync(CancellationToken cancellationToken)
		{
			return null;
		}

		public void SignOutProviderSession()
		{
		}

		public void Tick()
		{
		}

		private void EnsureInitialized()
		{
		}
	}
}

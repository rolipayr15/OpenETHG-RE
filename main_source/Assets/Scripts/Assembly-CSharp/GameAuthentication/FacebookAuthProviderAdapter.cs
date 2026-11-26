using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GameAuthentication
{
	public sealed class FacebookAuthProviderAdapter : IAuthProviderAdapter
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass12_0
		{
			public CancellationToken cancellationToken;

			public FacebookAuthProviderAdapter _003C_003E4__this;

			public int generation;

			public TaskCompletionSource<ProviderCredentialResult> completion;

			public int credentialContaminationVersion;

			internal void _003CAcquireCredentialAsync_003Eb__0(string token, string error, bool cancelled)
			{
			}
		}

		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass16_0
		{
			public TaskCompletionSource<bool> attempt;

			internal void _003CEnsureInitializedAsync_003Eb__0(bool initialized)
			{
			}
		}

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CAcquireCredentialAsync_003Ed__12 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder<ProviderCredentialResult> _003C_003Et__builder;

			public CancellationToken cancellationToken;

			public FacebookAuthProviderAdapter _003C_003E4__this;

			private _003C_003Ec__DisplayClass12_0 _003C_003E8__1;

			private TaskAwaiter<bool> _003C_003Eu__1;

			private TaskAwaiter<ProviderCredentialResult> _003C_003Eu__2;

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
		private struct _003CEnsureInitializedAsync_003Ed__16 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder<bool> _003C_003Et__builder;

			public FacebookAuthProviderAdapter _003C_003E4__this;

			public CancellationToken cancellationToken;

			private _003C_003Ec__DisplayClass16_0 _003C_003E8__1;

			private TaskAwaiter<bool> _003C_003Eu__1;

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

		private readonly IFacebookAuthClient client;

		private readonly object stateGate;

		private TaskCompletionSource<bool> initialization;

		private int acquisitionGeneration;

		private int sessionContaminationVersion;

		private bool rejectExistingSessionReuse;

		public GameAuthProvider Provider => GameAuthProvider.UnityPlayerAccount;

		public bool HasActiveProviderSession => false;

		public FacebookAuthProviderAdapter()
		{
		}

		internal FacebookAuthProviderAdapter(IFacebookAuthClient client)
		{
		}

		[AsyncStateMachine(typeof(_003CAcquireCredentialAsync_003Ed__12))]
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

		private void MarkRejected(int generation)
		{
		}

		[AsyncStateMachine(typeof(_003CEnsureInitializedAsync_003Ed__16))]
		private Task<bool> EnsureInitializedAsync(CancellationToken cancellationToken)
		{
			return null;
		}
	}
}

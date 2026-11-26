using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Core;

namespace GameAuthentication
{
	public sealed class UnityPlayerAccountProviderAdapter : IAuthProviderAdapter, IDisposable
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass14_0
		{
			public UnityPlayerAccountProviderAdapter _003C_003E4__this;

			public int generation;

			public TaskCompletionSource<ProviderCredentialResult> completion;

			internal void _003CAcquireCredentialAsync_003Eb__0()
			{
			}
		}

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CAcquireCredentialAsync_003Ed__14 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder<ProviderCredentialResult> _003C_003Et__builder;

			public UnityPlayerAccountProviderAdapter _003C_003E4__this;

			public CancellationToken cancellationToken;

			private _003C_003Ec__DisplayClass14_0 _003C_003E8__1;

			private CancellationTokenRegistration _003C_003E7__wrap1;

			private Task _003CstartSignIn_003E5__3;

			private TaskAwaiter<Task> _003C_003Eu__1;

			private TaskAwaiter<ProviderCredentialResult> _003C_003Eu__2;

			private TaskAwaiter _003C_003Eu__3;

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
		private struct _003CCompleteAttemptAsync_003Ed__19 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder<ProviderCredentialResult> _003C_003Et__builder;

			public Task<ProviderCredentialResult> operation;

			public UnityPlayerAccountProviderAdapter _003C_003E4__this;

			public CancellationToken cancellationToken;

			public int generation;

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

		private readonly IUnityPlayerAccountClient client;

		private readonly object gate;

		private int attemptGeneration;

		private int activeAttempt;

		private TaskCompletionSource<ProviderCredentialResult> activeCompletion;

		private CancellationToken activeCancellation;

		private bool rejectExistingSessionReuse;

		private bool disposed;

		public GameAuthProvider Provider => GameAuthProvider.UnityPlayerAccount;

		public bool HasActiveProviderSession => false;

		public UnityPlayerAccountProviderAdapter()
		{
		}

		internal UnityPlayerAccountProviderAdapter(IUnityPlayerAccountClient client)
		{
		}

		[AsyncStateMachine(typeof(_003CAcquireCredentialAsync_003Ed__14))]
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

		public void Dispose()
		{
		}

		private bool TryConsumeTrustedSession(out ProviderCredentialResult result)
		{
			result = null;
			return false;
		}

		[AsyncStateMachine(typeof(_003CCompleteAttemptAsync_003Ed__19))]
		private Task<ProviderCredentialResult> CompleteAttemptAsync(int generation, Task<ProviderCredentialResult> operation, CancellationToken cancellationToken)
		{
			return null;
		}

		private void OnAttemptCancelled(int generation, TaskCompletionSource<ProviderCredentialResult> completion)
		{
		}

		private void FailAttempt(int generation)
		{
		}

		private void ClearActiveAttempt(int generation)
		{
		}

		private void OnClientSignedIn()
		{
		}

		private void OnClientSignInFailed(RequestFailedException exception)
		{
		}
	}
}

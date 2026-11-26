using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GameAuthentication
{
	public sealed class GooglePlayGamesAuthProviderAdapter : IAuthProviderAdapter, IDisposable
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CAcquireCredentialAsync_003Ed__20 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder<ProviderCredentialResult> _003C_003Et__builder;

			public GooglePlayGamesAuthProviderAdapter _003C_003E4__this;

			public CancellationToken cancellationToken;

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

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CAuthenticateAsync_003Ed__24 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder<ProviderCredentialResult> _003C_003Et__builder;

			public GooglePlayGamesAuthProviderAdapter _003C_003E4__this;

			public CancellationToken cancellationToken;

			private int _003Cgeneration_003E5__2;

			private CancellationTokenRegistration _003C_003E7__wrap2;

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

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CLoadServerCredentialAsync_003Ed__25 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder<ProviderCredentialResult> _003C_003Et__builder;

			public CancellationToken cancellationToken;

			public GooglePlayGamesAuthProviderAdapter _003C_003E4__this;

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

		private static readonly TimeSpan DefaultAbandonedFlowQuarantine;

		private readonly IGooglePlayGamesAuthClient client;

		private readonly Func<DateTime> utcNow;

		private readonly TimeSpan abandonedFlowQuarantine;

		private readonly object gate;

		private int attemptGeneration;

		private int activeAttempt;

		private TaskCompletionSource<ProviderCredentialResult> activeAuthCompletion;

		private CancellationToken activeCancellation;

		private bool rejectExistingSessionReuse;

		private bool abandonedAuthenticationPending;

		private DateTime abandonedAuthenticationRecoveryAt;

		private bool disposed;

		public GameAuthProvider Provider => GameAuthProvider.UnityPlayerAccount;

		public bool HasActiveProviderSession => false;

		public GooglePlayGamesAuthProviderAdapter()
		{
		}

		internal GooglePlayGamesAuthProviderAdapter(IGooglePlayGamesAuthClient client)
		{
		}

		internal GooglePlayGamesAuthProviderAdapter(IGooglePlayGamesAuthClient client, Func<DateTime> utcNow, TimeSpan abandonedFlowQuarantine)
		{
		}

		[AsyncStateMachine(typeof(_003CAcquireCredentialAsync_003Ed__20))]
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

		[AsyncStateMachine(typeof(_003CAuthenticateAsync_003Ed__24))]
		private Task<ProviderCredentialResult> AuthenticateAsync(CancellationToken cancellationToken)
		{
			return null;
		}

		[AsyncStateMachine(typeof(_003CLoadServerCredentialAsync_003Ed__25))]
		private Task<ProviderCredentialResult> LoadServerCredentialAsync(CancellationToken cancellationToken)
		{
			return null;
		}

		private void OnAuthenticationChanged(bool authenticated, string error)
		{
		}
	}
}

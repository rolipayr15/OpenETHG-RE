using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameAuthentication
{
	public static class AuthSignOutPresenter
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CExecuteSignOutAsync_003Ed__18 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder _003C_003Et__builder;

			public Action onSignOutStarted;

			public IAuthCoordinator authCoordinator;

			public Action<AuthOperationResult> onCompleted;

			private TaskAwaiter<AuthOperationResult> _003C_003Eu__1;

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

		internal const string GuestSignOutTitle = "Sign out of Guest Game Account?";

		internal const string GuestSignOutWarning = "This Guest Game Account has no connected sign-in method. If you sign out, this progress may be permanently inaccessible.";

		internal const string LinkedSignOutTitle = "Sign out?";

		internal const string LinkedSignOutMessage = "Are you sure?";

		internal const string GuestSignOutConfirmLabel = "Sign Out";

		internal const string GuestSignOutCancelLabel = "Cancel";

		internal const string SignOutConfirmLabel = "Sign Out";

		internal const string SignOutCancelLabel = "Cancel";

		private static Func<string, string, string, Action, Action, bool> tryShowConfirmation;

		public static AuthSignOutRequestStatus TryRequestSignOut(IAuthCoordinator authCoordinator, Action<AuthOperationResult> onCompleted = null, Action onCancelled = null, Action onSignOutStarted = null)
		{
			return AuthSignOutRequestStatus.Proceeding;
		}

		internal static bool RequiresGuestWarning(AuthState state)
		{
			return false;
		}

		internal static bool RequiresSimpleConfirmation(AuthState state)
		{
			return false;
		}

		internal static string BuildGuestWarningMessage()
		{
			return null;
		}

		internal static string BuildLinkedConfirmationMessage()
		{
			return null;
		}

		private static AuthSignOutRequestStatus ShowConfirmationAndMaybeSignOut(string message, IAuthCoordinator authCoordinator, Action<AuthOperationResult> onCompleted, Action onCancelled, Action onSignOutStarted, string unavailableLogMessage)
		{
			return AuthSignOutRequestStatus.Proceeding;
		}

		internal static void SetConfirmationHandlerForTests(Func<string, string, string, Action, Action, bool> handler)
		{
		}

		internal static void ResetConfirmationHandlerForTests()
		{
		}

		private static bool DefaultTryShowConfirmation(string message, string yesLabel, string noLabel, Action onYes, Action onNo)
		{
			return false;
		}

		[AsyncStateMachine(typeof(_003CExecuteSignOutAsync_003Ed__18))]
		private static Task ExecuteSignOutAsync(IAuthCoordinator authCoordinator, Action<AuthOperationResult> onCompleted, Action onSignOutStarted)
		{
			return null;
		}
	}
}

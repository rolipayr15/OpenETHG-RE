using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GameAuthentication
{
	internal static class CancellableProviderTask
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CObserveAsync_003Ed__4 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder _003C_003Et__builder;

			public Task operation;

			private TaskAwaiter _003C_003Eu__1;

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
		private struct _003CWaitAsync_003Ed__0 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder _003C_003Et__builder;

			public Task operation;

			public CancellationToken cancellationToken;

			private TaskAwaiter _003C_003Eu__1;

			private CancellationTokenRegistration _003C_003E7__wrap1;

			private TaskAwaiter<Task> _003C_003Eu__2;

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
		private struct _003CWaitAsync_003Ed__1<T> : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder<T> _003C_003Et__builder;

			public Task<T> operation;

			public CancellationToken cancellationToken;

			private TaskAwaiter<T> _003C_003Eu__1;

			private CancellationTokenRegistration _003C_003E7__wrap1;

			private TaskAwaiter<Task> _003C_003Eu__2;

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
		private struct _003CWaitForCleanupAsync_003Ed__2 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder _003C_003Et__builder;

			public Task operation;

			public TimeSpan maximumWait;

			private TaskAwaiter<Task> _003C_003Eu__1;

			private TaskAwaiter _003C_003Eu__2;

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

		[AsyncStateMachine(typeof(_003CWaitAsync_003Ed__0))]
		public static Task WaitAsync(Task operation, CancellationToken cancellationToken)
		{
			return null;
		}

		[AsyncStateMachine(typeof(_003CWaitAsync_003Ed__1<>))]
		public static Task<T> WaitAsync<T>(Task<T> operation, CancellationToken cancellationToken)
		{
			return null;
		}

		[AsyncStateMachine(typeof(_003CWaitForCleanupAsync_003Ed__2))]
		public static Task WaitForCleanupAsync(Task operation, TimeSpan maximumWait)
		{
			return null;
		}

		public static void Observe(Task operation)
		{
		}

		[AsyncStateMachine(typeof(_003CObserveAsync_003Ed__4))]
		private static Task ObserveAsync(Task operation)
		{
			return null;
		}
	}
}

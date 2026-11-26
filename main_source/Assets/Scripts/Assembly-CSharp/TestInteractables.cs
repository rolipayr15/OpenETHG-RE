using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TestInteractables : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CStartClickingInteractables_003Ed__6 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TestInteractables _003C_003E4__this;

		private UnityEngine.Object[] _003C_003E7__wrap1;

		private int _003C_003E7__wrap2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003CStartClickingInteractables_003Ed__6(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	private int countClicked;

	private Interactable[] interactables;

	private int waitingForEndOfInteraction;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void InteractionEnded()
	{
	}

	[IteratorStateMachine(typeof(_003CStartClickingInteractables_003Ed__6))]
	private IEnumerator StartClickingInteractables()
	{
		return null;
	}
}

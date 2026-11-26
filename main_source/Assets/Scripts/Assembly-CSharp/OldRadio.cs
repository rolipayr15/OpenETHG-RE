using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class OldRadio : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CPlayAfterDelay_003Ed__5 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float delay;

		public OldRadio _003C_003E4__this;

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
		public _003CPlayAfterDelay_003Ed__5(int _003C_003E1__state)
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

	public float minDelay;

	public float maxDelay;

	private bool playerInside;

	private bool wasPlayed;

	private void OnTriggerEnter(Collider other)
	{
	}

	[IteratorStateMachine(typeof(_003CPlayAfterDelay_003Ed__5))]
	private IEnumerator PlayAfterDelay(float delay)
	{
		return null;
	}
}

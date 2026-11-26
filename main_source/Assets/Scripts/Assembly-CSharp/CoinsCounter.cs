using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class CoinsCounter : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCoinsCounterCoroune_003Ed__5 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public int end;

		public CoinsCounter _003C_003E4__this;

		public int start;

		private int _003CnumberOfSteps_003E5__2;

		private int _003Ci_003E5__3;

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
		public _003CCoinsCounterCoroune_003Ed__5(int _003C_003E1__state)
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

	private static int start;

	private static int end;

	[SerializeField]
	private Text coinsLabel;

	public static void Show(int startValue, int endValue)
	{
	}

	private void Start()
	{
	}

	[IteratorStateMachine(typeof(_003CCoinsCounterCoroune_003Ed__5))]
	private IEnumerator CoinsCounterCoroune(int start, int end)
	{
		return null;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class FreeCoinsButton : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CTimer_003Ed__10 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public FreeCoinsButton _003C_003E4__this;

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
		public _003CTimer_003Ed__10(int _003C_003E1__state)
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

	[SerializeField]
	private Text timeLeftLabel;

	[SerializeField]
	private Text buttonLabel;

	[SerializeField]
	private Button buttonWatchAd;

	private Animator animator;

	[SerializeField]
	private GameObject activeButtonGroup;

	[SerializeField]
	private GameObject deactivatedButtonGroup;

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	private void ChangeAdReadyStatus(bool isReady)
	{
	}

	private void StartTimer()
	{
	}

	[IteratorStateMachine(typeof(_003CTimer_003Ed__10))]
	private IEnumerator Timer()
	{
		return null;
	}

	public void ShowAd()
	{
	}
}

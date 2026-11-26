using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class RotateNameExample : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CChName_003Ed__10 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public RotateNameExample _003C_003E4__this;

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
		public _003CChName_003Ed__10(int _003C_003E1__state)
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

	[Header("Normal English Names")]
	public bool femaleOld;

	public bool femaleNew;

	public bool maleOld;

	public bool maleNew;

	[Header("Fantastic names")]
	public bool scifi;

	public bool femaleFantasy;

	public bool maleFantasy;

	private Text curName;

	private int typeName;

	private void Awake()
	{
	}

	[IteratorStateMachine(typeof(_003CChName_003Ed__10))]
	private IEnumerator ChName()
	{
		return null;
	}
}

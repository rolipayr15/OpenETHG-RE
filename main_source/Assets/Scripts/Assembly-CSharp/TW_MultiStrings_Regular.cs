using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TW_MultiStrings_Regular : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CMakeTypewriterText_003Ed__18 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TW_MultiStrings_Regular _003C_003E4__this;

		public string ORIGINAL;

		public string POINTER;

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
		public _003CMakeTypewriterText_003Ed__18(int _003C_003E1__state)
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

	[CompilerGenerated]
	private sealed class _003CMakeTypewriterTextWithNewLine_003Ed__19 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TW_MultiStrings_Regular _003C_003E4__this;

		public string ORIGINAL;

		public string POINTER;

		public List<int> List;

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
		public _003CMakeTypewriterTextWithNewLine_003Ed__19(int _003C_003E1__state)
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

	public bool LaunchOnStart;

	public int timeOut;

	public string[] MultiStrings;

	[HideInInspector]
	public int pointer;

	public string ORIGINAL_TEXT;

	private float time;

	private int сharIndex;

	private int index_of_string;

	private bool start;

	private List<int> n_l_list;

	private static string[] PointerSymbols;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void StartTypewriter()
	{
	}

	public void SkipTypewriter()
	{
	}

	public void NextString()
	{
	}

	public void LastString()
	{
	}

	private void NewLineCheck(string S)
	{
	}

	[IteratorStateMachine(typeof(_003CMakeTypewriterText_003Ed__18))]
	private IEnumerator MakeTypewriterText(string ORIGINAL, string POINTER)
	{
		return null;
	}

	[IteratorStateMachine(typeof(_003CMakeTypewriterTextWithNewLine_003Ed__19))]
	private IEnumerator MakeTypewriterTextWithNewLine(string ORIGINAL, string POINTER, List<int> List)
	{
		return null;
	}

	private List<int> MakeList(string S)
	{
		return null;
	}

	private string InsertNewLine(string _TEXT, List<int> _List)
	{
		return null;
	}

	private string GetPointerSymbol()
	{
		return null;
	}

	private void CharIndexPlus()
	{
	}
}

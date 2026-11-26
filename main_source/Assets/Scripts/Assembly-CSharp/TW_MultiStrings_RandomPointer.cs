using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TW_MultiStrings_RandomPointer : MonoBehaviour
{
	public enum RandomCharsType
	{
		LowerCase = 0,
		UpperCase = 1,
		LowerUpperCase = 2,
		Digits = 3,
		Symbols = 4,
		All = 5
	}

	[CompilerGenerated]
	private sealed class _003CMakeRandomTypewriterText_003Ed__25 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TW_MultiStrings_RandomPointer _003C_003E4__this;

		public string ORIGINAL;

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
		public _003CMakeRandomTypewriterText_003Ed__25(int _003C_003E1__state)
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
	private sealed class _003CMakeRandomTypewriterTextWithNewLine_003Ed__26 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TW_MultiStrings_RandomPointer _003C_003E4__this;

		public string ORIGINAL;

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
		public _003CMakeRandomTypewriterTextWithNewLine_003Ed__26(int _003C_003E1__state)
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

	public RandomCharsType Charstype;

	public string[] MultiStrings;

	public string ORIGINAL_TEXT;

	private float time;

	private int сharIndex;

	private int index_of_string;

	private bool start;

	private List<int> n_l_list;

	private static System.Random random;

	private static string lowerCase;

	private static string upperCase;

	private static string lowerupperCase;

	private static string digits;

	private static string symbols;

	private static string all;

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

	[IteratorStateMachine(typeof(_003CMakeRandomTypewriterText_003Ed__25))]
	private IEnumerator MakeRandomTypewriterText(string ORIGINAL)
	{
		return null;
	}

	[IteratorStateMachine(typeof(_003CMakeRandomTypewriterTextWithNewLine_003Ed__26))]
	private IEnumerator MakeRandomTypewriterTextWithNewLine(string ORIGINAL, List<int> List)
	{
		return null;
	}

	private string RandomChar(string ORIGINAL, int currentCharIndex)
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

	private string GetCharsType(RandomCharsType T)
	{
		return null;
	}

	private void CharIndexPlus()
	{
	}
}

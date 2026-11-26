using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Eyes.DailyChallenges;
using Google.Play.Common;
using Google.Play.Review;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ReviewPromptPresenter : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CPresentWhenQuiet_003Ed__24 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ReviewPromptPresenter _003C_003E4__this;

		private double _003Cdeadline_003E5__2;

		private ReviewManager _003Cmanager_003E5__3;

		private PlayAsyncOperation<PlayReviewInfo, ReviewErrorCode> _003Crequest_003E5__4;

		private PlayAsyncOperation<VoidResult, ReviewErrorCode> _003Claunch_003E5__5;

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
		public _003CPresentWhenQuiet_003Ed__24(int _003C_003E1__state)
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

	private readonly ReviewPromptOpportunity opportunity;

	private readonly HashSet<int> entryTouches;

	private Scene resultsScene;

	private ExpTable[] experience;

	private WinCoinsSummary[] coins;

	private WinResultShare[] shares;

	private DailyChallengeResultView[] challenges;

	private AdsManager ads;

	private string version;

	private long attemptTimestamp;

	private bool presented;

	private bool waitingForEntryRelease;

	private int entryFrame;

	public static void Schedule(Score owner)
	{
	}

	private void Initialize()
	{
	}

	private T[] InResultsScene<T>() where T : Component
	{
		return null;
	}

	private bool ResultsReady()
	{
		return false;
	}

	private bool CanContinue(bool acceptingSoftAsk = false)
	{
		return false;
	}

	private static bool HasInput()
	{
		return false;
	}

	private bool HasNewInput()
	{
		return false;
	}

	private void LateUpdate()
	{
	}

	private void OnApplicationPause(bool paused)
	{
	}

	private void OnApplicationFocus(bool focused)
	{
	}

	private void OnDisable()
	{
	}

	[IteratorStateMachine(typeof(_003CPresentWhenQuiet_003Ed__24))]
	private IEnumerator PresentWhenQuiet()
	{
		return null;
	}

	public bool TryAcceptSoftAsk()
	{
		return false;
	}

	private static void ReportError(string error)
	{
	}
}

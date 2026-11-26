using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PushNotificationRewardPresenter : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CShowClaimRetryWhenReady_003Ed__24 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PushNotificationRewardPresenter _003C_003E4__this;

		private WaitForSecondsRealtime _003CretryDelay_003E5__2;

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
		public _003CShowClaimRetryWhenReady_003Ed__24(int _003C_003E1__state)
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
	private sealed class _003CShowReplacementRewardWhenReady_003Ed__23 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PushNotificationRewardPresenter _003C_003E4__this;

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
		public _003CShowReplacementRewardWhenReady_003Ed__23(int _003C_003E1__state)
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
	private sealed class _003CShowWhenMenuIsReady_003Ed__17 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PushNotificationRewardPresenter _003C_003E4__this;

		private WaitForSecondsRealtime _003CretryDelay_003E5__2;

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
		public _003CShowWhenMenuIsReady_003Ed__17(int _003C_003E1__state)
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

	internal const string ClaimClickedEventName = "push_reward_claim_clicked";

	internal const string ClaimedEventName = "push_reward_claimed";

	private const int MaximumAnalyticsStringLength = 100;

	private const float RetryDelaySeconds = 0.5f;

	private static PushNotificationRewardPresenter instance;

	private PushNotificationRewardHandler rewardHandler;

	private Coroutine showCoroutine;

	private bool windowVisible;

	private bool dismissedDuringCurrentMenuVisit;

	public static void Initialize(PushNotificationRewardHandler handler)
	{
	}

	public static void NotifyPendingRewardAvailable()
	{
	}

	private void Awake()
	{
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void OnDestroy()
	{
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode _)
	{
	}

	private void RequestShow()
	{
	}

	[IteratorStateMachine(typeof(_003CShowWhenMenuIsReady_003Ed__17))]
	private IEnumerator ShowWhenMenuIsReady()
	{
		return null;
	}

	private void OnClaimSelected(PushNotificationReward presentedReward)
	{
	}

	private void HandleClaimResult(PushNotificationRewardClaimResult result, PushNotificationReward reward)
	{
	}

	private static void SelectRewardedChapter(PushNotificationReward reward)
	{
	}

	private void RequestClaimRetry()
	{
	}

	private void RequestReplacementReward()
	{
	}

	[IteratorStateMachine(typeof(_003CShowReplacementRewardWhenReady_003Ed__23))]
	private IEnumerator ShowReplacementRewardWhenReady()
	{
		return null;
	}

	[IteratorStateMachine(typeof(_003CShowClaimRetryWhenReady_003Ed__24))]
	private IEnumerator ShowClaimRetryWhenReady()
	{
		return null;
	}

	private void OnClaimRetryDeferred(PushNotificationReward presentedReward)
	{
	}

	private void OnRewardWindowClosed()
	{
	}

	private static bool IsTerminalPresentation(PushNotificationPendingPresentation presentation)
	{
		return false;
	}

	internal static PushNotificationClaimFollowUp GetClaimFollowUp(PushNotificationRewardClaimResult result)
	{
		return PushNotificationClaimFollowUp.Complete;
	}

	internal static string GetRetryMessage()
	{
		return null;
	}

	internal static string GetRewardMessage(PushNotificationReward reward)
	{
		return null;
	}

	internal static PushNotificationRewardAnalyticsData GetRewardAnalyticsData(PushNotificationReward reward)
	{
		return default;
	}

	private static void SendFirebaseRewardEvent(string eventName, PushNotificationReward reward)
	{
	}

	private static void SendRewardAnalytics(PushNotificationReward reward)
	{
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PushNotificationPermissionPresenter : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass18_0
	{
		public PushNotificationPermissionPresenter _003C_003E4__this;

		public Scene menu;

		public int version;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass18_1
	{
		public Scene dialog;

		public bool resolved;

		public bool accepted;

		public _003C_003Ec__DisplayClass18_0 CS_0024_003C_003E8__locals1;

		internal void _003CShowAfterGame_003Eb__0()
		{
		}

		internal void _003CShowAfterGame_003Eb__1()
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003CShowAfterGame_003Ed__18 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PushNotificationPermissionPresenter _003C_003E4__this;

		public Scene menu;

		public int version;

		private _003C_003Ec__DisplayClass18_0 _003C_003E8__1;

		private _003C_003Ec__DisplayClass18_1 _003C_003E8__2;

		private WaitForSecondsRealtime _003Cretry_003E5__2;

		private Task<PushPermissionStatus> _003Cquery_003E5__3;

		private float _003Cdeadline_003E5__4;

		private MenuInputScope _003Cinput_003E5__5;

		private Action _003Cdecline_003E5__6;

		private Task _003Crequest_003E5__7;

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
		public _003CShowAfterGame_003Ed__18(int _003C_003E1__state)
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

		private void _003C_003Em__Finally1()
		{
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	internal const string DecisionKey = "push.permission.invitation.v1";

	internal const string Invitation = "Would you like to occasionally receive notifications about promotions and special events in the game?";

	private static PushNotificationPermissionPresenter instance;

	private PushNotificationPermissionPolicy policy;

	private bool paused;

	private bool focused;

	private int contextVersion;

	private MenuInputScope activeInputScope;

	public static void BeginGame(Component owner)
	{
	}

	public static void FinishGame(Component owner)
	{
	}

	public static void FinishMultiplayerGame(Component owner, GameFinishedReason reason)
	{
	}

	public static void CancelGame(Component owner)
	{
	}

	public static void SceneDestroyed(Component owner)
	{
	}

	private void Awake()
	{
	}

	private static bool SaveDecision(PushPermissionDecision decision)
	{
		return false;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
	}

	private bool IsCurrentMenu(Scene menu, int version)
	{
		return false;
	}

	private bool CanPresent(Scene menu)
	{
		return false;
	}

	[IteratorStateMachine(typeof(_003CShowAfterGame_003Ed__18))]
	private IEnumerator ShowAfterGame(Scene menu, int version)
	{
		return null;
	}

	private static string Localize(string term, string fallback)
	{
		return null;
	}

	private void OnApplicationPause(bool value)
	{
	}

	private void OnApplicationFocus(bool value)
	{
	}

	private void OnApplicationQuit()
	{
	}

	private void OnDisable()
	{
	}

	private void OnDestroy()
	{
	}
}

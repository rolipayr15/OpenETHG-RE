using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ExternalApi.Models;
using Reflex.Attributes;
using UnityEngine;

public class ExperienceManagerExampleScript : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CTestExperienceManager_003Ed__12 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ExperienceManagerExampleScript _003C_003E4__this;

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
		public _003CTestExperienceManager_003Ed__12(int _003C_003E1__state)
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

	[Inject]
	private IExperienceManager experienceManager;

	private int displayedExp;

	private int displayedLevel;

	private float expLerpSpeed;

	private bool showLevelUp;

	private float levelUpTimer;

	private string lastPlayerInfo;

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnGUI()
	{
	}

	[IteratorStateMachine(typeof(_003CTestExperienceManager_003Ed__12))]
	private IEnumerator TestExperienceManager()
	{
		return null;
	}

	private void OnExpGained(GrantExpData expContext, bool isOverwrite)
	{
	}

	private void OnLevelUp(int oldLevel, int newLevel)
	{
	}

	private string PlayerInfo(int expOverride = -1, int levelOverride = -1)
	{
		return null;
	}
}

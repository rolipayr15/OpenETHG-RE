using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class HospitalLegJumpscare : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CRotateOverTime_003Ed__2 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public HospitalLegJumpscare _003C_003E4__this;

		public float angle;

		public float duration;

		private Quaternion _003CstartRotation_003E5__2;

		private Quaternion _003CendRotation_003E5__3;

		private float _003Ctime_003E5__4;

		private float _003CinverseDuration_003E5__5;

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
		public _003CRotateOverTime_003Ed__2(int _003C_003E1__state)
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
	private Transform legToRotate;

	private void OnTriggerEnter(Collider other)
	{
	}

	[IteratorStateMachine(typeof(_003CRotateOverTime_003Ed__2))]
	private IEnumerator RotateOverTime(float angle, float duration)
	{
		return null;
	}
}

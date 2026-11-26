using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CeilingFanController : MonoBehaviour
{
	public enum FanMode
	{
		TriggerSometimesGentle = 0,
		TriggerFast = 1,
		AlwaysSlow = 2,
		AlwaysSlowVariable = 3
	}

	[CompilerGenerated]
	private sealed class _003CSpinBurstSmoothStop_003Ed__30 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public CeilingFanController _003C_003E4__this;

		public float burstSpeed;

		public float spinTime;

		public float stopTime;

		private float _003Ct_003E5__2;

		private float _003CstartSpeed_003E5__3;

		private float _003Cs_003E5__4;

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
		public _003CSpinBurstSmoothStop_003Ed__30(int _003C_003E1__state)
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

	[Header("Mode")]
	public FanMode mode;

	public string playerTag;

	[Header("What rotates")]
	public Transform rotatingPart;

	public Vector3 rotationAxis;

	[Header("Speeds (degrees/sec)")]
	public float slowSpeed;

	public float gentleBurstSpeed;

	public float fastBurstSpeed;

	[Header("Trigger settings")]
	[Range(0f, 1f)]
	public float gentleChance;

	public float gentleDuration;

	public float fastDuration;

	[Header("Variable speed settings (mode 4)")]
	public float variableMin;

	public float variableMax;

	public float variableChangeSpeed;

	private float currentSpeed;

	private Coroutine burstCo;

	public float gentleSpinTime;

	public float gentleStopTime;

	public float fastSpinTime;

	public float fastStopTime;

	[Header("Random Mode On Awake")]
	public bool randomizeModeOnAwake;

	[Range(0f, 1f)]
	public float weightTriggerGentle;

	[Range(0f, 1f)]
	public float weightTriggerFast;

	[Range(0f, 1f)]
	public float weightAlwaysSlow;

	[Range(0f, 1f)]
	public float weightAlwaysVariable;

	private bool wasTriggered;

	private void Awake()
	{
	}

	private void RandomizeMode()
	{
	}

	private void Update()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
	}

	[IteratorStateMachine(typeof(_003CSpinBurstSmoothStop_003Ed__30))]
	private IEnumerator SpinBurstSmoothStop(float burstSpeed, float spinTime, float stopTime)
	{
		return null;
	}
}

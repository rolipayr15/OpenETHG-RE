using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(ControlModeManager))]
public sealed class TouchInputRecovery : MonoBehaviour
{
	private ControlModeManager controls;

	private int? trackedFinger;

	private uint trackedRevision;

	private uint neutralRevision;

	private bool observedNeutral;

	private EventSystem pointerSystem;

	private PointerEventData pointerData;

	private readonly List<RaycastResult> hits = new List<RaycastResult>();

	private string lastRejection;

	private void Awake()
	{
		controls = GetComponent<ControlModeManager>();
	}

	private void OnDisable()
	{
		CancelGesture();
	}

	private void LateUpdate()
	{
		if (controls == null || controls.Mode != ControlMode.Touch || !controls.RequiresExplicitResume)
		{
			CancelGesture();
			return;
		}
		int touchCount = Input.touchCount;
		if (touchCount == 1)
		{
			Touch touch = Input.GetTouch(0);
			ProcessTouchSample(touch.fingerId, touch.phase, 1, IsOverUI(touch.position));
		}
		else
		{
			ProcessTouchSample(0, TouchPhase.Stationary, touchCount, false);
		}
	}

	public void CancelGesture()
	{
		trackedFinger = null;
		observedNeutral = false;
	}

	private string BlockingReason(TouchPhase phase, int touchCount, bool pointerOverUI)
	{
		if (!isActiveAndEnabled || controls == null || !controls.isActiveAndEnabled)
		{
			return "inactive";
		}
		if (touchCount >= 2)
		{
			return "touchCount";
		}
		if (pointerOverUI)
		{
			return "overUI";
		}
		if (phase == TouchPhase.Canceled)
		{
			return "canceled";
		}
		if (controls.Mode != ControlMode.Touch)
		{
			return "notTouch";
		}
		if (!controls.HasFocus)
		{
			return "noFocus";
		}
		if (!controls.RequiresExplicitResume)
		{
			return "noRecovery";
		}
		if (!controls.SelectedSourceAvailable)
		{
			return "noSource";
		}
		return FullscreenAdSessions.IsShowing ? "adShowing" : null;
	}

	private bool Reject(string reason)
	{
		CancelGesture();
		return false;
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("EYES_INPUT_DIAGNOSTICS")]
	private void TraceRejection(string reason)
	{
		if (reason != lastRejection)
		{
			lastRejection = reason;
		}
	}

	public bool ProcessTouchSample(int fingerId, TouchPhase phase, int touchCount, bool pointerOverUI)
	{
		string reason = BlockingReason(phase, touchCount, pointerOverUI);
		if (reason != null)
		{
			return Reject(reason);
		}
		GameplayInputContext policy = GameplayInputContext.Shared;
		GameplayInputContext.Permissions state = policy.Current;
		if (state.TransitionBarrier)
		{
			return Reject("barrier");
		}
		if (!controls.OwnsRecoveryLease(state.TopOwner) || !policy.IsGameplayExcept(state.TopOwner))
		{
			return Reject("otherOwner");
		}
		if (touchCount == 0)
		{
			trackedFinger = null;
			observedNeutral = true;
			neutralRevision = state.Revision;
			return false;
		}
		if (phase == TouchPhase.Began)
		{
			if (observedNeutral && neutralRevision == state.Revision)
			{
				observedNeutral = false;
				trackedFinger = fingerId;
				trackedRevision = state.Revision;
				return false;
			}
			return Reject("notNeutral");
		}
		if (trackedFinger != fingerId || trackedRevision != state.Revision)
		{
			return Reject("untracked");
		}
		if (phase != TouchPhase.Ended)
		{
			return false;
		}
		CancelGesture();
		return controls.TryResumeExplicitly();
	}

	private bool IsOverUI(Vector2 position)
	{
		EventSystem eventSystem = EventSystem.current;
		if (eventSystem == null || !eventSystem.isActiveAndEnabled)
		{
			return false;
		}
		if (pointerData == null || pointerSystem != eventSystem)
		{
			pointerSystem = eventSystem;
			pointerData = new PointerEventData(eventSystem);
		}
		pointerData.Reset();
		pointerData.position = position;
		eventSystem.RaycastAll(pointerData, hits);
		foreach (RaycastResult hit in hits)
		{
			if (hit.module is UnityEngine.UI.GraphicRaycaster && hit.gameObject != null && !InteractionManager.IsGameplayTouchGraphic(hit.gameObject, controls.Mode))
			{
				return true;
			}
		}
		return false;
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class vp_StateEventHandler : vp_EventHandler
{
	private List<vp_Component> m_StateTargets = new List<vp_Component>();

	private List<vp_Activity> m_Activities = new List<vp_Activity>();

	protected override void Awake()
	{
		base.Awake();
		StoreStateTargets();
		StoreActivities();
	}

	protected void BindStateToActivity(vp_Activity a)
	{
		BindStateToActivityOnStart(a);
		BindStateToActivityOnStop(a);
	}

	protected void BindStateToActivityOnStart(vp_Activity a)
	{
		if (!ActivityInitialized(a))
		{
			return;
		}
		string s = a.EventName;
		a.StartCallbacks = (vp_Activity.Callback)Delegate.Combine(a.StartCallbacks, (vp_Activity.Callback)delegate
		{
			foreach (vp_Component stateTarget in m_StateTargets)
			{
				stateTarget.SetState(s, enabled: true, recursive: true);
			}
		});
	}

	protected void BindStateToActivityOnStop(vp_Activity a)
	{
		if (!ActivityInitialized(a))
		{
			return;
		}
		string s = a.EventName;
		a.StopCallbacks = (vp_Activity.Callback)Delegate.Combine(a.StopCallbacks, (vp_Activity.Callback)delegate
		{
			foreach (vp_Component stateTarget in m_StateTargets)
			{
				stateTarget.SetState(s, enabled: false, recursive: true);
			}
		});
	}

	protected void StoreStateTargets()
	{
		vp_Component[] componentsInChildren = base.transform.root.GetComponentsInChildren<vp_Component>(includeInactive: true);
		foreach (vp_Component vp_Component2 in componentsInChildren)
		{
			if (vp_Component2.Parent == null || vp_Component2.Parent.GetComponent<vp_Component>() == null)
			{
				m_StateTargets.Add(vp_Component2);
			}
		}
	}

	protected void StoreActivities()
	{
		for (int i = 0; i < m_Events.Count; i++)
		{
			if (m_Events[i] is vp_Activity || m_Events[i].Type.BaseType == typeof(vp_Activity))
			{
				m_Activities.Add(m_Events[i] as vp_Activity);
			}
		}
	}

	public void RefreshActivityStates()
	{
		Dictionary<Transform, bool> dictionary = new Dictionary<Transform, bool>();
		for (int i = 0; i < m_Activities.Count; i++)
		{
			dictionary.Clear();
			for (int j = 0; j < m_StateTargets.Count; j++)
			{
				bool flag = dictionary.ContainsKey(m_StateTargets[j].transform);
				m_StateTargets[j].SetState(m_Activities[i].EventName, m_Activities[i].Active, !flag);
				if (!flag)
				{
					dictionary.Add(m_StateTargets[j].transform, value: true);
				}
			}
		}
	}

	public void ResetActivityStates()
	{
		foreach (vp_Component stateTarget in m_StateTargets)
		{
			stateTarget.ResetState();
		}
	}

	public void SetState(string state, bool setActive = true, bool recursive = true, bool includeDisabled = false)
	{
		foreach (vp_Component stateTarget in m_StateTargets)
		{
			stateTarget.SetState(state, setActive, recursive, includeDisabled);
		}
	}

	private bool ActivityInitialized(vp_Activity a)
	{
		if (a == null)
		{
			Debug.LogError("Error: (" + this?.ToString() + ") Activity is null.");
			return false;
		}
		if (string.IsNullOrEmpty(a.EventName))
		{
			Debug.LogError("Error: (" + this?.ToString() + ") Activity not initialized. Make sure the event handler has run its Awake call before binding layers.");
			return false;
		}
		return true;
	}
}

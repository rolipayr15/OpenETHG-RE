using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class vp_UIControl : MonoBehaviour
{
	[HideInInspector]
	public vp_UIManager Manager;

	public vp_TouchButtonEventBinding PressControl;

	public vp_TouchButtonEventBinding HoldControl;

	public vp_TouchButtonEventBinding ReleaseControl;

	public vp_TouchButtonEventBinding DoublePressControl;

	public vp_TouchButtonEventBinding DoubleReleaseControl;

	public vp_TouchChangedEventBinding ChangeControl;

	[HideInInspector]
	public List<MonoBehaviour> Objects = new List<MonoBehaviour>();

	[HideInInspector]
	public List<int> MethodIndexes = new List<int>();

	[HideInInspector]
	public List<string> Methods = new List<string>();

	protected List<Delegate> m_ChangedEvents = new List<Delegate>();

	protected List<Delegate> m_ButtonEvents = new List<Delegate>();

	protected Transform m_Transform;

	protected Camera m_Camera;

	protected BoxCollider m_Collider;

	protected Transform m_Parent;

	protected bool m_Initialized;

	protected float m_DoubleTapTime;

	protected float m_TapCount;

	protected int m_LastFingerID;

	public int LastFingerID
	{
		get
		{
			return m_LastFingerID;
		}
		set
		{
			if (Manager != null)
			{
				if (value == -1)
				{
					if (Manager.FingerIDs.ContainsKey(m_LastFingerID))
					{
						if (Manager.FingerIDs[m_LastFingerID].Count == 1)
						{
							Manager.FingerIDs.Remove(m_LastFingerID);
						}
						else if (Manager.FingerIDs[m_LastFingerID].Contains(this))
						{
							Manager.FingerIDs[m_LastFingerID].Remove(this);
						}
					}
				}
				else if (!Manager.FingerIDs.ContainsKey(value))
				{
					Manager.FingerIDs.Add(value, new List<vp_UIControl> { this });
				}
				else if (Manager.FingerIDs.ContainsKey(value) && !Manager.FingerIDs[value].Contains(this))
				{
					Manager.FingerIDs[value].Add(this);
				}
			}
			m_LastFingerID = value;
		}
	}

	protected virtual void Awake()
	{
		m_Transform = base.transform;
		m_Camera = ((vp_UIManager.GetUICamera(m_Transform) == null) ? m_Transform.root.GetComponentInChildren<Camera>() : vp_UIManager.GetUICamera(m_Transform));
		m_Parent = m_Transform.parent;
		Manager = m_Transform.root.GetComponentInChildren<vp_UIManager>();
		m_LastFingerID = -1;
		if (GetComponent<Collider>() != null)
		{
			m_Collider = GetComponents<BoxCollider>().FirstOrDefault();
		}
		List<string> list = new List<string> { "OnPressControl", "OnHoldControl", "OnReleaseControl", "OnDoublePressControl", "OnDoubleReleaseControl" };
		List<string> list2 = new List<string> { "OnChangeControl" };
		List<MonoBehaviour> list3 = GetComponents<MonoBehaviour>().ToList();
		for (int i = 0; i < list3.Count; i++)
		{
			MonoBehaviour monoBehaviour = list3[i];
			MethodInfo[] methods = monoBehaviour.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.GetParameters().Length == 0 && methodInfo.ReturnType == typeof(void))
				{
					foreach (string item in list)
					{
						if (methodInfo.Name == item)
						{
							m_ButtonEvents.Add((vp_TouchButtonEventBinding)Delegate.CreateDelegate(typeof(vp_TouchButtonEventBinding), monoBehaviour, item));
						}
					}
				}
				else
				{
					if (methodInfo.GetParameters().Length != 1 || !(methodInfo.GetParameters()[0].ParameterType == typeof(vp_UIControl)) || !(methodInfo.ReturnType == typeof(void)))
					{
						continue;
					}
					foreach (string item2 in list2)
					{
						if (methodInfo.Name == item2)
						{
							m_ChangedEvents.Add((vp_TouchChangedEventBinding)Delegate.CreateDelegate(typeof(vp_TouchChangedEventBinding), monoBehaviour, item2));
						}
					}
				}
			}
		}
		for (int k = 0; k < Objects.Count; k++)
		{
			if (!(Objects[k] == null) && !(Methods[k] == ""))
			{
				string[] array = Methods[k].Split(new string[1] { "." }, StringSplitOptions.None);
				Component component = Objects[k].GetComponent(array[0]);
				if (component.GetType().GetMethod(array[1]).GetParameters()
					.Length != 0)
				{
					m_ChangedEvents.Add((vp_TouchChangedEventBinding)Delegate.CreateDelegate(typeof(vp_TouchChangedEventBinding), component, array[1]));
				}
				else if (component.GetType().GetMethod(array[1]).GetParameters()
					.Length == 0)
				{
					m_ButtonEvents.Add((vp_TouchButtonEventBinding)Delegate.CreateDelegate(typeof(vp_TouchButtonEventBinding), component, array[1]));
				}
			}
		}
	}

	protected virtual void Start()
	{
		m_Initialized = true;
		OnEnable();
	}

	protected virtual void OnEnable()
	{
		if (!m_Initialized)
		{
			return;
		}
		if (Manager != null && Manager.Player != null)
		{
			Manager.Player.Register(this);
		}
		if (Manager != null)
		{
			Manager.RegisterControl(this);
		}
		else
		{
			vp_InputMobile.Register(this);
		}
		foreach (vp_TouchButtonEventBinding buttonEvent in m_ButtonEvents)
		{
			if (buttonEvent.Method.Name == "OnPressControl")
			{
				PressControl = (vp_TouchButtonEventBinding)Delegate.Combine(PressControl, buttonEvent);
			}
			else if (buttonEvent.Method.Name == "OnHoldControl")
			{
				HoldControl = (vp_TouchButtonEventBinding)Delegate.Combine(HoldControl, buttonEvent);
			}
			else if (buttonEvent.Method.Name == "OnDoublePressControl")
			{
				DoublePressControl = (vp_TouchButtonEventBinding)Delegate.Combine(DoublePressControl, buttonEvent);
			}
			else if (buttonEvent.Method.Name == "OnDoubleReleaseControl")
			{
				DoubleReleaseControl = (vp_TouchButtonEventBinding)Delegate.Combine(DoubleReleaseControl, buttonEvent);
			}
			else
			{
				ReleaseControl = (vp_TouchButtonEventBinding)Delegate.Combine(ReleaseControl, buttonEvent);
			}
		}
		foreach (vp_TouchChangedEventBinding changedEvent in m_ChangedEvents)
		{
			ChangeControl = (vp_TouchChangedEventBinding)Delegate.Combine(ChangeControl, changedEvent);
		}
	}

	protected virtual void OnDisable()
	{
		if (Manager != null && Manager.Player != null)
		{
			Manager.Player.Unregister(this);
		}
		if (Manager != null)
		{
			Manager.UnregisterControl(this);
		}
		else
		{
			vp_InputMobile.Unregister(this);
		}
		foreach (vp_TouchButtonEventBinding buttonEvent in m_ButtonEvents)
		{
			if (buttonEvent.Method.Name == "OnPressControl")
			{
				PressControl = (vp_TouchButtonEventBinding)Delegate.Remove(PressControl, buttonEvent);
			}
			else if (buttonEvent.Method.Name == "OnHoldControl")
			{
				HoldControl = (vp_TouchButtonEventBinding)Delegate.Remove(HoldControl, buttonEvent);
			}
			else if (buttonEvent.Method.Name == "OnDoublePressControl")
			{
				DoublePressControl = (vp_TouchButtonEventBinding)Delegate.Remove(DoublePressControl, buttonEvent);
			}
			else if (buttonEvent.Method.Name == "OnDoubleReleaseControl")
			{
				DoubleReleaseControl = (vp_TouchButtonEventBinding)Delegate.Remove(DoubleReleaseControl, buttonEvent);
			}
			else
			{
				ReleaseControl = (vp_TouchButtonEventBinding)Delegate.Remove(ReleaseControl, buttonEvent);
			}
		}
		foreach (vp_TouchChangedEventBinding changedEvent in m_ChangedEvents)
		{
			ChangeControl = (vp_TouchChangedEventBinding)Delegate.Remove(ChangeControl, changedEvent);
		}
	}

	protected virtual void Update()
	{
		if (LastFingerID != -1 && HoldControl != null)
		{
			HoldControl();
		}
	}

	protected virtual void TouchesBegan(vp_Touch touch)
	{
		if (!(m_Collider == null) && LastFingerID == -1 && RaycastControl(touch))
		{
			if (PressControl != null)
			{
				PressControl();
			}
			if (Time.time > m_DoubleTapTime)
			{
				m_TapCount = 0f;
			}
			if (m_TapCount == 0f)
			{
				m_DoubleTapTime = Time.time + ((Manager != null) ? Manager.DoubleTapTimeout : 0.25f);
			}
			m_TapCount += 1f;
			if (m_TapCount == 2f && DoublePressControl != null)
			{
				DoublePressControl();
			}
			LastFingerID = touch.FingerID;
		}
	}

	public virtual void TouchesFinished(vp_Touch touch)
	{
		if (LastFingerID == touch.FingerID)
		{
			if (ReleaseControl != null)
			{
				ReleaseControl();
			}
			if (m_TapCount == 2f && DoubleReleaseControl != null)
			{
				DoubleReleaseControl();
			}
			LastFingerID = -1;
		}
	}

	public virtual bool RaycastControl(vp_Touch touch)
	{
		return (from hit in Physics.RaycastAll(m_Camera.ScreenPointToRay(touch.Position))
			where hit.collider == m_Collider
			select hit).ToList().Count > 0;
	}
}

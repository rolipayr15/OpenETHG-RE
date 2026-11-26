using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class vp_InputMobile : vp_Input
{
	public static Dictionary<string, vp_TouchButtonActionEventHandler> ButtonDownCallbacks = new Dictionary<string, vp_TouchButtonActionEventHandler>();

	public static Dictionary<string, vp_TouchButtonActionEventHandler> ButtonUpCallbacks = new Dictionary<string, vp_TouchButtonActionEventHandler>();

	public static Dictionary<string, vp_TouchButtonActionEventHandler> ButtonHoldCallbacks = new Dictionary<string, vp_TouchButtonActionEventHandler>();

	protected static Dictionary<string, vp_TouchesEventHandler> TouchEventBindings = new Dictionary<string, vp_TouchesEventHandler>
	{
		{ "TouchesBegan", null },
		{ "TouchesMoved", null },
		{ "TouchesStationary", null },
		{ "TouchesEnded", null },
		{ "TouchesCancelled", null },
		{ "TouchesFinished", null }
	};

	protected static Dictionary<string, vp_TouchesEventHandler> TouchesDelegates = new Dictionary<string, vp_TouchesEventHandler>();

	public static vp_RawAxisEventHandler LookAxisCallback;

	public static vp_RawAxisEventHandler MoveAxisCallback;

	protected override void Awake()
	{
		GameObject gameObject = Resources.Load("Input/vp_Input") as GameObject;
		vp_Input vp_Input2 = ((gameObject != null) ? gameObject.GetComponent<vp_Input>() : new GameObject("vp_Input").AddComponent<vp_Input>());
		ControlType = vp_Input2.ControlType;
		Buttons = vp_Input2.Buttons;
		Axis = vp_Input2.Axis;
		UnityAxis = vp_Input2.UnityAxis;
		ButtonKeys = vp_Input2.ButtonKeys;
		ButtonValues = vp_Input2.ButtonValues;
		AxisKeys = vp_Input2.AxisKeys;
		AxisValues = vp_Input2.AxisValues;
		vp_Input.m_Instance = this;
		SetupDefaults();
		Application.targetFrameRate = 60;
	}

	public override void SetupDefaults(string type = "")
	{
		base.SetupDefaults(type);
		foreach (string key in Buttons.Keys)
		{
			if (!ButtonDownCallbacks.ContainsKey(key))
			{
				ButtonDownCallbacks.Add(key, null);
			}
			if (!ButtonHoldCallbacks.ContainsKey(key))
			{
				ButtonHoldCallbacks.Add(key, null);
			}
			if (!ButtonUpCallbacks.ContainsKey(key))
			{
				ButtonUpCallbacks.Add(key, null);
			}
		}
	}

	public override bool DoGetButton(string id)
	{
		if (Input.touchCount == 0 && !vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse"))
		{
			return base.DoGetButton(id);
		}
		if (ButtonHoldCallbacks.ContainsKey(id) && ButtonHoldCallbacks[id] != null)
		{
			Delegate[] invocationList = ButtonHoldCallbacks[id].GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				if (((vp_TouchButtonActionEventHandler)invocationList[i])(id, hold: true))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool DoGetButtonDown(string id)
	{
		if (Input.touchCount == 0 && !vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse"))
		{
			return base.DoGetButtonDown(id);
		}
		if (ButtonDownCallbacks.ContainsKey(id) && ButtonDownCallbacks[id] != null)
		{
			Delegate[] invocationList = ButtonDownCallbacks[id].GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				if (((vp_TouchButtonActionEventHandler)invocationList[i])(id))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool DoGetButtonUp(string id)
	{
		if (Input.touchCount == 0 && !vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse"))
		{
			return base.DoGetButtonUp(id);
		}
		if (ButtonUpCallbacks.ContainsKey(id) && ButtonUpCallbacks[id] != null)
		{
			Delegate[] invocationList = ButtonUpCallbacks[id].GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				if (((vp_TouchButtonActionEventHandler)invocationList[i])(id))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool DoGetButtonAny(string id)
	{
		if (!DoGetButtonDown(id) && !DoGetButton(id))
		{
			return DoGetButtonUp(id);
		}
		return true;
	}

	public override float DoGetAxisRaw(string id)
	{
		if (Input.touchCount == 0 && !vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse"))
		{
			return base.DoGetAxisRaw(id);
		}
		if (id == "Horizontal" || id == "Vertical")
		{
			if (MoveAxisCallback == null)
			{
				return 0f;
			}
			return MoveAxisCallback(id);
		}
		if (LookAxisCallback == null)
		{
			return 0f;
		}
		return LookAxisCallback(id);
	}

	protected virtual void Update()
	{
		InputMouse();
		InputTouches();
	}

	protected virtual void InputMouse()
	{
		if (!Application.isEditor || (!Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonUp(0)))
		{
			return;
		}
		vp_Touch vp_Touch2 = new vp_Touch();
		vp_Touch2.FingerID = 0;
		vp_Touch2.Position = Input.mousePosition;
		vp_Touch2.DeltaPosition = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 4f;
		if (Input.GetMouseButtonDown(0) && TouchEventBindings["TouchesBegan"] != null)
		{
			TouchEventBindings["TouchesBegan"](vp_Touch2);
		}
		if (Input.GetMouseButton(0) && TouchEventBindings["TouchesMoved"] != null)
		{
			TouchEventBindings["TouchesMoved"](vp_Touch2);
		}
		if (vp_Touch2.DeltaPosition == Vector2.zero && Input.GetMouseButton(0) && TouchEventBindings["TouchesStationary"] != null)
		{
			TouchEventBindings["TouchesStationary"](vp_Touch2);
		}
		if (Input.GetMouseButtonUp(0))
		{
			if (TouchEventBindings["TouchesEnded"] != null)
			{
				TouchEventBindings["TouchesEnded"](vp_Touch2);
			}
			if (TouchEventBindings["TouchesFinished"] != null)
			{
				TouchEventBindings["TouchesFinished"](vp_Touch2);
			}
		}
	}

	protected virtual void InputTouches()
	{
		Touch[] touches = Input.touches;
		for (int i = 0; i < touches.Length; i++)
		{
			Touch touch = touches[i];
			vp_Touch vp_Touch2 = new vp_Touch();
			vp_Touch2.FingerID = touch.fingerId;
			vp_Touch2.Position = touch.position;
			vp_Touch2.DeltaPosition = touch.deltaPosition;
			if (touch.phase != TouchPhase.Canceled && touch.phase == TouchPhase.Began && TouchEventBindings["TouchesBegan"] != null)
			{
				TouchEventBindings["TouchesBegan"](vp_Touch2);
			}
			if (touch.phase == TouchPhase.Moved && TouchEventBindings["TouchesMoved"] != null)
			{
				TouchEventBindings["TouchesMoved"](vp_Touch2);
			}
			if (touch.phase == TouchPhase.Stationary && TouchEventBindings["TouchesStationary"] != null)
			{
				TouchEventBindings["TouchesStationary"](vp_Touch2);
			}
			if (touch.phase == TouchPhase.Ended && TouchEventBindings["TouchesEnded"] != null)
			{
				TouchEventBindings["TouchesEnded"](vp_Touch2);
			}
			if (touch.phase == TouchPhase.Canceled && TouchEventBindings["TouchesCancelled"] != null)
			{
				TouchEventBindings["TouchesCancelled"](vp_Touch2);
			}
			if (touch.phase == TouchPhase.Ended || (touch.phase == TouchPhase.Canceled && TouchEventBindings["TouchesFinished"] != null))
			{
				TouchEventBindings["TouchesFinished"](vp_Touch2);
			}
		}
	}

	public static void Register(vp_UIControl control)
	{
		ControlRegistration(control, register: true);
	}

	public static void Unregister(vp_UIControl control)
	{
		ControlRegistration(control, register: false);
	}

	protected static void ControlRegistration(vp_UIControl control, bool register)
	{
		List<MonoBehaviour> list = control.GetComponents<MonoBehaviour>().ToList();
		for (int i = 0; i < list.Count; i++)
		{
			MonoBehaviour monoBehaviour = list[i];
			MethodInfo[] methods = monoBehaviour.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.GetParameters().Length == 1 && methodInfo.GetParameters().FirstOrDefault((ParameterInfo p) => p.ParameterType == typeof(vp_Touch)) != null && methodInfo.ReturnType == typeof(void) && TouchEventBindings.ContainsKey(methodInfo.Name))
				{
					string key = monoBehaviour.GetInstanceID() + "." + methodInfo.Name;
					vp_TouchesEventHandler value = null;
					if (!TouchesDelegates.TryGetValue(key, out value))
					{
						value = (vp_TouchesEventHandler)Delegate.CreateDelegate(typeof(vp_TouchesEventHandler), monoBehaviour, methodInfo.Name);
						TouchesDelegates.Add(key, value);
					}
					if (register)
					{
						Dictionary<string, vp_TouchesEventHandler> touchEventBindings = TouchEventBindings;
						string key2 = methodInfo.Name;
						touchEventBindings[key2] = (vp_TouchesEventHandler)Delegate.Combine(touchEventBindings[key2], value);
					}
					else
					{
						Dictionary<string, vp_TouchesEventHandler> touchEventBindings = TouchEventBindings;
						string key2 = methodInfo.Name;
						touchEventBindings[key2] = (vp_TouchesEventHandler)Delegate.Remove(touchEventBindings[key2], value);
					}
				}
			}
		}
	}
}

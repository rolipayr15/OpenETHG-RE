using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(vp_InputMobile))]
public class vp_UIManager : MonoBehaviour
{
	public int UILayer = 24;

	public vp_FPPlayerEventHandler Player;

	public AudioSource AudioSource;

	public Vector2 Resolution = new Vector2(1024f, 768f);

	public Vector2 RelativeResolution = new Vector2(1024f, 768f);

	public Bounds ScreenArea;

	public Camera UICamera;

	public bool SimulateTouchWithMouse = true;

	public List<vp_UIControl> Controls = new List<vp_UIControl>();

	public List<vp_UIAnchor> Anchors = new List<vp_UIAnchor>();

	public List<Transform> Transforms = new List<Transform>();

	public float DoubleTapTimeout = 0.25f;

	public Dictionary<int, List<vp_UIControl>> FingerIDs = new Dictionary<int, List<vp_UIControl>>();

	[SerializeField]
	protected Vector2 m_ScreenSize = Vector2.one;

	public virtual Vector2 ScreenSize => new Vector2(Screen.width, Screen.height);

	protected virtual void Awake()
	{
		if (!Application.isEditor)
		{
			SimulateTouchWithMouse = true;
		}
		ForceUIRefresh();
	}

	protected virtual void Init()
	{
		if (ScreenSize.x > ScreenSize.y)
		{
			float num = Resolution.y / Resolution.x * ScreenSize.x;
			RelativeResolution = new Vector2(Resolution.x / Resolution.y * num, ScreenSize.y);
		}
		else
		{
			float num2 = Resolution.x / Resolution.y * ScreenSize.y;
			RelativeResolution = new Vector2(ScreenSize.x, Resolution.y / Resolution.x * num2);
		}
		if (UICamera == null)
		{
			UICamera = GetUICamera(base.transform);
		}
		Anchors = base.transform.ChildComponentsToList<vp_UIAnchor>();
		Transforms = base.transform.ChildComponentsToList<Transform>();
		if (!Application.isPlaying)
		{
			Controls = base.transform.ChildComponentsToList<vp_UIControl>();
		}
		else
		{
			Controls.Clear();
			foreach (vp_UIControl item in base.transform.ChildComponentsToList<vp_UIControl>())
			{
				RegisterControl(item);
			}
		}
		if (Player == null || Application.isPlaying)
		{
			Player = UnityEngine.Object.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler;
		}
		if (AudioSource == null && Application.isPlaying)
		{
			AudioSource = Player.gameObject.GetComponent<AudioSource>();
			if (AudioSource == null)
			{
				AudioSource = Player.gameObject.AddComponent<AudioSource>();
			}
		}
	}

	protected virtual void OnEnable()
	{
		vp_GlobalEventReturn<bool>.Register("SimulateTouchWithMouse", CheckMouseSimulationMode);
	}

	protected virtual void OnDisable()
	{
		vp_GlobalEventReturn<bool>.Unregister("SimulateTouchWithMouse", CheckMouseSimulationMode);
	}

	protected virtual bool CheckMouseSimulationMode()
	{
		return SimulateTouchWithMouse;
	}

	public virtual void RegisterControl(vp_UIControl control)
	{
		if (!Controls.Contains(control))
		{
			vp_InputMobile.Register(control);
			Controls.Add(control);
		}
	}

	public virtual void UnregisterControl(vp_UIControl control)
	{
		vp_InputMobile.Unregister(control);
		Controls.Remove(control);
	}

	public static Camera GetUICamera(Transform t)
	{
		vp_UICamera[] componentsInChildren = t.root.GetComponentsInChildren<vp_UICamera>(includeInactive: true);
		if (componentsInChildren.Length == 0)
		{
			return null;
		}
		return componentsInChildren[0].GetComponent<Camera>();
	}

	public virtual void ForceUIRefresh()
	{
		Init();
		if (!Application.isPlaying)
		{
			if ((((LayerMask)UICamera.cullingMask).value & (1 << UILayer)) == 0)
			{
				UICamera.cullingMask |= 1 << UILayer;
			}
			foreach (Transform transform in Transforms)
			{
				transform.gameObject.layer = UILayer;
			}
		}
		foreach (vp_UIAnchor anchor in Anchors)
		{
			anchor.RefreshUI();
		}
		if (Application.isPlaying)
		{
			vp_GlobalEvent.Send("Update UI Positions");
		}
	}

	public static Vector3 PixelPerfect(Vector3 vector, int decimals = 3)
	{
		Vector3 result = vector;
		if ((double)result.x - Math.Round(result.x) != 0.0)
		{
			result.x = (float)Math.Round(result.x, decimals);
		}
		if ((double)result.y - Math.Round(result.y) != 0.0)
		{
			result.y = (float)Math.Round(result.y, decimals);
		}
		if ((double)result.z - Math.Round(result.z) != 0.0)
		{
			result.z = (float)Math.Round(result.z, decimals);
		}
		return result;
	}
}

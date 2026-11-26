using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class vp_UITween : MonoBehaviour
{
	public class Handle
	{
		public bool Custom;

		public Tween TweenObject;

		protected bool m_Active;

		public bool Active
		{
			get
			{
				return m_Active;
			}
			set
			{
				if (value)
				{
					TweenObject.ElapsedTime = 0f;
				}
				m_Active = value;
			}
		}
	}

	public class Tween
	{
		public object Object;

		public Transform Transform;

		public Handle Handle = new Handle();

		public string Name;

		public float ElapsedTime;

		public float Duration;

		public Action Callback;

		public Color StartColor;

		public Color EndColor;

		public Color CurrentColor;

		public Vector3 StartScale;

		public Vector3 EndScale;

		public Vector3 CurrentScale;

		public Quaternion StartRotation;

		public Quaternion EndRotation;

		public Quaternion CurrentRotation;

		public vp_UITweenType Type;

		public Tween(object obj, params object[] parameters)
		{
			Object = obj;
			for (int i = 0; i < parameters.Length; i++)
			{
				object obj2 = parameters[i];
				if (i % 2 == 0)
				{
					object obj3 = parameters[i + 1];
					if ((string)obj2 == "duration")
					{
						Duration = Convert.ToSingle(obj3);
					}
					if ((string)obj2 == "startColor")
					{
						StartColor = (Color)obj3;
					}
					if ((string)obj2 == "endColor")
					{
						EndColor = (Color)obj3;
					}
					if ((string)obj2 == "scale")
					{
						EndScale = (Vector3)obj3;
					}
					if ((string)obj2 == "rotation")
					{
						EndRotation = (Quaternion)obj3;
					}
					if ((string)obj2 == "handle")
					{
						Handle = (Handle)obj3;
					}
					if ((string)obj2 == "onComplete")
					{
						Callback = (Action)obj3;
					}
					if ((string)obj2 == "type")
					{
						Type = (vp_UITweenType)obj3;
					}
				}
			}
			if (Handle != null)
			{
				Handle.Custom = true;
			}
			if (Handle == null)
			{
				Handle = new Handle();
			}
			Handle.TweenObject = this;
			if (obj.GetType() == typeof(GameObject))
			{
				GameObject gameObject = (GameObject)obj;
				Transform = gameObject.transform;
				Name = gameObject.name;
				StartScale = Transform.localScale;
			}
		}

		public virtual void ColorCheck(object obj, params object[] parameters)
		{
			Color color = (Color)parameters[0];
			Color color2 = (Color)parameters[1];
			float num = Convert.ToSingle(parameters[2]);
			Action action = (Action)parameters[3];
			if (Object != obj || color != CurrentColor || EndColor != color2 || Duration != num || Callback != action)
			{
				Object = obj;
				StartColor = color;
				EndColor = color2;
				Duration = num;
				Callback = action;
				Handle.Active = true;
			}
		}

		public virtual void ScaleCheck(object obj, params object[] parameters)
		{
			Vector3 vector = (Vector3)parameters[0];
			float num = Convert.ToSingle(parameters[1]);
			Action action = (Action)parameters[2];
			if (Object != obj || Transform.localScale != CurrentScale || EndScale != vector || Duration != num || Callback != action)
			{
				Object = obj;
				StartScale = Transform.localScale;
				EndScale = vector;
				Duration = num;
				Callback = action;
				Handle.Active = true;
			}
		}

		public virtual void RotationCheck(object obj, params object[] parameters)
		{
			Quaternion quaternion = (Quaternion)parameters[0];
			float num = Convert.ToSingle(parameters[1]);
			Action action = (Action)parameters[2];
			if (Object != obj || Transform.localScale != CurrentScale || EndRotation != quaternion || Duration != num || Callback != action)
			{
				Object = obj;
				StartRotation = Transform.localRotation;
				EndRotation = quaternion;
				Duration = num;
				Callback = action;
				Handle.Active = true;
			}
		}

		public virtual void ColorUpdate()
		{
			((GameObject)Object).GetComponent<Renderer>().material.color = CurrentColor;
		}

		public virtual void ScaleUpdate()
		{
			Transform.localScale = CurrentScale;
		}
	}

	public enum vp_UITweenType
	{
		None,
		Color,
		Scale,
		Rotation
	}

	public Dictionary<Handle, Tween> Tweens = new Dictionary<Handle, Tween>();

	protected static List<string> AllowedHashParameters = new List<string> { "color", "startColor", "endColor", "type", "handle", "duration", "onComplete" };

	protected static vp_UITween m_Instance;

	public static vp_UITween Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = new GameObject("vp_UITween").AddComponent<vp_UITween>();
			}
			return m_Instance;
		}
	}

	protected virtual void Start()
	{
		Update();
	}

	protected virtual void Update()
	{
		foreach (Tween value in Tweens.Values)
		{
			ColorUpdate(value);
			ScaleUpdate(value);
			RotationUpdate(value);
		}
	}

	protected virtual void ColorUpdate(Tween tween)
	{
		if (tween.Type != vp_UITweenType.Color)
		{
			return;
		}
		if (tween.ElapsedTime < tween.Duration)
		{
			tween.CurrentColor = Color.Lerp(tween.StartColor, tween.EndColor, tween.ElapsedTime / tween.Duration);
			tween.ColorUpdate();
			tween.ElapsedTime += Time.deltaTime;
			return;
		}
		if (tween.CurrentColor != tween.EndColor)
		{
			tween.CurrentColor = tween.EndColor;
			tween.ColorUpdate();
		}
		if (tween.Callback != null)
		{
			tween.Callback();
		}
		if (!tween.Handle.Custom)
		{
			Instance.Tweens.Remove(tween.Handle);
		}
		else
		{
			tween.Handle.Active = false;
		}
	}

	protected virtual void ScaleUpdate(Tween tween)
	{
		if (tween.Type != vp_UITweenType.Scale || !tween.Handle.Active)
		{
			return;
		}
		if (tween.ElapsedTime < tween.Duration)
		{
			tween.CurrentScale = Vector3.Lerp(tween.StartScale, tween.EndScale, tween.ElapsedTime / tween.Duration);
			tween.Transform.localScale = tween.CurrentScale;
			tween.ElapsedTime += Time.deltaTime;
			return;
		}
		if (tween.Transform.localScale != tween.EndScale)
		{
			tween.Transform.localScale = tween.EndScale;
		}
		if (tween.Callback != null)
		{
			tween.Callback();
		}
		if (!tween.Handle.Custom)
		{
			Instance.Tweens.Remove(tween.Handle);
		}
		else
		{
			tween.Handle.Active = false;
		}
	}

	protected virtual void RotationUpdate(Tween tween)
	{
		if (tween.Type != vp_UITweenType.Rotation || !tween.Handle.Active)
		{
			return;
		}
		if (tween.ElapsedTime < tween.Duration)
		{
			tween.CurrentRotation = Quaternion.Lerp(tween.StartRotation, tween.EndRotation, tween.ElapsedTime / tween.Duration);
			tween.Transform.localRotation = tween.CurrentRotation;
			tween.ElapsedTime += Time.deltaTime;
			return;
		}
		if (tween.Transform.localRotation != tween.EndRotation)
		{
			tween.Transform.localRotation = tween.EndRotation;
		}
		if (tween.Callback != null)
		{
			tween.Callback();
		}
		if (!tween.Handle.Custom)
		{
			Instance.Tweens.Remove(tween.Handle);
		}
		else
		{
			tween.Handle.Active = false;
		}
	}

	public static Hashtable Hash(params object[] parameters)
	{
		if (parameters.Length % 2 != 0)
		{
			Debug.LogError("Invalid parameter length");
			return null;
		}
		Hashtable hashtable = new Hashtable();
		string key = null;
		for (int i = 0; i < parameters.Length; i++)
		{
			object obj = parameters[i];
			if (i % 2 == 0)
			{
				key = (string)obj;
				continue;
			}
			hashtable.Add(key, obj);
			key = null;
		}
		return hashtable;
	}

	public static Tween GetTweenForObject(object obj)
	{
		return Instance.Tweens.Values.FirstOrDefault((Tween t) => t.Object == obj);
	}

	public static Handle GetHandleForObject(object obj)
	{
		return GetTweenForObject(obj)?.Handle;
	}

	public static void FadeTo(object obj, float alpha, float duration)
	{
		ColorTo(obj, Hash("color", alpha, "duration", duration, "handle", null, "onComplete", null));
	}

	public static void FadeTo(object obj, float alpha, float duration, Handle handle)
	{
		ColorTo(obj, Hash("color", alpha, "duration", duration, "handle", handle, "onComplete", null));
	}

	public static void FadeTo(object obj, float alpha, float duration, Handle handle, Action onComplete)
	{
		ColorTo(obj, Hash("color", alpha, "duration", duration, "handle", handle, "onComplete", onComplete));
	}

	public static void ColorTo(object obj, Color color, float duration)
	{
		ColorTo(obj, Hash("color", color, "duration", duration, "handle", null, "onComplete", null));
	}

	public static void ColorTo(object obj, Color color, float duration, Handle handle)
	{
		ColorTo(obj, Hash("color", color, "duration", duration, "handle", handle, "onComplete", null));
	}

	public static void ColorTo(object obj, Color color, float duration, Handle handle, Action onComplete)
	{
		ColorTo(obj, Hash("color", color, "duration", duration, "handle", handle, "onComplete", onComplete));
	}

	public static void ColorTo(object obj, Hashtable ht)
	{
		if (obj.GetType() != typeof(GameObject))
		{
			Debug.LogError("This object must be of type GameObject. vp_UITween.ColorTo cannot proceed.");
			return;
		}
		GameObject gameObject = (GameObject)obj;
		if (gameObject.GetComponent<Renderer>() == null)
		{
			Debug.LogError("This GameObject does not have a renderer. vp_UITween.ColorTo cannot proceed.");
			return;
		}
		Color color = gameObject.GetComponent<Renderer>().material.color;
		color.a = ((ht["color"].GetType() == typeof(float)) ? ((float)ht["color"]) : color.a);
		Color color2 = ((ht["color"].GetType() == typeof(float)) ? color : ((Color)ht["color"]));
		Tween value = null;
		if (!Instance.Tweens.TryGetValue((Handle)ht["handle"], out value))
		{
			value = new Tween(obj, "startColor", gameObject.GetComponent<Renderer>().material.color, "endColor", color2, "duration", ht["duration"], "handle", ht["handle"], "onComplete", ht["onComplete"], "type", vp_UITweenType.Color);
			Instance.Tweens.Add((Handle)ht["handle"], value);
		}
		value.ColorCheck(obj, gameObject.GetComponent<Renderer>().material.color, color2, ht["duration"], ht["onComplete"]);
	}

	public static void ScaleTo(object obj, Hashtable ht)
	{
		if (obj.GetType() != typeof(GameObject))
		{
			Debug.LogError("This object must be of type GameObject. vp_UITween.ColorTween cannot proceed.");
			return;
		}
		Tween value = null;
		if (!Instance.Tweens.TryGetValue((Handle)ht["handle"], out value))
		{
			value = new Tween(obj, "scale", ht["scale"], "duration", ht["duration"], "handle", ht["handle"], "onComplete", ht["onComplete"], "type", vp_UITweenType.Scale);
			Instance.Tweens.Add((Handle)ht["handle"], value);
		}
		if (Convert.ToSingle(ht["duration"]) == 0f)
		{
			value.Transform.localScale = (Vector3)ht["scale"];
		}
		value.ScaleCheck(obj, ht["scale"], ht["duration"], ht["onComplete"]);
	}

	public static void RotateTo(object obj, Hashtable ht)
	{
		if (obj.GetType() != typeof(GameObject))
		{
			Debug.LogError("This object must be of type GameObject. vp_UITween.ColorTween cannot proceed.");
			return;
		}
		Tween value = null;
		if (!Instance.Tweens.TryGetValue((Handle)ht["handle"], out value))
		{
			value = new Tween(obj, "rotation", ht["rotation"], "duration", ht["duration"], "handle", ht["handle"], "onComplete", ht["onComplete"], "type", vp_UITweenType.Rotation);
			Instance.Tweens.Add((Handle)ht["handle"], value);
		}
		if (Convert.ToSingle(ht["duration"]) == 0f)
		{
			value.Transform.localRotation = (Quaternion)ht["rotation"];
		}
		value.RotationCheck(obj, ht["rotation"], ht["duration"], ht["onComplete"]);
	}
}

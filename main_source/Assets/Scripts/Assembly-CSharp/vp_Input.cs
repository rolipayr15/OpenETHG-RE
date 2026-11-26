using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_Input : MonoBehaviour
{
	[Serializable]
	public class vp_InputAxis
	{
		public KeyCode Positive;

		public KeyCode Negative;
	}

	public int ControlType;

	public Dictionary<string, KeyCode> Buttons = new Dictionary<string, KeyCode>();

	public List<string> ButtonKeys = new List<string>();

	public List<KeyCode> ButtonValues = new List<KeyCode>();

	public Dictionary<string, KeyCode> Buttons2 = new Dictionary<string, KeyCode>();

	public List<KeyCode> ButtonValues2 = new List<KeyCode>();

	public Dictionary<string, vp_InputAxis> Axis = new Dictionary<string, vp_InputAxis>();

	public List<string> AxisKeys = new List<string>();

	public List<vp_InputAxis> AxisValues = new List<vp_InputAxis>();

	public List<string> UnityAxis = new List<string>();

	public static string FolderPath = "UFPS/Base/Content/Resources/Input";

	public static string PrefabPath = "Assets/UFPS/Base/Content/Resources/Input/vp_Input.prefab";

	public static bool mIsDirty = true;

	protected static vp_Input m_Instance;

	public static vp_Input Instance
	{
		get
		{
			if (mIsDirty)
			{
				mIsDirty = false;
				if (m_Instance == null)
				{
					if (Application.isPlaying)
					{
						GameObject gameObject = Resources.Load("Input/vp_Input") as GameObject;
						if (gameObject == null)
						{
							m_Instance = new GameObject("vp_Input").AddComponent<vp_Input>();
						}
						else
						{
							m_Instance = gameObject.GetComponent<vp_Input>();
							if (m_Instance == null)
							{
								m_Instance = gameObject.AddComponent<vp_Input>();
							}
						}
					}
					m_Instance.SetupDefaults();
				}
			}
			return m_Instance;
		}
	}

	public static void CreateMissingInputPrefab(string prefabPath, string folderPath)
	{
	}

	protected virtual void Awake()
	{
		if (m_Instance == null)
		{
			m_Instance = Instance;
		}
	}

	public virtual void SetDirty(bool dirty)
	{
		mIsDirty = dirty;
	}

	public virtual void SetupDefaults(string type = "")
	{
		if ((type == "" || type == "Buttons") && ButtonKeys.Count == 0)
		{
			AddButton("Attack", KeyCode.Mouse0);
			AddButton("Jump", KeyCode.Space);
			AddButton("Crouch", KeyCode.C);
			AddButton("Run", KeyCode.LeftShift);
			AddButton("Interact", KeyCode.F);
			AddButton("Accept1", KeyCode.Return);
			AddButton("Accept2", KeyCode.KeypadEnter);
			AddButton("Pause", KeyCode.P);
			AddButton("Menu", KeyCode.Escape);
			AddButton("Teleport");
			CreateMissingSecondaryButtons();
			AddSecondaryButton("Attack", KeyCode.JoystickButton5);
			AddSecondaryButton("Jump", KeyCode.JoystickButton0);
			AddSecondaryButton("Run", KeyCode.JoystickButton8);
			AddSecondaryButton("Interact", KeyCode.JoystickButton2);
			AddSecondaryButton("Accept1");
			AddSecondaryButton("Accept2");
			AddSecondaryButton("Pause", KeyCode.P);
			AddSecondaryButton("Menu", KeyCode.JoystickButton6);
			AddSecondaryButton("Teleport");
		}
		if ((type == "" || type == "Axis") && AxisKeys.Count == 0)
		{
			AddAxis("Vertical", KeyCode.W, KeyCode.S);
			AddAxis("Horizontal", KeyCode.D, KeyCode.A);
		}
		if ((type == "" || type == "UnityAxis") && UnityAxis.Count == 0)
		{
			AddUnityAxis("Mouse X");
			AddUnityAxis("Mouse Y");
			AddUnityAxis("Horizontal");
			AddUnityAxis("Vertical");
			AddUnityAxis("LeftTrigger");
			AddUnityAxis("RightTrigger");
		}
		UpdateDictionaries();
	}

	public virtual void CreateMissingSecondaryButtons()
	{
		foreach (KeyValuePair<string, KeyCode> button in Buttons)
		{
			if (!Buttons2.ContainsKey(button.Key))
			{
				AddSecondaryButton(button.Key);
			}
		}
	}

	private bool HaveBinding(string button)
	{
		if (Buttons.ContainsKey(button))
		{
			return true;
		}
		if (Buttons2.ContainsKey(button))
		{
			return true;
		}
		Debug.LogError("Error (" + this?.ToString() + ") \"" + button + "\" is not declared in the UFPS Input Manager. You must add it from the 'UFPS -> Input Manager' editor menu for this button to work.");
		return false;
	}

	public virtual void AddButton(string n, KeyCode k = KeyCode.None)
	{
		if (ButtonKeys.Contains(n))
		{
			ButtonValues[ButtonKeys.IndexOf(n)] = k;
			return;
		}
		ButtonKeys.Add(n);
		ButtonValues.Add(k);
	}

	public virtual void AddSecondaryButton(string n, KeyCode k = KeyCode.None)
	{
		if (ButtonKeys.Contains(n))
		{
			try
			{
				ButtonValues2[ButtonKeys.IndexOf(n)] = k;
				return;
			}
			catch
			{
				ButtonValues2.Add(k);
				return;
			}
		}
		ButtonValues2.Add(k);
	}

	public virtual void AddAxis(string n, KeyCode pk = KeyCode.None, KeyCode nk = KeyCode.None)
	{
		if (AxisKeys.Contains(n))
		{
			AxisValues[AxisKeys.IndexOf(n)] = new vp_InputAxis
			{
				Positive = pk,
				Negative = nk
			};
		}
		else
		{
			AxisKeys.Add(n);
			AxisValues.Add(new vp_InputAxis
			{
				Positive = pk,
				Negative = nk
			});
		}
	}

	public virtual void AddUnityAxis(string n)
	{
		if (UnityAxis.Contains(n))
		{
			UnityAxis[UnityAxis.IndexOf(n)] = n;
		}
		else
		{
			UnityAxis.Add(n);
		}
	}

	public virtual void UpdateDictionaries()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Buttons.Clear();
		for (int i = 0; i < ButtonKeys.Count; i++)
		{
			if (!Buttons.ContainsKey(ButtonKeys[i]))
			{
				Buttons.Add(ButtonKeys[i], ButtonValues[i]);
			}
		}
		try
		{
			Buttons2.Clear();
			for (int j = 0; j < ButtonKeys.Count; j++)
			{
				if (!Buttons2.ContainsKey(ButtonKeys[j]))
				{
					Buttons2.Add(ButtonKeys[j], ButtonValues2[j]);
				}
			}
		}
		catch
		{
		}
		Axis.Clear();
		for (int k = 0; k < AxisKeys.Count; k++)
		{
			Axis.Add(AxisKeys[k], new vp_InputAxis
			{
				Positive = AxisValues[k].Positive,
				Negative = AxisValues[k].Negative
			});
		}
		CreateMissingSecondaryButtons();
	}

	public static bool GetButtonAny(string button)
	{
		return Instance.DoGetButtonAny(button);
	}

	public virtual bool DoGetButtonAny(string button)
	{
		if (!HaveBinding(button))
		{
			return false;
		}
		if (Input.GetKey(Buttons[button]) || Input.GetKeyDown(Buttons[button]) || Input.GetKeyUp(Buttons[button]))
		{
			return true;
		}
		if (Input.GetKeyDown(Buttons2[button]) || Input.GetKeyDown(Buttons2[button]) || Input.GetKeyUp(Buttons2[button]))
		{
			return true;
		}
		return false;
	}

	public static bool GetButton(string button)
	{
		return Instance.DoGetButton(button);
	}

	public virtual bool DoGetButton(string button)
	{
		if (!HaveBinding(button))
		{
			return false;
		}
		if (Input.GetKey(Buttons[button]))
		{
			return true;
		}
		if (Input.GetKey(Buttons2[button]))
		{
			return true;
		}
		return false;
	}

	public static bool GetButtonDown(string button)
	{
		return Instance.DoGetButtonDown(button);
	}

	public virtual bool DoGetButtonDown(string button)
	{
		if (!HaveBinding(button))
		{
			return false;
		}
		if (Input.GetKeyDown(Buttons[button]))
		{
			return true;
		}
		if (Input.GetKeyDown(Buttons2[button]))
		{
			return true;
		}
		return false;
	}

	public static bool GetButtonUp(string button)
	{
		return Instance.DoGetButtonUp(button);
	}

	public virtual bool DoGetButtonUp(string button)
	{
		if (!HaveBinding(button))
		{
			return false;
		}
		if (Input.GetKeyUp(Buttons[button]))
		{
			return true;
		}
		if (Input.GetKeyUp(Buttons2[button]))
		{
			return true;
		}
		return false;
	}

	public static float GetAxisRaw(string axis)
	{
		return Instance.DoGetAxisRaw(axis);
	}

	public virtual float DoGetAxisRaw(string axis)
	{
		if (Axis.ContainsKey(axis) && ControlType == 0)
		{
			float result = 0f;
			if (Input.GetKey(Axis[axis].Positive))
			{
				result = 1f;
			}
			if (Input.GetKey(Axis[axis].Negative))
			{
				result = -1f;
			}
			return result;
		}
		if (UnityAxis.Contains(axis))
		{
			return Input.GetAxisRaw(axis);
		}
		Debug.LogError("Error (" + this?.ToString() + ") \"" + axis + "\" is not declared in the UFPS Input Manager. You must add it from the 'UFPS -> Input Manager' editor menu for this axis to work.");
		return 0f;
	}

	public static void ChangeButtonKey(string button, KeyCode keyCode, bool save = false)
	{
		if (Instance.Buttons.ContainsKey(button))
		{
			if (save)
			{
				Instance.ButtonValues[Instance.ButtonKeys.IndexOf(button)] = keyCode;
			}
			Instance.Buttons[button] = keyCode;
		}
		else if (Instance.Buttons2.ContainsKey(button))
		{
			if (save)
			{
				Instance.ButtonValues2[Instance.ButtonKeys.IndexOf(button)] = keyCode;
			}
			Instance.Buttons2[button] = keyCode;
		}
		else
		{
			Instance.HaveBinding(button);
		}
	}

	public static void ChangeAxis(string n, KeyCode pk = KeyCode.None, KeyCode nk = KeyCode.None, bool save = false)
	{
		if (!Instance.AxisKeys.Contains(n))
		{
			Debug.LogWarning("The Axis \"" + n + "\" Doesn't Exist");
			return;
		}
		if (save)
		{
			Instance.AxisValues[Instance.AxisKeys.IndexOf(n)] = new vp_InputAxis
			{
				Positive = pk,
				Negative = nk
			};
		}
		Instance.Axis[n] = new vp_InputAxis
		{
			Positive = pk,
			Negative = nk
		};
	}

	private void DebugDumpCollections()
	{
		string text = "\n\n---- BUTTON KEYS: ---- \n";
		string text2 = "\n\n---- BUTTON VALUES: ---- \n";
		string text3 = "\n\n---- BUTTON VALUES 2: ---- \n";
		foreach (string buttonKey in ButtonKeys)
		{
			text = text + "\t\t" + buttonKey + "\n";
		}
		foreach (KeyCode buttonValue in ButtonValues)
		{
			text2 = text2 + "\t\t" + buttonValue.ToString() + "\n";
		}
		foreach (KeyCode item in ButtonValues2)
		{
			text3 = text3 + "\t\t" + item.ToString() + "\n";
		}
		Debug.Log("-------- DUMP --------\n" + text + text2 + text3);
	}
}

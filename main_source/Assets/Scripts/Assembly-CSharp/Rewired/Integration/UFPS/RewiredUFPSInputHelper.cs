using System.Collections;
using System.ComponentModel;
using UnityEngine;

namespace Rewired.Integration.UFPS
{
	[AddComponentMenu("")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class RewiredUFPSInputHelper : vp_Input
	{
		[SerializeField]
		public bool m_fixJoystickLookSensitivity = true;

		[SerializeField]
		protected int m_lookActionX = 18;

		[SerializeField]
		protected int m_lookActionY = 19;

		[SerializeField]
		protected float m_fixJLSTargetFPS = 60f;

		[SerializeField]
		protected bool m_fixJLSUseSmoothDeltaTime;

		protected Player player;

		protected string lookActionXName;

		protected string lookActionYName;

		protected bool fixJLSInitialized;

		public bool fixJoystickLookSensitivity
		{
			get
			{
				return m_fixJoystickLookSensitivity;
			}
			set
			{
				if (value != m_fixJoystickLookSensitivity)
				{
					m_fixJoystickLookSensitivity = value;
					if (value)
					{
						InitializeFixJLS();
					}
				}
			}
		}

		public int lookActionX
		{
			get
			{
				return m_lookActionX;
			}
			set
			{
				if (value != m_lookActionX)
				{
					m_lookActionX = value;
					InitializeFixJLS();
				}
			}
		}

		public int lookActionY
		{
			get
			{
				return m_lookActionY;
			}
			set
			{
				if (value != m_lookActionY)
				{
					m_lookActionY = value;
					InitializeFixJLS();
				}
			}
		}

		public float fixJLSTargetFPS
		{
			get
			{
				return m_fixJLSTargetFPS;
			}
			set
			{
				m_fixJLSTargetFPS = value;
			}
		}

		public bool fixJLSUseSmoothDeltaTime
		{
			get
			{
				return m_fixJLSUseSmoothDeltaTime;
			}
			set
			{
				m_fixJLSUseSmoothDeltaTime = value;
			}
		}

		protected override void Awake()
		{
			StartCoroutine(DelayedInit());
		}

		private IEnumerator DelayedInit()
		{
			yield return null;
			yield return null;
			yield return null;
			if (m_fixJoystickLookSensitivity)
			{
				InitializeFixJLS();
			}
			player = ReInput.players.GetPlayer(0);
			vp_Input.m_Instance = this;
		}

		private void OnValidate()
		{
			if (Application.isPlaying && m_fixJoystickLookSensitivity)
			{
				InitializeFixJLS();
			}
		}

		public override bool DoGetButtonAny(string button)
		{
			if (!DoGetButton(button) && !DoGetButtonDown(button))
			{
				return DoGetButtonUp(button);
			}
			return true;
		}

		public override bool DoGetButton(string button)
		{
			return player.GetButton(button);
		}

		public override bool DoGetButtonDown(string button)
		{
			return player.GetButtonDown(button);
		}

		public override bool DoGetButtonUp(string button)
		{
			return player.GetButtonUp(button);
		}

		public override float DoGetAxisRaw(string axis)
		{
			float num = player.GetAxisRaw(axis);
			if (m_fixJoystickLookSensitivity && (axis == lookActionXName || axis == lookActionYName))
			{
				Controller lastActiveController = player.controllers.GetLastActiveController();
				if (lastActiveController != null && (lastActiveController.type == ControllerType.Joystick || lastActiveController.type == ControllerType.Custom))
				{
					num *= (m_fixJLSUseSmoothDeltaTime ? Time.smoothDeltaTime : Time.deltaTime) * m_fixJLSTargetFPS;
				}
			}
			return num;
		}

		public override void SetDirty(bool dirty)
		{
			LogNotSupportedError("SetDirty");
		}

		public override void SetupDefaults(string type = "")
		{
			LogNotSupportedError("SetupDefaults");
		}

		public override void AddButton(string n, KeyCode k = KeyCode.None)
		{
			LogNotSupportedError("AddButton");
		}

		public override void AddAxis(string n, KeyCode pk = KeyCode.None, KeyCode nk = KeyCode.None)
		{
			LogNotSupportedError("AddAxis");
		}

		public override void AddUnityAxis(string n)
		{
			LogNotSupportedError("AddUnityAxis");
		}

		public override void UpdateDictionaries()
		{
			LogNotSupportedError("UpdateDictionaries");
		}

		protected virtual void InitializeFixJLS()
		{
			if (ReInput.isReady)
			{
				InputAction action = ReInput.mapping.GetAction(m_lookActionX);
				if (action == null)
				{
					Debug.LogError("Rewired UFPS: The Action chosen for Look Action X does not exist!");
					m_fixJoystickLookSensitivity = false;
				}
				else
				{
					lookActionXName = action.name;
				}
				action = ReInput.mapping.GetAction(m_lookActionY);
				if (action == null)
				{
					Debug.LogError("Rewired UFPS: The Action chosen for Look Action Y does not exist!");
					m_fixJoystickLookSensitivity = false;
				}
				else
				{
					lookActionYName = action.name;
				}
				fixJLSInitialized = true;
			}
		}

		protected virtual void LogNotSupportedError(string name)
		{
			Debug.LogError("Rewired UFPS: The method \"" + name + "\" is not supported. Use Rewired to manage input and reassignment instead.");
		}
	}
}

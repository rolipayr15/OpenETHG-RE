using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Rewired.Integration.UFPS
{
	[AddComponentMenu(null)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class Fixed_RewiredUFPSInputHelper : RewiredUFPSInputHelper
	{
		private GameplayInputContext policy;

		private readonly Dictionary<string, float> axisSamples = new Dictionary<string, float>();

		private int sampleFrame = -1;

		private int resetFrame = -1;

		protected override void Awake()
		{
			policy = GameplayInputContext.Shared;
			m_Instance = this;
		}

		private void OnEnable()
		{
			policy = GameplayInputContext.Shared;
			policy.InputResetRequested += ResetSamples;
			ResetSamples(default(GameplayInputContext.ResetReason));
		}

		private void OnDisable()
		{
			policy.InputResetRequested -= ResetSamples;
			ResetSamples(default(GameplayInputContext.ResetReason));
			player = null;
			fixJLSInitialized = false;
		}

		private void ResetSamples(GameplayInputContext.ResetReason reason)
		{
			axisSamples.Clear();
			sampleFrame = Time.frameCount;
			resetFrame = sampleFrame;
		}

		private bool BindPlayer()
		{
			if (!base.isActiveAndEnabled || !ReInput.isReady)
			{
				return false;
			}
			ControlModeManager manager = ControlModeManager.Instance;
			if (manager == null || !manager.isActiveAndEnabled || manager.Player == null)
			{
				return false;
			}
			if (player != manager.Player)
			{
				player = manager.Player;
				InitializeFixJLS();
				ResetSamples(default(GameplayInputContext.ResetReason));
			}
			return true;
		}

		public override bool DoGetButton(string button)
		{
			return BindPlayer() && player.GetButton(button);
		}

		public override bool DoGetButtonDown(string button)
		{
			return BindPlayer() && player.GetButtonDown(button);
		}

		public override bool DoGetButtonUp(string button)
		{
			return BindPlayer() && player.GetButtonUp(button);
		}

		public override float DoGetAxisRaw(string axis)
		{
			if (!BindPlayer())
			{
				return 0f;
			}
			GameplayInputContext.Permissions state = policy.Current;
			bool look = axis == lookActionXName || axis == lookActionYName;
			bool move = axis == "Horizontal" || axis == "Vertical";
			if (look || move)
			{
				ControlModeManager manager = ControlModeManager.Instance;
				if (!manager.HasFocus || manager.RequiresRecoveryUI || !(look ? state.Look : state.Move))
				{
					return 0f;
				}
			}
			if (resetFrame == Time.frameCount)
			{
				return 0f;
			}
			if (sampleFrame != Time.frameCount)
			{
				sampleFrame = Time.frameCount;
				axisSamples.Clear();
			}
			if (!axisSamples.TryGetValue(axis, out float value))
			{
				value = player.GetAxisRaw(axis);
				if (look && m_fixJoystickLookSensitivity && player.GetAxisRawCoordinateMode(axis) == AxisCoordinateMode.Absolute)
				{
					value *= (m_fixJLSUseSmoothDeltaTime ? Time.smoothDeltaTime : Time.deltaTime) * m_fixJLSTargetFPS;
				}
				axisSamples.Add(axis, value);
			}
			return value;
		}

		protected void LateUpdate()
		{
			if (m_Instance != this)
			{
				m_Instance = this;
			}
		}
	}
}

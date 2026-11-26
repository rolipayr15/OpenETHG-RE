using System;
using UnityEngine;
using System.Collections.Generic;

namespace I2.Loc
{
	public class BaseSpecializationManager
	{
		public string[] mSpecializations;

		public Dictionary<string, string> mSpecializationsFallbacks;

		public virtual void InitializeSpecializations()
		{
			mSpecializations = new string[] { "Any", "PC", "Touch", "Controller", "VR", "XBox", "PS4", "OculusVR", "ViveVR", "GearVR", "Android", "IOS" };
			mSpecializationsFallbacks = new Dictionary<string, string>(StringComparer.Ordinal)
			{
				{ "XBox", "Controller" },
				{ "PS4", "Controller" },
				{ "OculusVR", "VR" },
				{ "ViveVR", "VR" },
				{ "GearVR", "VR" },
				{ "Android", "Touch" },
				{ "IOS", "Touch" }
			};
		}

		public virtual string GetCurrentSpecialization()
		{
			if (mSpecializations == null)
			{
				InitializeSpecializations();
			}
			return IsTouchInputSupported() ? "Touch" : "PC";
		}

		private bool IsTouchInputSupported()
		{
			return Input.touchSupported;
		}

		public virtual string GetFallbackSpecialization(string specialization)
		{
			if (mSpecializationsFallbacks == null)
			{
				InitializeSpecializations();
			}
			return mSpecializationsFallbacks.TryGetValue(specialization, out string fallback) ? fallback : "Any";
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace I2.Loc
{
	public class LocalizationParamsManager : MonoBehaviour, ILocalizationParamsManager
	{
		[Serializable]
		public struct ParamValue
		{
			public string Name;

			public string Value;
		}

		[SerializeField]
		public List<ParamValue> _Params;

		public bool _IsGlobalManager;

		public string GetParameterValue(string ParamName)
		{
			if (_Params != null)
			{
				foreach (ParamValue item in _Params)
				{
					if (item.Name == ParamName)
					{
						return item.Value;
					}
				}
			}
			return null;
		}

		public void SetParameterValue(string ParamName, string ParamValue, bool localize = true)
		{
			bool found = false;
			for (int i = 0; i < _Params.Count; i++)
			{
				if (_Params[i].Name == ParamName)
				{
					ParamValue value = _Params[i];
					value.Value = ParamValue;
					_Params[i] = value;
					found = true;
					break;
				}
			}
			if (!found)
			{
				_Params.Add(new ParamValue { Name = ParamName, Value = ParamValue });
			}
			if (localize)
			{
				OnLocalize();
			}
		}

		public void OnLocalize()
		{
			Localize localize = GetComponent<Localize>();
			if (localize != null)
			{
				localize.OnLocalize(true);
			}
		}

		public virtual void OnEnable()
		{
			if (_IsGlobalManager)
			{
				DoAutoRegister();
			}
		}

		public void DoAutoRegister()
		{
			if (!LocalizationManager.ParamManagers.Contains(this))
			{
				LocalizationManager.ParamManagers.Add(this);
				LocalizationManager.LocalizeAll(true);
			}
		}

		public void OnDisable()
		{
			LocalizationManager.ParamManagers.Remove(this);
		}
	}
}

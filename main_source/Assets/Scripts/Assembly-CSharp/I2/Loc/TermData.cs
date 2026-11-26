using System;
using System.Collections.Generic;
using UnityEngine;

namespace I2.Loc
{
	[Serializable]
	public class TermData
	{
		public string Term;

		public eTermType TermType;

		[NonSerialized]
		public string Description;

		public string[] Languages;

		public byte[] Flags;

		[SerializeField]
		private string[] Languages_Touch;

		public string GetTranslation(int idx, string specialization = null, bool editMode = false)
		{
			if (Languages == null || idx < 0 || idx >= Languages.Length)
			{
				return null;
			}
			return SpecializationManager.GetSpecializedText(Languages[idx], specialization);
		}

		public void SetTranslation(int idx, string translation, string specialization = null)
		{
			if (Languages != null && idx >= 0 && idx < Languages.Length)
			{
				Languages[idx] = SpecializationManager.SetSpecializedText(Languages[idx], translation, specialization);
			}
		}

		public void RemoveSpecialization(string specialization)
		{
		}

		public void RemoveSpecialization(int idx, string specialization)
		{
		}

		public bool IsAutoTranslated(int idx, bool IsTouch)
		{
			return Flags != null && idx >= 0 && idx < Flags.Length && (Flags[idx] & 2) != 0;
		}

		public void Validate()
		{
			if (Languages == null)
			{
				Languages = new string[0];
			}
			if (Flags == null || Flags.Length != Languages.Length)
			{
				byte[] flags = new byte[Languages.Length];
				if (Flags != null)
				{
					Array.Copy(Flags, flags, Math.Min(Flags.Length, flags.Length));
				}
				Flags = flags;
			}
		}

		public bool IsTerm(string name, bool allowCategoryMistmatch)
		{
			if (allowCategoryMistmatch)
			{
				return name == LanguageSourceData.GetKeyFromFullTerm(Term);
			}
			return name == Term;
		}

		public bool HasSpecializations()
		{
			if (Languages != null)
			{
				foreach (string text in Languages)
				{
					if (!string.IsNullOrEmpty(text) && text.IndexOf("[i2s_", StringComparison.Ordinal) >= 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		public List<string> GetAllSpecializations()
		{
			List<string> list = new List<string>();
			if (Languages != null)
			{
				foreach (string text in Languages)
				{
					SpecializationManager.AppendSpecializations(text, list);
				}
			}
			return list;
		}
	}
}

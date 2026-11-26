using System.Text;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace I2.Loc
{
	[AddComponentMenu("I2/Localization/I2 Localize")]
	public class Localize : MonoBehaviour
	{
		public enum TermModification
		{
			DontModify = 0,
			ToUpper = 1,
			ToLower = 2,
			ToUpperFirst = 3,
			ToTitle = 4
		}

		public string mTerm;

		public string mTermSecondary;

		[NonSerialized]
		public string FinalTerm;

		[NonSerialized]
		public string FinalSecondaryTerm;

		public TermModification PrimaryTermModifier;

		public TermModification SecondaryTermModifier;

		public string TermPrefix;

		public string TermSuffix;

		public bool LocalizeOnAwake;

		public bool IgnoreRTL;

		public int MaxCharactersInRTL;

		public bool IgnoreNumbersInRTL;

		public bool CorrectAlignmentForRTL;

		public bool AddSpacesToJoinedLanguages;

		public bool AllowLocalizedParameters;

		public bool AllowParameters;

		public List<UnityEngine.Object> TranslatedObjects;

		[NonSerialized]
		public Dictionary<string, UnityEngine.Object> mAssetDictionary;

		public UnityEvent LocalizeEvent;

		public static string MainTranslation;

		public static string SecondaryTranslation;

		public static string CallBackTerm;

		public static string CallBackSecondaryTerm;

		public static Localize CurrentLocalizeComponent;

		public bool AlwaysForceLocalize;

		[SerializeField]
		public EventCallback LocalizeCallBack;

		public bool mGUI_ShowReferences;

		public bool mGUI_ShowTems;

		public bool mGUI_ShowCallback;

		public ILocalizeTarget mLocalizeTarget;

		public string mLocalizeTargetName;

		public string Term
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public string SecondaryTerm
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		private void Awake()
		{
			UpdateAssetDictionary();
			FindTarget();
			if (LocalizeOnAwake)
			{
				OnLocalize();
			}
		}

		private void OnEnable()
		{
			OnLocalize();
		}

		private string LastLocalizedLanguage;

		private static string Modify(string text, TermModification modifier)
		{
			switch (modifier)
			{
			case TermModification.ToUpper:
				return text.ToUpper();
			case TermModification.ToLower:
				return text.ToLower();
			case TermModification.ToUpperFirst:
				return text.Length > 0 ? char.ToUpper(text[0]) + text.Substring(1).ToLower() : text;
			case TermModification.ToTitle:
				return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
			default:
				return text;
			}
		}

		internal T GetSecondaryTranslatedObj<T>(ref string mainTranslation, ref string secondaryTranslation) where T : UnityEngine.Object
		{
			string main = mainTranslation;
			string secondary = null;
			if (!string.IsNullOrEmpty(mainTranslation) && mainTranslation.Length > 1 && mainTranslation[0] == '[')
			{
				int close = mainTranslation.IndexOf(']');
				if (close > 0)
				{
					secondary = mainTranslation.Substring(1, close - 1);
					main = mainTranslation.Substring(close + 1);
				}
			}
			T asset = null;
			if (!string.IsNullOrEmpty(secondary))
			{
				asset = FindTranslatedObject<T>(secondary);
				if (asset != null)
				{
					mainTranslation = main;
					secondaryTranslation = secondary;
				}
			}
			return asset != null ? asset : FindTranslatedObject<T>(secondaryTranslation);
		}

		public bool HasCallback()
		{
			return LocalizeCallBack.HasCallback() || (LocalizeEvent != null && LocalizeEvent.GetPersistentEventCount() > 0);
		}

		public void OnLocalize(bool Force = false)
		{
			if (!Force && (!enabled || gameObject == null || !gameObject.activeInHierarchy))
			{
				return;
			}
			if (string.IsNullOrEmpty(LocalizationManager.CurrentLanguage))
			{
				return;
			}
			if (!AlwaysForceLocalize && !Force && !HasCallback() && LastLocalizedLanguage == LocalizationManager.CurrentLanguage)
			{
				return;
			}
			LastLocalizedLanguage = LocalizationManager.CurrentLanguage;
			if (string.IsNullOrEmpty(FinalTerm) || string.IsNullOrEmpty(FinalSecondaryTerm))
			{
				GetFinalTerms(out FinalTerm, out FinalSecondaryTerm);
			}
			bool hasCallback = Application.isPlaying && HasCallback();
			if (!hasCallback && string.IsNullOrEmpty(FinalTerm) && string.IsNullOrEmpty(FinalSecondaryTerm))
			{
				return;
			}
			CurrentLocalizeComponent = this;
			CallBackTerm = FinalTerm;
			CallBackSecondaryTerm = FinalSecondaryTerm;
			MainTranslation = (string.IsNullOrEmpty(FinalTerm) || FinalTerm == "-") ? null : LocalizationManager.GetTranslation(FinalTerm, false);
			SecondaryTranslation = (string.IsNullOrEmpty(FinalSecondaryTerm) || FinalSecondaryTerm == "-") ? null : LocalizationManager.GetTranslation(FinalSecondaryTerm, false);
			if (!hasCallback && string.IsNullOrEmpty(FinalTerm) && string.IsNullOrEmpty(SecondaryTranslation))
			{
				CurrentLocalizeComponent = null;
				return;
			}
			LocalizeCallBack.Execute(this);
			LocalizeEvent?.Invoke();
			if (AllowParameters)
			{
				LocalizationManager.ApplyLocalizationParams(ref MainTranslation, gameObject, AllowLocalizedParameters);
			}
			if (!FindTarget())
			{
				CurrentLocalizeComponent = null;
				return;
			}
			bool applyRtl = LocalizationManager.IsRight2Left && !IgnoreRTL;
			if (MainTranslation != null)
			{
				MainTranslation = Modify(MainTranslation, PrimaryTermModifier);
				if (!string.IsNullOrEmpty(TermPrefix))
				{
					MainTranslation = applyRtl ? MainTranslation + TermPrefix : TermPrefix + MainTranslation;
				}
				if (!string.IsNullOrEmpty(TermSuffix))
				{
					MainTranslation = applyRtl ? TermSuffix + MainTranslation : MainTranslation + TermSuffix;
				}
				if (AddSpacesToJoinedLanguages && LocalizationManager.HasJoinedWords && !string.IsNullOrEmpty(MainTranslation))
				{
					StringBuilder builder = new StringBuilder();
					builder.Append(MainTranslation[0]);
					for (int i = 1; i < MainTranslation.Length; i++)
					{
						builder.Append(' ').Append(MainTranslation[i]);
					}
					MainTranslation = builder.ToString();
				}
				if (applyRtl && mLocalizeTarget.AllowMainTermToBeRTL() && !string.IsNullOrEmpty(MainTranslation))
				{
					MainTranslation = LocalizationManager.ApplyRTLfix(MainTranslation, MaxCharactersInRTL, IgnoreNumbersInRTL);
				}
			}
			if (SecondaryTranslation != null)
			{
				SecondaryTranslation = Modify(SecondaryTranslation, SecondaryTermModifier);
				if (applyRtl && mLocalizeTarget.AllowSecondTermToBeRTL() && !string.IsNullOrEmpty(SecondaryTranslation))
				{
					SecondaryTranslation = LocalizationManager.ApplyRTLfix(SecondaryTranslation);
				}
			}
			if (LocalizationManager.HighlightLocalizedTargets)
			{
				MainTranslation = "LOC:" + FinalTerm;
			}
			mLocalizeTarget.DoLocalize(this, MainTranslation, SecondaryTranslation);
			CurrentLocalizeComponent = null;
		}

		public bool FindTarget()
		{
			if (mLocalizeTarget != null && mLocalizeTarget.IsValid(this))
			{
				return true;
			}
			if (mLocalizeTarget != null)
			{
				if (Application.isPlaying)
				{
					Destroy(mLocalizeTarget);
				}
				mLocalizeTarget = null;
				mLocalizeTargetName = null;
			}
			if (!string.IsNullOrEmpty(mLocalizeTargetName))
			{
				foreach (ILocalizeTargetDescriptor descriptor in LocalizationManager.mLocalizeTargets)
				{
					if (mLocalizeTargetName == descriptor.GetTargetType().ToString())
					{
						if (descriptor.CanLocalize(this))
						{
							mLocalizeTarget = descriptor.CreateTarget(this);
						}
						if (mLocalizeTarget != null)
						{
							return true;
						}
					}
				}
			}
			foreach (ILocalizeTargetDescriptor descriptor in LocalizationManager.mLocalizeTargets)
			{
				if (descriptor.CanLocalize(this))
				{
					mLocalizeTarget = descriptor.CreateTarget(this);
					mLocalizeTargetName = descriptor.GetTargetType().ToString();
					if (mLocalizeTarget != null)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void GetFinalTerms(out string primaryTerm, out string secondaryTerm)
		{
			primaryTerm = string.Empty;
			secondaryTerm = string.Empty;
			if (!FindTarget())
			{
				return;
			}
			if (mLocalizeTarget != null)
			{
				mLocalizeTarget.GetFinalTerms(this, mTerm, mTermSecondary, out primaryTerm, out secondaryTerm);
			}
			if (!string.IsNullOrEmpty(mTerm))
			{
				primaryTerm = mTerm;
			}
			if (!string.IsNullOrEmpty(mTermSecondary))
			{
				secondaryTerm = mTermSecondary;
			}
			primaryTerm = primaryTerm?.Trim();
			secondaryTerm = secondaryTerm?.Trim();
		}

		public string GetMainTargetsText()
		{
			string primary = null;
			string secondary = null;
			if (mLocalizeTarget != null)
			{
				mLocalizeTarget.GetFinalTerms(this, null, null, out primary, out secondary);
			}
			return string.IsNullOrEmpty(primary) ? mTerm : primary;
		}

		public void SetFinalTerms(string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII)
		{
			primaryTerm = Main;
			secondaryTerm = Secondary;
		}

		public void SetTerm(string primary)
		{
			if (!string.IsNullOrEmpty(primary))
			{
				FinalTerm = mTerm = primary;
			}
			OnLocalize(true);
		}

		public void SetTerm(string primary, string secondary)
		{
			if (!string.IsNullOrEmpty(primary))
			{
				FinalTerm = mTerm = primary;
			}
			FinalSecondaryTerm = mTermSecondary = secondary;
			OnLocalize(true);
		}

		public void UpdateAssetDictionary()
		{
			if (TranslatedObjects == null)
			{
				return;
			}
			TranslatedObjects.RemoveAll((UnityEngine.Object asset) => asset == null);
			mAssetDictionary = new Dictionary<string, UnityEngine.Object>(StringComparer.Ordinal);
			foreach (UnityEngine.Object asset in TranslatedObjects)
			{
				mAssetDictionary[asset.name] = asset;
			}
		}

		internal T GetObject<T>(string Translation) where T : UnityEngine.Object
		{
			return null;
		}

		private T GetTranslatedObject<T>(string Translation) where T : UnityEngine.Object
		{
			return null;
		}

		private void DeserializeTranslation(string translation, out string value, out string secondary)
		{
			value = null;
			secondary = null;
		}

		public T FindTranslatedObject<T>(string value) where T : UnityEngine.Object
		{
			if (string.IsNullOrEmpty(value))
			{
				return null;
			}
			if (mAssetDictionary == null || mAssetDictionary.Count != TranslatedObjects.Count)
			{
				UpdateAssetDictionary();
			}
			if (mAssetDictionary != null)
			{
				foreach (KeyValuePair<string, UnityEngine.Object> pair in mAssetDictionary)
				{
					if (pair.Value is T asset && value.EndsWith(pair.Key, StringComparison.OrdinalIgnoreCase) && string.Compare(value, pair.Key, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return asset;
					}
				}
			}
			return LocalizationManager.GetTranslatedObject<T>(value);
		}

		public bool HasTranslatedObject(UnityEngine.Object Obj)
		{
			return TranslatedObjects.Contains(Obj) || ResourceManager.pInstance.HasAsset(Obj);
		}

		public void AddTranslatedObject(UnityEngine.Object Obj)
		{
			if (!TranslatedObjects.Contains(Obj))
			{
				TranslatedObjects.Add(Obj);
				UpdateAssetDictionary();
			}
		}

		public void SetGlobalLanguage(string Language)
		{
			LocalizationManager.CurrentLanguage = Language;
		}
	}
}

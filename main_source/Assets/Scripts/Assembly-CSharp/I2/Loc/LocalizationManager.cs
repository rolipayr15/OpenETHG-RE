using System.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace I2.Loc
{
	public static class LocalizationManager
	{
		public delegate bool FnCustomApplyLocalizationParams(ref string translation, _GetParam getParam, bool allowLocalizedParameters);

		public delegate object _GetParam(string param);

		public delegate void OnLocalizeCallback();



		private static string mCurrentLanguage = null;

		private static string mLanguageCode;

		private static CultureInfo mCurrentCulture;

		private static bool mChangeCultureInfo;

		public static bool IsRight2Left = false;

		public static bool HasJoinedWords = false;

		public static List<ILocalizationParamsManager> ParamManagers = new List<ILocalizationParamsManager>();

		public static FnCustomApplyLocalizationParams CustomApplyLocalizationParams;

		private static string[] LanguagesRTL = new string[] { "ar-DZ", "ar", "ar-BH", "ar-EG", "ar-IQ", "ar-JO", "ar-KW", "ar-LB", "ar-LY", "ar-MA", "ar-OM", "ar-QA", "ar-SA", "ar-SY", "ar-TN", "ar-AE", "ar-YE", "fa", "he", "ur", "ji" };

		public static List<LanguageSourceData> Sources = new List<LanguageSourceData>();

		public static string[] GlobalSources = new string[] { "I2Languages" };

		public static Func<LanguageSourceData, bool> Callback_AllowSyncFromGoogle;

		private static string mCurrentDeviceLanguage;

		public static List<ILocalizeTargetDescriptor> mLocalizeTargets = new List<ILocalizeTargetDescriptor>();

		[CompilerGenerated]
		private static OnLocalizeCallback m_OnLocalizeEvent;

		private static bool mLocalizeIsScheduled;

		private static bool mLocalizeIsScheduledWithForcedValue;

		public static bool HighlightLocalizedTargets;

		public static string CurrentLanguage
		{
			get
			{
				InitializeIfNeeded();
				return mCurrentLanguage;
			}
			set
			{
				InitializeIfNeeded();
				string language = GetSupportedLanguage(value);
				if (!string.IsNullOrEmpty(language) && mCurrentLanguage != language)
				{
					SetLanguageAndCode(language, GetLanguageCode(language));
				}
			}
		}

		public static string CurrentLanguageCode
		{
			get
			{
				InitializeIfNeeded();
				return mLanguageCode;
			}
			set
			{
				InitializeIfNeeded();
				if (mLanguageCode != value)
				{
					string language = GetLanguageFromCode(value);
					if (!string.IsNullOrEmpty(language))
					{
						SetLanguageAndCode(language, value);
					}
				}
			}
		}

		public static string CurrentRegion
		{
			get
			{
				string language = CurrentLanguage;
				int index = language.IndexOfAny("/\\".ToCharArray());
				if (index > 0)
				{
					return language.Substring(index + 1);
				}
				index = language.IndexOfAny("[(".ToCharArray());
				int close = language.LastIndexOfAny("])".ToCharArray());
				return index > 0 && index != close ? language.Substring(index + 1, close - index - 1) : string.Empty;
			}
			set
			{
				string language = CurrentLanguage;
				int index = language.IndexOfAny("/\\".ToCharArray());
				if (index > 0)
				{
					CurrentLanguage = language.Substring(index + 1) + value;
					return;
				}
				index = language.IndexOfAny("[(".ToCharArray());
				int close = language.LastIndexOfAny("])".ToCharArray());
				if (index > 0 && index != close)
				{
					language = language.Substring(index);
				}
				CurrentLanguage = language + "(" + value + ")";
			}
		}

		public static string CurrentRegionCode
		{
			get
			{
				string code = CurrentLanguageCode;
				int index = code.IndexOfAny(" -_/\\".ToCharArray());
				return index < 0 ? string.Empty : code.Substring(index + 1);
			}
			set
			{
				string code = CurrentLanguageCode;
				int index = code.IndexOfAny(" -_/\\".ToCharArray());
				if (index > 0)
				{
					code = code.Substring(0, index);
				}
				CurrentLanguageCode = code + "-" + value;
			}
		}

		public static CultureInfo CurrentCulture => mCurrentCulture;

		public static event OnLocalizeCallback OnLocalizeEvent
		{
			add
			{
				m_OnLocalizeEvent += value;
			}
			remove
			{
				m_OnLocalizeEvent -= value;
			}
		}

		public static void InitializeIfNeeded()
		{
			if (string.IsNullOrEmpty(mCurrentLanguage) || Sources.Count == 0)
			{
				AutoLoadGlobalParamManagers();
				UpdateSources();
				SelectStartupLanguage();
			}
		}

		public static string GetVersion()
		{
			return null;
		}

		public static int GetRequiredWebServiceVersion()
		{
			return 0;
		}

		public static string GetWebServiceURL(LanguageSourceData source = null)
		{
			return null;
		}

		public static void SetLanguageAndCode(string LanguageName, string LanguageCode, bool RememberLanguage = true, bool Force = false)
		{
			if (mCurrentLanguage != LanguageName || mLanguageCode != LanguageCode || Force)
			{
				if (RememberLanguage)
				{
					PersistentStorage.SetSetting_String("I2 Language", LanguageName);
				}
				mCurrentLanguage = LanguageName;
				mLanguageCode = LanguageCode;
				mCurrentCulture = CreateCultureForCode(LanguageCode);
				if (mChangeCultureInfo)
				{
					SetCurrentCultureInfo();
				}
				IsRight2Left = IsRTL(mLanguageCode);
				HasJoinedWords = GoogleLanguages.LanguageCode_HasJoinedWord(mLanguageCode);
				LocalizeAll(Force);
			}
		}

		private static CultureInfo CreateCultureForCode(string code)
		{
			try
			{
				return CultureInfo.CreateSpecificCulture(code);
			}
			catch (Exception)
			{
				return CultureInfo.InvariantCulture;
			}
		}

		public static void EnableChangingCultureInfo(bool bEnable)
		{
			if (!mChangeCultureInfo && bEnable)
			{
				SetCurrentCultureInfo();
			}
			mChangeCultureInfo = bEnable;
		}

		private static void SetCurrentCultureInfo()
		{
			Thread.CurrentThread.CurrentCulture = mCurrentCulture;
		}

		private static void SelectStartupLanguage()
		{
			if (Sources.Count == 0)
			{
				return;
			}
			string saved = PersistentStorage.GetSetting_String("I2 Language", string.Empty);
			string device = GetCurrentDeviceLanguage();
			if (!string.IsNullOrEmpty(saved) && HasLanguage(saved, true, false))
			{
				SetLanguageAndCode(saved, GetLanguageCode(saved));
				return;
			}
			if (!Sources[0].IgnoreDeviceLanguage)
			{
				string supported = GetSupportedLanguage(device, true);
				if (!string.IsNullOrEmpty(supported))
				{
					SetLanguageAndCode(supported, GetLanguageCode(supported), false);
					return;
				}
			}
			foreach (LanguageSourceData source in Sources)
			{
				if (source.mLanguages.Count > 0)
				{
					foreach (LanguageData language in source.mLanguages)
					{
						if (language.IsEnabled())
						{
							SetLanguageAndCode(language.Name, language.Code, false);
							return;
						}
					}
				}
			}
		}

		public static bool HasLanguage(string Language, bool AllowDiscartingRegion = true, bool Initialize = true, bool SkipDisabled = true)
		{
			if (Initialize)
			{
				InitializeIfNeeded();
			}
			foreach (LanguageSourceData source in Sources)
			{
				if (source.GetLanguageIndex(Language, false, SkipDisabled) >= 0)
				{
					return true;
				}
			}
			if (AllowDiscartingRegion)
			{
				foreach (LanguageSourceData source in Sources)
				{
					if (source.GetLanguageIndex(Language, true, SkipDisabled) >= 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static string GetSupportedLanguage(string Language, bool ignoreDisabled = false)
		{
			string code = GoogleLanguages.GetLanguageCode(Language);
			if (!string.IsNullOrEmpty(code))
			{
				foreach (LanguageSourceData source in Sources)
				{
					int index = source.GetLanguageIndexFromCode(code, true, ignoreDisabled);
					if (index >= 0)
					{
						return source.mLanguages[index].Name;
					}
				}
				foreach (LanguageSourceData source in Sources)
				{
					int index = source.GetLanguageIndexFromCode(code, false, ignoreDisabled);
					if (index >= 0)
					{
						return source.mLanguages[index].Name;
					}
				}
			}
			foreach (LanguageSourceData source in Sources)
			{
				int index = source.GetLanguageIndex(Language, false, ignoreDisabled);
				if (index >= 0)
				{
					return source.mLanguages[index].Name;
				}
			}
			foreach (LanguageSourceData source in Sources)
			{
				int index = source.GetLanguageIndex(Language, true, ignoreDisabled);
				if (index >= 0)
				{
					return source.mLanguages[index].Name;
				}
			}
			return string.Empty;
		}

		public static string GetLanguageCode(string Language)
		{
			if (Sources.Count == 0)
			{
				UpdateSources();
			}
			foreach (LanguageSourceData source in Sources)
			{
				int index = source.GetLanguageIndex(Language);
				if (index >= 0)
				{
					return source.mLanguages[index].Code;
				}
			}
			return string.Empty;
		}

		public static string GetLanguageFromCode(string Code, bool exactMatch = true)
		{
			if (Sources.Count == 0)
			{
				UpdateSources();
			}
			foreach (LanguageSourceData source in Sources)
			{
				int index = source.GetLanguageIndexFromCode(Code, exactMatch);
				if (index >= 0)
				{
					return source.mLanguages[index].Name;
				}
			}
			return null;
		}

		public static List<string> GetAllLanguages(bool SkipDisabled = true)
		{
			if (Sources.Count == 0)
			{
				UpdateSources();
			}
			List<string> languages = new List<string>();
			foreach (LanguageSourceData source in Sources)
			{
				foreach (string language in source.GetLanguages(SkipDisabled))
				{
					if (!languages.Contains(language))
					{
						languages.Add(language);
					}
				}
			}
			return languages;
		}

		public static List<string> GetAllLanguagesCode(bool allowRegions = true, bool SkipDisabled = true)
		{
			List<string> codes = new List<string>();
			foreach (LanguageSourceData source in Sources)
			{
				foreach (string code in source.GetLanguagesCode(allowRegions, SkipDisabled))
				{
					if (!codes.Contains(code))
					{
						codes.Add(code);
					}
				}
			}
			return codes;
		}

		public static bool IsLanguageEnabled(string Language)
		{
			foreach (LanguageSourceData source in Sources)
			{
				if (!source.IsLanguageEnabled(Language))
				{
					return false;
				}
			}
			return true;
		}

		private static void LoadCurrentLanguage()
		{
		}

		public static void PreviewLanguage(string NewLanguage)
		{
			mCurrentLanguage = NewLanguage;
			mLanguageCode = GetLanguageCode(mCurrentLanguage);
			IsRight2Left = IsRTL(mLanguageCode);
			HasJoinedWords = GoogleLanguages.LanguageCode_HasJoinedWord(mLanguageCode);
		}

		public static void AutoLoadGlobalParamManagers()
		{
		}

		public static void ApplyLocalizationParams(ref string translation, bool allowLocalizedParameters = true)
		{
			ApplyLocalizationParams(ref translation, (string name) => GetLocalizationParam(name, null), allowLocalizedParameters);
		}

		public static void ApplyLocalizationParams(ref string translation, GameObject root, bool allowLocalizedParameters = true)
		{
			ApplyLocalizationParams(ref translation, (string name) => GetLocalizationParam(name, root), allowLocalizedParameters);
		}

		public static void ApplyLocalizationParams(ref string translation, Dictionary<string, object> parameters, bool allowLocalizedParameters = true)
		{
			ApplyLocalizationParams(ref translation, delegate(string name)
			{
				if (name == null)
				{
					return null;
				}
				return parameters.TryGetValue(name, out object value) ? value : null;
			}, allowLocalizedParameters);
		}

		public static void ApplyLocalizationParams(ref string translation, _GetParam getParam, bool allowLocalizedParameters = true)
		{
			if (translation == null)
			{
				return;
			}
			if (CustomApplyLocalizationParams != null && CustomApplyLocalizationParams(ref translation, getParam, allowLocalizedParameters))
			{
				return;
			}
			int index = 0;
			int length = translation.Length;
			while (index < length)
			{
				int open = translation.IndexOf("{[", index, StringComparison.Ordinal);
				if (open < 0)
				{
					break;
				}
				int close = translation.IndexOf("]}", open, StringComparison.Ordinal);
				if (close < 0)
				{
					break;
				}
				int nested = translation.IndexOf("{[", open + 1, StringComparison.Ordinal);
				if (nested > 0 && nested < close)
				{
					index = nested;
					continue;
				}
				int offset = translation[open + 2] == '#' ? 3 : 2;
				string name = translation.Substring(open + offset, close - open - offset);
				string value = (string)getParam(name);
				if (value != null)
				{
					if (allowLocalizedParameters)
					{
						string term = GetTermTranslation(value);
						if (!string.IsNullOrEmpty(term))
						{
							value = term;
						}
					}
					string token = translation.Substring(open, close - open + 2);
					translation = translation.Replace(token, value);
					int removed = token.Length - value.Length;
					length -= removed;
					index = close + 2 - removed;
				}
				else
				{
					index = close + 2;
				}
			}
		}

		internal static string GetLocalizationParam(string ParamName, GameObject root)
		{
			if (root != null)
			{
				foreach (MonoBehaviour behaviour in root.GetComponents<MonoBehaviour>())
				{
					if (behaviour is ILocalizationParamsManager manager && behaviour.enabled)
					{
						string value = manager.GetParameterValue(ParamName);
						if (value != null)
						{
							return value;
						}
					}
				}
			}
			foreach (ILocalizationParamsManager manager in ParamManagers)
			{
				string value = manager.GetParameterValue(ParamName);
				if (value != null)
				{
					return value;
				}
			}
			return null;
		}

		private static string GetPluralType(MatchCollection matches, string langCode, _GetParam getParam)
		{
			return null;
		}

		public static string ApplyRTLfix(string line)
		{
			return ApplyRTLfix(line, 0, true);
		}

		public static string ApplyRTLfix(string line, int maxCharacters, bool ignoreNumbers)
		{
			return ApplyRTLfix(line, maxCharacters, ignoreNumbers, false);
		}

		public static string ApplyRTLfix(string line, int maxCharacters, bool ignoreNumbers, bool preserveRichText)
		{
			return line;
		}

		private static int[] FindRichTextTagPairs(List<string> tags)
		{
			return null;
		}

		private static string BalanceRichTextLines(string line, List<string> tags, List<char> markers, int[] pairs)
		{
			return null;
		}

		public static string FixRTL_IfNeeded(string text, int maxCharacters = 0, bool ignoreNumber = false)
		{
			return IsRight2Left ? ApplyRTLfix(text, maxCharacters, ignoreNumber) : text;
		}

		public static bool IsRTL(string Code)
		{
			return Array.IndexOf(LanguagesRTL, Code) >= 0;
		}

		public static bool UpdateSources()
		{
			UnregisterDeletededSources();
			RegisterSourceInResources();
			RegisterSceneSources();
			return Sources.Count > 0;
		}

		private static void UnregisterDeletededSources()
		{
			for (int i = Sources.Count - 1; i >= 0; i--)
			{
				if (Sources[i] == null)
				{
					Sources.RemoveAt(i);
				}
			}
		}

		private static void RegisterSceneSources()
		{
			foreach (LanguageSource source in UnityEngine.Object.FindObjectsByType<LanguageSource>(FindObjectsSortMode.None))
			{
				if (!Sources.Contains(source.mSource))
				{
					if (source.mSource.owner == null)
					{
						source.mSource.owner = source;
					}
					AddSource(source.mSource);
				}
			}
		}

		private static void RegisterSourceInResources()
		{
			foreach (string name in GlobalSources)
			{
				LanguageSourceAsset asset = Resources.Load<LanguageSourceAsset>(name);
				if (asset != null && !Sources.Contains(asset.mSource))
				{
					asset.mSource.mIsGlobalSource = true;
					asset.mSource.owner = asset;
					AddSource(asset.mSource);
				}
			}
		}

		private static bool AllowSyncFromGoogle(LanguageSourceData Source)
		{
			return Callback_AllowSyncFromGoogle == null || Callback_AllowSyncFromGoogle(Source);
		}

		internal static void AddSource(LanguageSourceData Source)
		{
			if (!Sources.Contains(Source))
			{
				Sources.Add(Source);
				Source.UpdateDictionary();
			}
		}

		private static IEnumerator Delayed_Import_Google(LanguageSourceData source, float delay, bool justCheck)
		{
			return null;
		}

		internal static void RemoveSource(LanguageSourceData Source)
		{
			Sources.Remove(Source);
		}

		public static bool IsGlobalSource(string SourceName)
		{
			return Array.IndexOf(GlobalSources, SourceName) >= 0;
		}

		public static LanguageSourceData GetSourceContaining(string term, bool fallbackToFirst = true)
		{
			if (!string.IsNullOrEmpty(term))
			{
				foreach (LanguageSourceData source in Sources)
				{
					if (source.GetTermData(term) != null)
					{
						return source;
					}
				}
			}
			return fallbackToFirst && Sources.Count > 0 ? Sources[0] : null;
		}

		public static UnityEngine.Object FindAsset(string value)
		{
			foreach (LanguageSourceData source in Sources)
			{
				UnityEngine.Object asset = source.FindAsset(value);
				if (asset != null)
				{
					return asset;
				}
			}
			return null;
		}

		public static void ApplyDownloadedDataFromGoogle()
		{
		}

		public static string GetCurrentDeviceLanguage(bool force = false)
		{
			if (force || string.IsNullOrEmpty(mCurrentDeviceLanguage))
			{
				DetectDeviceLanguage();
			}
			return mCurrentDeviceLanguage;
		}

		private static void DetectDeviceLanguage()
		{
			mCurrentDeviceLanguage = Application.systemLanguage.ToString();
			if (mCurrentDeviceLanguage == "ChineseSimplified")
			{
				mCurrentDeviceLanguage = "Chinese (Simplified)";
			}
			if (mCurrentDeviceLanguage == "ChineseTraditional")
			{
				mCurrentDeviceLanguage = "Chinese (Traditional)";
			}
		}

		public static void RegisterTarget(ILocalizeTargetDescriptor desc)
		{
			if (!mLocalizeTargets.Contains(desc))
			{
				mLocalizeTargets.Add(desc);
				mLocalizeTargets.Sort((ILocalizeTargetDescriptor a, ILocalizeTargetDescriptor b) => a.Priority.CompareTo(b.Priority));
			}
		}

		public static string GetTranslation(string Term, bool FixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, bool applyParameters = false, GameObject localParametersRoot = null, string overrideLanguage = null, bool allowLocalizedParameters = true)
		{
			return TryGetTranslation(Term, out string translation, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, overrideLanguage, allowLocalizedParameters) ? translation : null;
		}

		public static string GetTermTranslation(string Term, bool FixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, bool applyParameters = false, GameObject localParametersRoot = null, string overrideLanguage = null, bool allowLocalizedParameters = true)
		{
			return GetTranslation(Term, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot, overrideLanguage, allowLocalizedParameters);
		}

		public static bool TryGetTranslation(string Term, out string Translation, bool FixForRTL = true, int maxLineLengthForRTL = 0, bool ignoreRTLnumbers = true, bool applyParameters = false, GameObject localParametersRoot = null, string overrideLanguage = null, bool allowLocalizedParameters = true)
		{
			Translation = null;
			if (string.IsNullOrEmpty(Term))
			{
				return false;
			}
			InitializeIfNeeded();
			foreach (LanguageSourceData source in Sources)
			{
				if (source.TryGetTranslation(Term, out Translation, overrideLanguage))
				{
					if (applyParameters)
					{
						ApplyLocalizationParams(ref Translation, localParametersRoot, allowLocalizedParameters);
					}
					if (IsRight2Left && FixForRTL)
					{
						Translation = ApplyRTLfix(Translation, maxLineLengthForRTL, ignoreRTLnumbers);
					}
					return true;
				}
			}
			return false;
		}

		public static T GetTranslatedObject<T>(string AssetName, Localize optionalLocComp = null) where T : UnityEngine.Object
		{
			if (optionalLocComp != null)
			{
				return optionalLocComp.FindTranslatedObject<T>(AssetName);
			}
			T asset = FindAsset(AssetName) as T;
			return asset != null ? asset : ResourceManager.pInstance.GetAsset<T>(AssetName);
		}

		public static T GetTranslatedObjectByTermName<T>(string Term, Localize optionalLocComp = null) where T : UnityEngine.Object
		{
			return GetTranslatedObject<T>(GetTranslation(Term, false), optionalLocComp);
		}

		public static string GetAppName(string languageCode)
		{
			return null;
		}

		public static void LocalizeAll(bool Force = false)
		{
			mLocalizeIsScheduled = true;
			mLocalizeIsScheduledWithForcedValue |= Force;
			CoroutineManager.Start(Coroutine_LocalizeAll());
		}

		private static IEnumerator Coroutine_LocalizeAll()
		{
			yield return null;
			if (mLocalizeIsScheduled)
			{
				mLocalizeIsScheduled = false;
				bool force = mLocalizeIsScheduledWithForcedValue;
				mLocalizeIsScheduledWithForcedValue = false;
				DoLocalizeAll(force);
			}
		}

		private static void DoLocalizeAll(bool Force = false)
		{
			foreach (Localize localize in UnityEngine.Object.FindObjectsByType<Localize>(FindObjectsInactive.Include, FindObjectsSortMode.None))
			{
				localize.OnLocalize(Force);
			}
			m_OnLocalizeEvent?.Invoke();
			ResourceManager.pInstance.CleanResourceCache();
		}

		public static List<string> GetCategories()
		{
			List<string> categories = new List<string>();
			foreach (LanguageSourceData source in Sources)
			{
				source.GetCategories(false, categories);
			}
			return categories;
		}

		public static List<string> GetTermsList(string Category = null)
		{
			if (Sources.Count == 0)
			{
				UpdateSources();
			}
			if (Sources.Count == 1)
			{
				return Sources[0].GetTermsList(Category);
			}
			HashSet<string> terms = new HashSet<string>();
			foreach (LanguageSourceData source in Sources)
			{
				terms.UnionWith(source.GetTermsList(Category));
			}
			return new List<string>(terms);
		}

		public static TermData GetTermData(string term)
		{
			return GetTermData(term, out LanguageSourceData _);
		}

		public static TermData GetTermData(string term, out LanguageSourceData source)
		{
			InitializeIfNeeded();
			foreach (LanguageSourceData candidate in Sources)
			{
				TermData data = candidate.GetTermData(term);
				if (data != null)
				{
					source = candidate;
					return data;
				}
			}
			source = null;
			return null;
		}
	}
}

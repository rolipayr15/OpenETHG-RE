using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace I2.Loc
{
	[Serializable]
	[ExecuteInEditMode]
	public class LanguageSourceData
	{
		public enum MissingTranslationAction
		{
			Empty = 0,
			Fallback = 1,
			ShowWarning = 2,
			ShowTerm = 3
		}

		public enum eAllowUnloadLanguages
		{
			Never = 0,
			OnlyInDevice = 1,
			EditorAndDevice = 2
		}

		public enum eGoogleUpdateFrequency
		{
			Always = 0,
			Never = 1,
			Daily = 2,
			Weekly = 3,
			Monthly = 4,
			OnlyOnce = 5,
			EveryOtherDay = 6
		}

		public enum eGoogleUpdateSynchronization
		{
			Manual = 0,
			OnSceneLoaded = 1,
			AsSoonAsDownloaded = 2
		}


		[NonSerialized]
		public ILanguageSource owner;

		public bool UserAgreesToHaveItOnTheScene;

		public bool UserAgreesToHaveItInsideThePluginsFolder;

		public bool GoogleLiveSyncIsUptoDate;

		[NonSerialized]
		public bool mIsGlobalSource;

		public List<TermData> mTerms;

		public bool CaseInsensitiveTerms;

		[NonSerialized]
		public Dictionary<string, TermData> mDictionary = new Dictionary<string, TermData>(StringComparer.Ordinal);

		public MissingTranslationAction OnMissingTranslation;

		public string mTerm_AppName;

		public List<LanguageData> mLanguages;

		public bool IgnoreDeviceLanguage;

		public eAllowUnloadLanguages _AllowUnloadingLanguages;

		public string Google_WebServiceURL;

		public string Google_SpreadsheetKey;

		public string Google_SpreadsheetName;

		public string Google_LastUpdatedVersion;

		public eGoogleUpdateFrequency GoogleUpdateFrequency;

		public eGoogleUpdateFrequency GoogleInEditorCheckFrequency;

		public eGoogleUpdateSynchronization GoogleUpdateSynchronization;

		public float GoogleUpdateDelay;

		[CompilerGenerated]
		private LanguageSource.fnOnSourceUpdated m_Event_OnSourceUpdateFromGoogle;

		public List<UnityEngine.Object> Assets;

		[NonSerialized]
		public Dictionary<string, UnityEngine.Object> mAssetDictionary = new Dictionary<string, UnityEngine.Object>(StringComparer.Ordinal);

		private string mDelayedGoogleData;

		public static string EmptyCategory = "Default";

		public static char[] CategorySeparators = "/".ToCharArray();

		public UnityEngine.Object ownerObject => owner as UnityEngine.Object;

		public event LanguageSource.fnOnSourceUpdated Event_OnSourceUpdateFromGoogle
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public void Awake()
		{
			LocalizationManager.AddSource(this);
			UpdateDictionary();
			UpdateAssetDictionary();
			LocalizationManager.LocalizeAll(true);
		}

		public void OnDestroy()
		{
			LocalizationManager.RemoveSource(this);
		}

		public bool IsEqualTo(LanguageSourceData Source)
		{
			if (Source == null || Source.mLanguages.Count != mLanguages.Count)
			{
				return false;
			}
			for (int i = 0; i < mLanguages.Count; i++)
			{
				if (Source.GetLanguageIndex(mLanguages[i].Name) < 0)
				{
					return false;
				}
			}
			if (Source.mTerms.Count != mTerms.Count)
			{
				return false;
			}
			for (int i = 0; i < mTerms.Count; i++)
			{
				if (Source.GetTermData(mTerms[i].Term) == null)
				{
					return false;
				}
			}
			return true;
		}

		internal bool ManagerHasASimilarSource()
		{
			foreach (LanguageSourceData source in LocalizationManager.Sources)
			{
				if (source != null && source != this && source.IsEqualTo(this))
				{
					return true;
				}
			}
			return false;
		}

		public void ClearAllData()
		{
			mTerms.Clear();
			mLanguages.Clear();
			mDictionary.Clear();
			mAssetDictionary.Clear();
		}

		public bool IsGlobalSource()
		{
			return mIsGlobalSource;
		}

		public void Editor_SetDirty()
		{
		}

		public void UpdateAssetDictionary()
		{
			Assets.RemoveAll((UnityEngine.Object asset) => asset == null);
			Assets = Assets.Distinct().ToList();
			mAssetDictionary.Clear();
			foreach (UnityEngine.Object asset in Assets)
			{
				if (asset != null)
				{
					mAssetDictionary[asset.name] = asset;
				}
			}
		}

		public UnityEngine.Object FindAsset(string Name)
		{
			if (Assets == null)
			{
				return null;
			}
			if (mAssetDictionary == null || mAssetDictionary.Count != Assets.Count)
			{
				UpdateAssetDictionary();
			}
			return mAssetDictionary.TryGetValue(Name, out UnityEngine.Object asset) ? asset : null;
		}

		public bool HasAsset(UnityEngine.Object Obj)
		{
			return Assets.Contains(Obj);
		}

		public void AddAsset(UnityEngine.Object Obj)
		{
			if (!Assets.Contains(Obj))
			{
				Assets.Add(Obj);
				UpdateAssetDictionary();
			}
		}

		private string Export_Language_to_Cache(int langIndex, bool fillTermWithFallback)
		{
			return null;
		}

		public string Export_I2CSV(string Category, char Separator = ',', bool specializationsAsRows = true, bool sortRows = true)
		{
			return null;
		}

		private static void AppendI2Term(StringBuilder Builder, int nLanguages, string Term, TermData termData, char Separator, string forceSpecialization)
		{
		}

		private static void AppendI2Text(StringBuilder Builder, string text)
		{
		}

		public string Export_CSV(string Category, char Separator = ',', bool specializationsAsRows = true, bool sortRows = true)
		{
			return null;
		}

		private static void AppendTerm(StringBuilder Builder, int nLanguages, string Term, TermData termData, string specialization, char Separator)
		{
		}

		private static void AppendString(StringBuilder Builder, string Text, char Separator)
		{
		}

		private static void AppendTranslation(StringBuilder Builder, string Text, char Separator, string tags)
		{
		}

		public UnityWebRequest Export_Google_CreateWWWcall(eSpreadsheetUpdateMode UpdateMode = eSpreadsheetUpdateMode.Replace)
		{
			return null;
		}

		private string Export_Google_CreateData()
		{
			return null;
		}

		public string Import_CSV(string Category, string CSVstring, eSpreadsheetUpdateMode UpdateMode = eSpreadsheetUpdateMode.Replace, char Separator = ',')
		{
			return null;
		}

		public string Import_I2CSV(string Category, string I2CSVstring, eSpreadsheetUpdateMode UpdateMode = eSpreadsheetUpdateMode.Replace)
		{
			return null;
		}

		public string Import_CSV(string Category, List<string[]> CSV, eSpreadsheetUpdateMode UpdateMode = eSpreadsheetUpdateMode.Replace)
		{
			return null;
		}

		private bool ArrayContains(string MainText, params string[] texts)
		{
			return false;
		}

		public static eTermType GetTermType(string type)
		{
			return eTermType.Text;
		}

		private void Import_Language_from_Cache(int langIndex, string langData, bool useFallback, bool onlyCurrentSpecialization)
		{
		}

		public static void FreeUnusedLanguages()
		{
		}

		public void Import_Google_FromCache()
		{
		}

		private bool IsNewerVersion(string currentVersion, string newVersion)
		{
			return false;
		}

		public void Import_Google(bool ForceUpdate, bool justCheck)
		{
		}

		private string GetSourcePlayerPrefName()
		{
			return null;
		}

		private IEnumerator Import_Google_Coroutine(bool forceUpdate, bool JustCheck)
		{
			return null;
		}

		private void ApplyDownloadedDataOnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
		}

		public void ApplyDownloadedDataFromGoogle()
		{
		}

		public UnityWebRequest Import_Google_CreateWWWcall(bool ForceUpdate, bool justCheck)
		{
			return null;
		}

		public bool HasGoogleSpreadsheet()
		{
			return false;
		}

		public string Import_Google_Result(string JsonString, eSpreadsheetUpdateMode UpdateMode, bool saveInPlayerPrefs = false)
		{
			return null;
		}

		public int GetLanguageIndex(string language, bool AllowDiscartingRegion = true, bool SkipDisabled = true)
		{
			for (int i = 0; i < mLanguages.Count; i++)
			{
				if (string.Compare(mLanguages[i].Name, language, StringComparison.OrdinalIgnoreCase) == 0 && (!SkipDisabled || mLanguages[i].IsEnabled()))
				{
					return i;
				}
			}
			if (AllowDiscartingRegion)
			{
				int best = -1;
				int bestScore = 0;
				for (int i = 0; i < mLanguages.Count; i++)
				{
					if (!SkipDisabled || mLanguages[i].IsEnabled())
					{
						int score = GetCommonWordInLanguageNames(mLanguages[i].Name, language);
						if (score > bestScore)
						{
							bestScore = score;
							best = i;
						}
					}
				}
				if (best >= 0)
				{
					return best;
				}
			}
			return -1;
		}

		public LanguageData GetLanguageData(string language, bool AllowDiscartingRegion = true)
		{
			int index = GetLanguageIndex(language, AllowDiscartingRegion, false);
			return index >= 0 ? mLanguages[index] : null;
		}

		public bool IsCurrentLanguage(int languageIndex)
		{
			return LocalizationManager.CurrentLanguage == mLanguages[languageIndex].Name;
		}

		public int GetLanguageIndexFromCode(string Code, bool exactMatch = true, bool ignoreDisabled = false)
		{
			for (int i = 0; i < mLanguages.Count; i++)
			{
				if ((!ignoreDisabled || mLanguages[i].IsEnabled()) && string.Compare(mLanguages[i].Code, Code, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return i;
				}
			}
			if (!exactMatch)
			{
				for (int i = 0; i < mLanguages.Count; i++)
				{
					if ((!ignoreDisabled || mLanguages[i].IsEnabled()) && !string.IsNullOrEmpty(mLanguages[i].Code) && mLanguages[i].Code.Length >= 2 && Code != null && Code.Length >= 2 && string.Compare(mLanguages[i].Code, 0, Code, 0, 2, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return i;
					}
				}
			}
			return -1;
		}

		public static int GetCommonWordInLanguageNames(string Language1, string Language2)
		{
			if (string.IsNullOrEmpty(Language1) || string.IsNullOrEmpty(Language2))
			{
				return 0;
			}
			char[] separators = "( )-/\\".ToCharArray();
			string[] first = Language1.ToLowerInvariant().Split(separators);
			string[] second = Language2.ToLowerInvariant().Split(separators);
			int common = 0;
			foreach (string word in first)
			{
				if (!string.IsNullOrEmpty(word) && second.Contains(word))
				{
					common++;
				}
			}
			foreach (string word in second)
			{
				if (!string.IsNullOrEmpty(word) && first.Contains(word))
				{
					common++;
				}
			}
			return common;
		}

		public static bool AreTheSameLanguage(string Language1, string Language2)
		{
			return string.Compare(GetLanguageWithoutRegion(Language1), GetLanguageWithoutRegion(Language2), StringComparison.Ordinal) == 0;
		}

		public static string GetLanguageWithoutRegion(string Language)
		{
			int index = Language.IndexOfAny("(/\\[,{".ToCharArray());
			return index < 0 ? Language : Language.Substring(0, index).Trim();
		}

		public void AddLanguage(string LanguageName)
		{
			AddLanguage(LanguageName, GoogleLanguages.GetLanguageCode(LanguageName));
		}

		public void AddLanguage(string LanguageName, string LanguageCode)
		{
			if (GetLanguageIndex(LanguageName, false) >= 0)
			{
				return;
			}
			mLanguages.Add(new LanguageData { Name = LanguageName, Code = LanguageCode });
			foreach (TermData term in mTerms)
			{
				Array.Resize(ref term.Languages, mLanguages.Count);
				Array.Resize(ref term.Flags, mLanguages.Count);
			}
		}

		public void RemoveLanguage(string LanguageName)
		{
		}

		public List<string> GetLanguages(bool skipDisabled = true)
		{
			List<string> languages = new List<string>();
			foreach (LanguageData language in mLanguages)
			{
				if (!skipDisabled || language.IsEnabled())
				{
					languages.Add(language.Name);
				}
			}
			return languages;
		}

		public List<string> GetLanguagesCode(bool allowRegions = true, bool skipDisabled = true)
		{
			List<string> codes = new List<string>();
			foreach (LanguageData language in mLanguages)
			{
				if (!skipDisabled || language.IsEnabled())
				{
					string code = language.Code;
					if (!allowRegions && code != null && code.Length > 2)
					{
						code = code.Substring(0, 2);
					}
					if (!string.IsNullOrEmpty(code) && !codes.Contains(code))
					{
						codes.Add(code);
					}
				}
			}
			return codes;
		}

		public bool IsLanguageEnabled(string Language)
		{
			int index = GetLanguageIndex(Language, false);
			return index >= 0 && mLanguages[index].IsEnabled();
		}

		public void EnableLanguage(string Language, bool bEnabled)
		{
			int index = GetLanguageIndex(Language, false, false);
			if (index >= 0)
			{
				mLanguages[index].SetEnabled(bEnabled);
			}
		}

		public bool AllowUnloadingLanguages()
		{
			return false;
		}

		private string GetSavedLanguageFileName(int languageIndex)
		{
			return null;
		}

		public void LoadLanguage(int languageIndex, bool UnloadOtherLanguages, bool useFallback, bool onlyCurrentSpecialization, bool forceLoad)
		{
		}

		public void LoadAllLanguages(bool forceLoad = false)
		{
		}

		public void UnloadLanguage(int languageIndex)
		{
		}

		public void SaveLanguages(bool unloadAll, PersistentStorage.eFileType fileLocation = PersistentStorage.eFileType.Temporal)
		{
		}

		public bool HasUnloadedLanguages()
		{
			return false;
		}

		public List<string> GetCategories(bool OnlyMainCategory = false, List<string> Categories = null)
		{
			List<string> categories = Categories ?? new List<string>();
			foreach (TermData term in mTerms)
			{
				string category = GetCategoryFromFullTerm(term.Term, OnlyMainCategory);
				if (!categories.Contains(category))
				{
					categories.Add(category);
				}
			}
			categories.Sort();
			return categories;
		}

		public static string GetKeyFromFullTerm(string FullTerm, bool OnlyMainCategory = false)
		{
			int index = OnlyMainCategory ? FullTerm.IndexOfAny(CategorySeparators) : FullTerm.LastIndexOfAny(CategorySeparators);
			return index < 0 ? FullTerm : FullTerm.Substring(index + 1);
		}

		public static string GetCategoryFromFullTerm(string FullTerm, bool OnlyMainCategory = false)
		{
			int index = OnlyMainCategory ? FullTerm.IndexOfAny(CategorySeparators) : FullTerm.LastIndexOfAny(CategorySeparators);
			return index < 0 ? EmptyCategory : FullTerm.Substring(0, index);
		}

		public static void DeserializeFullTerm(string FullTerm, out string Key, out string Category, bool OnlyMainCategory = false)
		{
			int index = OnlyMainCategory ? FullTerm.IndexOfAny(CategorySeparators) : FullTerm.LastIndexOfAny(CategorySeparators);
			if (index < 0)
			{
				Category = EmptyCategory;
				Key = FullTerm;
			}
			else
			{
				Category = FullTerm.Substring(0, index);
				Key = FullTerm.Substring(index + 1);
			}
		}

		public void UpdateDictionary(bool force = false)
		{
			if (!force && mDictionary != null && mDictionary.Count == mTerms.Count)
			{
				return;
			}
			StringComparer comparer = CaseInsensitiveTerms ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
			if (mDictionary == null || mDictionary.Comparer != comparer)
			{
				mDictionary = new Dictionary<string, TermData>(comparer);
			}
			else
			{
				mDictionary.Clear();
			}
			foreach (TermData term in mTerms)
			{
				string name = term.Term;
				ValidateFullTerm(ref name);
				mDictionary[name] = term;
				term.Validate();
			}
		}

		public string GetTranslation(string term, string overrideLanguage = null, string overrideSpecialization = null, bool skipDisabled = false, bool allowCategoryMistmatch = false)
		{
			return TryGetTranslation(term, out string translation, overrideLanguage, overrideSpecialization, skipDisabled, allowCategoryMistmatch) ? translation : null;
		}

		public bool TryGetTranslation(string term, out string Translation, string overrideLanguage = null, string overrideSpecialization = null, bool skipDisabled = false, bool allowCategoryMistmatch = false)
		{
			int index = GetLanguageIndex(overrideLanguage ?? LocalizationManager.CurrentLanguage, true, false);
			if (index >= 0 && (!skipDisabled || mLanguages[index].IsEnabled()))
			{
				TermData data = GetTermData(term, allowCategoryMistmatch);
				if (data != null)
				{
					Translation = data.GetTranslation(index, overrideSpecialization, true);
					if (Translation == "---")
					{
						Translation = string.Empty;
						return true;
					}
					if (!string.IsNullOrEmpty(Translation))
					{
						return true;
					}
					Translation = null;
				}
				switch (OnMissingTranslation)
				{
				case MissingTranslationAction.ShowWarning:
					Translation = "<!-Missing Translation [" + term + "]-!>";
					return true;
				case MissingTranslationAction.Fallback:
					if (data != null)
					{
						return TryGetFallbackTranslation(data, out Translation, index, overrideSpecialization, skipDisabled);
					}
					break;
				case MissingTranslationAction.Empty:
					Translation = string.Empty;
					return true;
				case MissingTranslationAction.ShowTerm:
					Translation = term;
					return true;
				}
			}
			Translation = null;
			return false;
		}

		private bool TryGetFallbackTranslation(TermData termData, out string Translation, int langIndex, string overrideSpecialization = null, bool skipDisabled = false)
		{
			string baseLanguage = mLanguages[langIndex].Code;
			if (!string.IsNullOrEmpty(baseLanguage) && baseLanguage.Length >= 2)
			{
				baseLanguage = baseLanguage.Substring(0, 2);
				for (int i = 0; i < mLanguages.Count; i++)
				{
					if (i != langIndex && !string.IsNullOrEmpty(mLanguages[i].Code) && mLanguages[i].Code.StartsWith(baseLanguage, StringComparison.Ordinal) && (!skipDisabled || mLanguages[i].IsEnabled()))
					{
						Translation = termData.GetTranslation(i, overrideSpecialization, true);
						if (!string.IsNullOrEmpty(Translation))
						{
							return true;
						}
					}
				}
			}
			for (int i = 0; i < mLanguages.Count; i++)
			{
				if (i != langIndex && (!skipDisabled || mLanguages[i].IsEnabled()))
				{
					Translation = termData.GetTranslation(i, overrideSpecialization, true);
					if (!string.IsNullOrEmpty(Translation))
					{
						return true;
					}
				}
			}
			Translation = null;
			return false;
		}

		public TermData AddTerm(string term)
		{
			return null;
		}

		public TermData GetTermData(string term, bool allowCategoryMistmatch = false)
		{
			if (string.IsNullOrEmpty(term))
			{
				return null;
			}
			if (mDictionary == null || mDictionary.Count == 0)
			{
				UpdateDictionary();
			}
			if (mDictionary.TryGetValue(term, out TermData data))
			{
				return data;
			}
			if (allowCategoryMistmatch)
			{
				string key = GetKeyFromFullTerm(term);
				foreach (KeyValuePair<string, TermData> pair in mDictionary)
				{
					if (pair.Value.IsTerm(key, true))
					{
						return pair.Value;
					}
				}
			}
			return null;
		}

		public bool ContainsTerm(string term)
		{
			return GetTermData(term) != null;
		}

		public List<string> GetTermsList(string Category = null)
		{
			if (mDictionary == null || mDictionary.Count != mTerms.Count)
			{
				UpdateDictionary();
			}
			if (string.IsNullOrEmpty(Category))
			{
				return new List<string>(mDictionary.Keys);
			}
			List<string> terms = new List<string>();
			foreach (TermData term in mTerms)
			{
				if (GetCategoryFromFullTerm(term.Term) == Category)
				{
					terms.Add(term.Term);
				}
			}
			return terms;
		}

		public TermData AddTerm(string NewTerm, eTermType termType, bool SaveSource = true)
		{
			return null;
		}

		public void RemoveTerm(string term)
		{
		}

		public static void ValidateFullTerm(ref string Term)
		{
			Term = Term.Replace('\\', '/');
			Term = Term.Trim();
			if (Term.StartsWith(EmptyCategory, StringComparison.Ordinal) && Term.Length > EmptyCategory.Length && Term[EmptyCategory.Length] == '/')
			{
				Term = Term.Substring(EmptyCategory.Length + 1);
			}
		}
	}
}

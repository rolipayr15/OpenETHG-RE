using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace I2.Loc
{
	[AddComponentMenu("I2/Localization/Source")]
	[ExecuteInEditMode]
	public class LanguageSource : MonoBehaviour, ISerializationCallbackReceiver, ILanguageSource
	{
		public delegate void fnOnSourceUpdated(LanguageSourceData source, bool ReceivedNewData, string errorMsg);

		public LanguageSourceData mSource;

		public int version;

		public bool NeverDestroy;

		public bool UserAgreesToHaveItOnTheScene;

		public bool UserAgreesToHaveItInsideThePluginsFolder;

		public bool GoogleLiveSyncIsUptoDate;

		public List<Object> Assets;

		public string Google_WebServiceURL;

		public string Google_SpreadsheetKey;

		public string Google_SpreadsheetName;

		public string Google_LastUpdatedVersion;

		public LanguageSourceData.eGoogleUpdateFrequency GoogleUpdateFrequency;

		public float GoogleUpdateDelay;

		[CompilerGenerated]
		private fnOnSourceUpdated m_Event_OnSourceUpdateFromGoogle;

		public List<LanguageData> mLanguages;

		public bool IgnoreDeviceLanguage;

		public LanguageSourceData.eAllowUnloadLanguages _AllowUnloadingLanguages;

		public List<TermData> mTerms;

		public bool CaseInsensitiveTerms;

		public LanguageSourceData.MissingTranslationAction OnMissingTranslation;

		public string mTerm_AppName;

		public LanguageSourceData SourceData
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public event fnOnSourceUpdated Event_OnSourceUpdateFromGoogle
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

		private void Awake()
		{
			mSource.owner = this;
			mSource.Awake();
		}

		private void OnDestroy()
		{
			NeverDestroy = false;
			if (!NeverDestroy)
			{
				mSource.OnDestroy();
			}
		}

		public string GetSourceName()
		{
			string name = gameObject.name;
			Transform parent = transform.parent;
			while (parent != null)
			{
				name = parent.name + "_" + name;
				parent = parent.parent;
			}
			return name;
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
		}
	}
}

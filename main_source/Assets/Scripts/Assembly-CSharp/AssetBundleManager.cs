using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Google.Play.AssetDelivery;
using Google.Play.Common;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class AssetBundleManager : MonoBehaviour
{
	private struct RequestProgressSnapshot
	{
		public PlayAssetBundleRequest Request;

		public AssetDeliveryStatus Status;

		public float DownloadProgress;
	}

	[CompilerGenerated]
	private sealed class _003CDownloadAfterPreviousRequestCompletes_003Ed__49 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PlayAssetBundleRequest previousRequest;

		public AssetBundleManager _003C_003E4__this;

		public string bundleName;

		private float _003CwaitTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003CDownloadAfterPreviousRequestCompletes_003Ed__49(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003CDownloadAndLoadBundles_003Ed__26 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public string[] _rawBundleNames;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003CDownloadAndLoadBundles_003Ed__26(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003CKeepCheckingStatus_003Ed__50 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AssetBundleManager _003C_003E4__this;

		private float _003CpreviousCheckTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003CKeepCheckingStatus_003Ed__50(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003CShowAssetDeliveryConfirmation_003Ed__55 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AssetBundleManager _003C_003E4__this;

		private PlayAsyncOperation<ConfirmationDialogResult, AssetDeliveryErrorCode> _003CconfirmationOperation_003E5__2;

		private float _003CwaitTime_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003CShowAssetDeliveryConfirmation_003Ed__55(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003CStart_003Ed__18 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public AssetBundleManager _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003CStart_003Ed__18(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003CUnloadManagerSceneAndCallError_003Ed__31 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Action<string> errorCallback;

		public string message;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003CUnloadManagerSceneAndCallError_003Ed__31(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003CUnloadManagerSceneAndCallSuccess_003Ed__30 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Action<AssetBundle> successCallback;

		public AssetBundle bundle;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003CUnloadManagerSceneAndCallSuccess_003Ed__30(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003C_FetchBundlesDownloadSizes_003Ed__44 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private int _003Ci_003E5__2;

		private PlayAsyncOperation<long, AssetDeliveryErrorCode> _003CasyncOp_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003C_FetchBundlesDownloadSizes_003Ed__44(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	private static string[] allBundleNames;

	private static Dictionary<string, LocalizedString> bundlesHumanReadableNames;

	private static Dictionary<string, int> bundleSizes;

	[SerializeField]
	private Text statusLabel;

	private StringBuilder statusSb;

	private static HashSet<AssetBundle> loadedBundles;

	private static Action<AssetBundle> globalSuccessCallback;

	private static Action<string> globalErrorCallback;

	private static List<string> bundleNames;

	private static string targetBundleName;

	private static bool requestInProgress;

	private bool cleanupStarted;

	private const float AssetDeliveryOperationTimeoutSeconds = 120f;

	private static Dictionary<AssetDeliveryStatus, LocalizedString> deliveryStatusHumanReadableNames;

	private static Dictionary<AssetDeliveryErrorCode, LocalizedString> deliveryErrorCodeReadableNames;

	private static readonly HashSet<PlayAssetBundleRequest> requestsPendingCleanup;

	private readonly Dictionary<string, RequestProgressSnapshot> requestProgressSnapshots;

	private bool confirmationDialogInProgress;

	private float requestStalledSeconds;

	private void _tryCancelRequests()
	{
	}

	private static void _perPlatform_UnloadAllBundles()
	{
	}

	private void _perPlatform_InitRequest()
	{
	}

	private void _perPlatform_PopulateStatusString(StringBuilder sb)
	{
	}

	public static void MakeSureBundlesAreDownloadedAndLoaded(Action<AssetBundle> successCallback, Action<string> errorCallback, params string[] bundles)
	{
	}

	[IteratorStateMachine(typeof(_003CStart_003Ed__18))]
	private IEnumerator Start()
	{
		return null;
	}

	private void UpdateStatusLabel()
	{
	}

	public static int GetSize(string bundleName)
	{
		return 0;
	}

	public static void FetchBundlesDownloadSizes()
	{
	}

	public static string GetHumanReadableNameForBundle(string bundleName)
	{
		return null;
	}

	private static string GetInternalBundleError(string details)
	{
		return null;
	}

	private static AssetBundle GetLoadedBundle(string bundleName)
	{
		return null;
	}

	public static string GetDownloadSizeNoticeTextForBundles(params string[] listOfBundles)
	{
		return null;
	}

	[IteratorStateMachine(typeof(_003CDownloadAndLoadBundles_003Ed__26))]
	private static IEnumerator DownloadAndLoadBundles(string[] _rawBundleNames)
	{
		return null;
	}

	public static bool LevelInAssetBundle(Level level)
	{
		return false;
	}

	private void CleanupAndCallSuccessCallback(AssetBundle bundle)
	{
	}

	private void CleanupAndCallErrorCallback(string message)
	{
	}

	[IteratorStateMachine(typeof(_003CUnloadManagerSceneAndCallSuccess_003Ed__30))]
	private static IEnumerator UnloadManagerSceneAndCallSuccess(AssetBundle bundle, Action<AssetBundle> successCallback)
	{
		return null;
	}

	[IteratorStateMachine(typeof(_003CUnloadManagerSceneAndCallError_003Ed__31))]
	private static IEnumerator UnloadManagerSceneAndCallError(string message, Action<string> errorCallback)
	{
		return null;
	}

	private static void CompleteWithoutManagerSuccess(AssetBundle bundle)
	{
	}

	private static void CompleteWithoutManagerError(string message)
	{
	}

	public static void UnloadAllBundles()
	{
	}

	public void CancelButtonClicked()
	{
	}

	[IteratorStateMachine(typeof(_003C_FetchBundlesDownloadSizes_003Ed__44))]
	private static IEnumerator _FetchBundlesDownloadSizes()
	{
		return null;
	}

	public static string[] FilterBundleNamesThatNeedDownloading(params string[] listOfBundles)
	{
		return null;
	}

	public static string GetHumanReadableNameForStatus(AssetDeliveryStatus status)
	{
		return null;
	}

	public static string GetHumanReadableNameForError(AssetDeliveryErrorCode errorCode)
	{
		return null;
	}

	private void DownloadAssetBundle(string bundleName)
	{
	}

	[IteratorStateMachine(typeof(_003CDownloadAfterPreviousRequestCompletes_003Ed__49))]
	private IEnumerator DownloadAfterPreviousRequestCompletes(string bundleName, PlayAssetBundleRequest previousRequest)
	{
		return null;
	}

	[IteratorStateMachine(typeof(_003CKeepCheckingStatus_003Ed__50))]
	private IEnumerator KeepCheckingStatus()
	{
		return null;
	}

	private bool CurrentRequestsMadeProgress()
	{
		return false;
	}

	private static void BundleRequestCompleted(PlayAssetBundleRequest request)
	{
	}

	private static void DiscardBundleRequestCompleted(PlayAssetBundleRequest request)
	{
	}

	private void CheckStatuses()
	{
	}

	[IteratorStateMachine(typeof(_003CShowAssetDeliveryConfirmation_003Ed__55))]
	private IEnumerator ShowAssetDeliveryConfirmation()
	{
		return null;
	}

	private bool CurrentRequestRequiresConfirmation()
	{
		return false;
	}

	private void Update()
	{
	}
}

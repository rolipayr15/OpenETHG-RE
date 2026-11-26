using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DarkTonic.MasterAudio
{
	[AudioScriptOrder(-40)]
	[RequireComponent(typeof(SoundGroupVariationUpdater))]
	public class SoundGroupVariation : MonoBehaviour
	{
		public delegate void SoundFinishedEventHandler();

		public delegate void SoundLoopedEventHandler(int loopNumberStarted);

		public class PlaySoundParams
		{
			public string SoundType;

			public float VolumePercentage;

			public float? Pitch;

			public double? TimeToSchedulePlay;

			public Transform SourceTrans;

			public bool AttachToSource;

			public float DelaySoundTime;

			public bool IsChainLoop;

			public bool IsSingleSubscribedPlay;

			public float GroupCalcVolume;

			public bool IsPlaying;

			public PlaySoundParams(string soundType, float volPercent, float groupCalcVolume, float? pitch, Transform sourceTrans, bool attach, float delaySoundTime, double? timeToSchedulePlay, bool isChainLoop, bool isSingleSubscribedPlay)
			{
			}
		}

		public enum PitchMode
		{
			None = 0,
			Gliding = 1
		}

		public enum FadeMode
		{
			None = 0,
			FadeInOut = 1,
			FadeOutEarly = 2,
			GradualFade = 3
		}

		public enum RandomPitchMode
		{
			AddToClipPitch = 0,
			IgnoreClipPitch = 1
		}

		public enum RandomVolumeMode
		{
			AddToClipVolume = 0,
			IgnoreClipVolume = 1
		}

		public enum DetectEndMode
		{
			None = 0,
			DetectEnd = 1
		}

		[CompilerGenerated]
		private sealed class _003CWaitForLoadToUnloadClipAndDeactivate_003Ed__113 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public SoundGroupVariation _003C_003E4__this;

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
			public _003CWaitForLoadToUnloadClipAndDeactivate_003Ed__113(int _003C_003E1__state)
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

		public int weight;

		[Range(0f, 1f)]
		public int probabilityToPlay;

		[Range(0f, 10f)]
		public int importance;

		public bool isUninterruptible;

		public bool useLocalization;

		public bool useRandomPitch;

		public RandomPitchMode randomPitchMode;

		public float randomPitchMin;

		public float randomPitchMax;

		public bool useRandomVolume;

		public RandomVolumeMode randomVolumeMode;

		public float randomVolumeMin;

		public float randomVolumeMax;

		public string clipAlias;

		public MasterAudio.AudioLocation audLocation;

		public string resourceFileName;

		public float original_pitch;

		public float original_volume;

		public bool isExpanded;

		public bool isChecked;

		public bool useFades;

		public float fadeInTime;

		public float fadeOutTime;

		public bool useCustomLooping;

		public int minCustomLoops;

		public int maxCustomLoops;

		public bool useRandomStartTime;

		public float randomStartMinPercent;

		public float randomStartMaxPercent;

		public float randomEndPercent;

		public bool useIntroSilence;

		public float introSilenceMin;

		public float introSilenceMax;

		public float fadeMaxVolume;

		public FadeMode curFadeMode;

		public PitchMode curPitchMode;

		public DetectEndMode curDetectEndMode;

		public int frames;

		private AudioSource _audioSource;

		private readonly PlaySoundParams _playSndParam;

		private AudioDistortionFilter _distFilter;

		private AudioEchoFilter _echoFilter;

		private AudioHighPassFilter _hpFilter;

		private AudioLowPassFilter _lpFilter;

		private AudioReverbFilter _reverbFilter;

		private AudioChorusFilter _chorusFilter;

		private string _objectName;

		private float _maxVol;

		private int _instanceId;

		private bool? _audioLoops;

		private int _maxLoops;

		private SoundGroupVariationUpdater _varUpdater;

		private int _previousSoundFinishedFrame;

		private string _soundGroupName;

		private MasterAudio.VariationLoadStatus _loadStatus;

		private bool _isStopRequested;

		private bool _isPaused;

		private bool _isWarmingPlay;

		[CompilerGenerated]
		private SoundFinishedEventHandler m_SoundFinished;

		[CompilerGenerated]
		private SoundLoopedEventHandler m_SoundLooped;

		private Transform _trans;

		private GameObject _go;

		private Transform _objectToFollow;

		private Transform _objectToTriggerFrom;

		private MasterAudioGroup _parentGroupScript;

		private bool _attachToSource;

		private string _resFileName;

		private bool _hasStartedEndLinkedGroups;

		private Coroutine _loadResourceFileCoroutine;

		private Coroutine _loadAddressableCoroutine;

		private bool _isUnloadAddressableCoroutineRunning;

		private TransformFollower _ambientFollower;

		public TransformFollower AmbientFollower => null;

		public AudioDistortionFilter DistortionFilter => null;

		public AudioReverbFilter ReverbFilter => null;

		public AudioChorusFilter ChorusFilter => null;

		public AudioEchoFilter EchoFilter => null;

		public AudioLowPassFilter LowPassFilter
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public AudioHighPassFilter HighPassFilter => null;

		public Transform ObjectToFollow
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public Transform ObjectToTriggerFrom
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public bool HasActiveFXFilter => false;

		public MasterAudioGroup ParentGroup => _parentGroupScript;

		public float OriginalPitch => original_pitch;

		public float OriginalVolume => original_volume;

		public string SoundGroupName => (_parentGroupScript != null) ? _parentGroupScript.name : null;

		public bool IsAvailableToPlay
		{
			get
			{
				if (weight == 0)
				{
					return false;
				}
				if (!rtPlaying)
				{
					return true;
				}
				AudioClip clip = VarAudio.clip;
				return clip != null && clip.length > 0f && _parentGroupScript != null && VarAudio.time / clip.length * 100f >= (float)_parentGroupScript.retriggerPercentage && _parentGroupScript.retriggerPercentage < 100;
			}
		}

		public float LastTimePlayed { get; set; }

		public bool ClipIsLoaded => VarAudio != null && VarAudio.clip != null;

		public bool IsPlaying => rtPlaying;

		public MasterAudio.VariationLoadStatus LoadStatus
		{
			get
			{
				return MasterAudio.VariationLoadStatus.None;
			}
			set
			{
			}
		}

		public int InstanceId => GetInstanceID();

		public bool IsStopRequested => _isStopRequested;

		public Transform Trans => base.transform;

		public GameObject GameObj => base.gameObject;

		public AudioSource VarAudio
		{
			get
			{
				if (_audioSource == null)
				{
					_audioSource = GetComponent<AudioSource>();
				}
				return _audioSource;
			}
		}

		public bool AudioLoops => VarAudio.loop;

		public string ResFileName => null;

		public SoundGroupVariationUpdater VariationUpdater
		{
			get
			{
				if (_varUpdater == null)
				{
					_varUpdater = GetComponent<SoundGroupVariationUpdater>();
				}
				return _varUpdater;
			}
		}

		public PlaySoundParams PlaySoundParm => null;

		public float SetGroupVolume
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public int MaxLoops => _maxLoops;

		private bool Is2D => false;

		public bool UsesOcclusion => false;

		public bool IsPaused => _isPaused;

		public string GameObjectName
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public event SoundFinishedEventHandler SoundFinished
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

		public event SoundLoopedEventHandler SoundLooped
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
		}

		private void Start()
		{
		}

		public void SetMixerGroup()
		{
		}

		public void SetSpatialBlend()
		{
		}

		private void SetOcclusion()
		{
		}

		private void SetPriority()
		{
		}

		public void DisableUpdater()
		{
		}

		private void OnDestroy()
		{
			if (rtPlaying && _parentGroupScript != null)
			{
				_parentGroupScript.RtActive.Remove(this);
			}
		}

		private void OnDisable()
		{
		}

		private void StopSoundEarly()
		{
		}

		public void Play(float? pitch, float maxVolume, string gameObjectName, float volPercent, float targetVol, float? targetPitch, Transform sourceTrans, bool attach, float delayTime, double? timeToSchedulePlay, bool isChaining, bool isSingleSubscribedPlay)
		{
		}

		public void SetPlaySoundParams(string gameObjectName, float volPercent, float targetVol, float? targetPitch, Transform sourceTrans, bool attach, float delayTime, double? timeToSchedulePlay, bool isChaining, bool isSingleSubscribedPlay)
		{
		}

		private void MaybeCleanupFinishedDelegate()
		{
		}

		private void ResourceFailedToLoad()
		{
		}

		private void FinishSetupToPlay()
		{
		}

		public void JumpToTime(float timeToJumpTo)
		{
		}

		public void GlideByPitch(float pitchAddition, float glideTime, Action completionCallback = null)
		{
		}

		public void AdjustVolume(float volumePercentage)
		{
			rtPercent = volumePercentage;
			RtApplyVolume();
		}

		public void Pause()
		{
			if (rtPlaying && !_isPaused)
			{
				_isPaused = true;
				VarAudio.Pause();
			}
		}

		public void PlayVideo()
		{
		}

		public void StopVideo()
		{
		}

		public void Unpause()
		{
			if (rtPlaying && _isPaused)
			{
				_isPaused = false;
				VarAudio.UnPause();
			}
		}

		public void DoNextChain(float volumePercentage, float? pitch, Transform transActor, bool attach)
		{
		}

		public void PlayEndLinkedGroups(double? timeToPlayClip = null)
		{
		}

		private void EnableUpdater(bool waitForSoundFinish = true)
		{
		}

		private void MaybeUnloadClip()
		{
		}

		private void PlayEndLinkedGroup(string sType, double? timeToPlayClip = null)
		{
		}

		public void Stop(bool stopEndDetection = false, bool skipLinked = false)
		{
			if (!rtPlaying)
			{
				return;
			}
			_isStopRequested = true;
			rtPlaying = false;
			_isPaused = false;
			_objectToFollow = null;
			_objectToTriggerFrom = null;
			rtFadeTime = 0f;
			if (_audioSource != null)
			{
				_audioSource.Stop();
			}
			if (_parentGroupScript != null)
			{
				_parentGroupScript.RtActive.Remove(this);
			}
			SoundFinishedEventHandler finished = m_SoundFinished;
			if (finished != null)
			{
				finished();
			}
			if (_parentGroupScript != null && _parentGroupScript.RtActive.Count == 0)
			{
				_parentGroupScript.FireLastVariationFinishedPlay();
			}
			if (base.gameObject != null && base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(false);
			}
		}

		private void StopEndCleanup()
		{
		}

		[IteratorStateMachine(typeof(_003CWaitForLoadToUnloadClipAndDeactivate_003Ed__113))]
		private IEnumerator WaitForLoadToUnloadClipAndDeactivate()
		{
			return null;
		}

		public void FadeToVolume(float newVolume, float fadeTime, Action completionCallback = null)
		{
			RtBeginFade(newVolume, fadeTime, false, completionCallback);
		}

		public void FadeOutNowAndStop(Action completionCallback = null)
		{
			RtBeginFade(0f, fadeOutTime, true, completionCallback);
		}

		public void FadeOutNowAndStop(float fadeTime, Action completionCallback = null)
		{
			RtBeginFade(0f, fadeTime, true, completionCallback);
		}

		public void MoveToAmbientColliderPosition(Vector3 newPosition, TransformFollower follower)
		{
		}

		public void UpdateAudioVariation(TransformFollower transformFollower)
		{
		}

		public bool WasTriggeredFromTransform(Transform trans)
		{
			return trans != null && (_objectToTriggerFrom == trans || _objectToFollow == trans);
		}

		public bool WasTriggeredFromAnyOfTransformMap(HashSet<Transform> transMap)
		{
			return false;
		}

		public void UpdateTransformTracker(Transform sourceTrans)
		{
			_objectToTriggerFrom = sourceTrans;
		}

		public void SoundLoopStarted(int numberOfLoops)
		{
		}

		public void ClearSubscribers()
		{
			m_SoundFinished = null;
			m_SoundLooped = null;
		}

		private bool rtPlaying;

		private float rtPercent = 1f;

		private float rtRandomVolume;

		private float rtFade = 1f;

		private float rtFadeFrom;

		private float rtFadeTo;

		private float rtFadeTime;

		private float rtFadeElapsed;

		private bool rtFadeStops;

		private Action rtFadeDone;

		private bool rtFollowing;

		private bool rtEndFadeStarted;

		private float rtLastTime;

		private int rtLoopsLeft;

		private bool rtLoopedChain;

		private float? rtChainPitch;

		private Transform rtChainSource;

		private Vector3? rtChainPosition;

		private bool rtChainFollow;

		// Prepares this voice when its group is indexed by the mixer.
		internal void RtInit(MasterAudioGroup group)
		{
			_parentGroupScript = group;
			_audioSource = GetComponent<AudioSource>();
			original_pitch = _audioSource.pitch;
			original_volume = _audioSource.volume;
			_audioSource.playOnAwake = false;
			if (_audioSource.isPlaying)
			{
				_audioSource.Stop();
			}
			base.gameObject.SetActive(false);
		}

		internal void RtStart(float volumePercentage, float? pitch, float delay, Transform source, Vector3? position, bool follow)
		{
			if (rtPlaying)
			{
				Stop();
			}
			AudioSource audio = VarAudio;
			if (audio.clip == null)
			{
				return;
			}
			base.gameObject.SetActive(true);
			_isStopRequested = false;
			_isPaused = false;
			rtPlaying = true;
			_parentGroupScript.RtActive.Add(this);
			rtPercent = volumePercentage;
			rtFadeTime = 0f;
			rtEndFadeStarted = false;
			rtLoopedChain = _parentGroupScript.curVariationMode == MasterAudioGroup.VariationMode.LoopedChain;
			rtChainPitch = pitch;
			rtChainSource = source;
			rtChainPosition = position;
			rtChainFollow = follow;
			rtRandomVolume = 0f;
			if (useRandomVolume)
			{
				rtRandomVolume = UnityEngine.Random.Range(randomVolumeMin, randomVolumeMax);
			}
			float finalPitch = pitch ?? original_pitch;
			if (useRandomPitch)
			{
				float random = UnityEngine.Random.Range(randomPitchMin, randomPitchMax);
				finalPitch = (randomPitchMode == RandomPitchMode.AddToClipPitch) ? (finalPitch + random) : random;
			}
			audio.pitch = finalPitch;
			float blend = MasterAudio.Instance.RtSpatialBlendFor(_parentGroupScript);
			if (blend >= 0f)
			{
				audio.spatialBlend = blend;
			}
			_objectToTriggerFrom = source;
			_objectToFollow = null;
			rtFollowing = false;
			if (source != null)
			{
				base.transform.position = source.position;
				if (follow)
				{
					_objectToFollow = source;
					rtFollowing = true;
				}
			}
			else if (position.HasValue)
			{
				base.transform.position = position.Value;
			}
			else
			{
				// Played "in the head": keep the voice on the listener.
				_objectToFollow = MasterAudio.RtListenerTransform;
				rtFollowing = _objectToFollow != null;
				if (rtFollowing)
				{
					base.transform.position = _objectToFollow.position;
				}
			}
			rtLoopsLeft = 0;
			if (useCustomLooping)
			{
				rtLoopsLeft = UnityEngine.Random.Range(minCustomLoops, maxCustomLoops + 1);
				audio.loop = true;
			}
			audio.time = 0f;
			if (useRandomStartTime && audio.clip.length > 0f)
			{
				audio.time = audio.clip.length * UnityEngine.Random.Range(randomStartMinPercent, randomStartMaxPercent) * 0.01f;
			}
			rtLastTime = audio.time;
			rtFade = 1f;
			if (useFades && fadeInTime > 0f)
			{
				rtFade = 0f;
				RtBeginFade(1f, fadeInTime, false, null);
			}
			RtApplyVolume();
			RtApplyMute();
			float wait = delay;
			if (useIntroSilence)
			{
				wait += UnityEngine.Random.Range(introSilenceMin, introSilenceMax);
			}
			if (wait > 0f)
			{
				audio.PlayDelayed(wait);
			}
			else
			{
				audio.Play();
			}
			VariationUpdater.enabled = true;
		}

		internal void RtApplyVolume()
		{
			if (_audioSource != null && _parentGroupScript != null)
			{
				float clipVolume = original_volume;
				if (useRandomVolume)
				{
					clipVolume = (randomVolumeMode == RandomVolumeMode.AddToClipVolume) ? (clipVolume + rtRandomVolume) : rtRandomVolume;
				}
				_audioSource.volume = Mathf.Clamp01(clipVolume * rtPercent * rtFade * _parentGroupScript.groupMasterVolume * MasterAudio.RtMasterVolume);
			}
		}

		internal void RtApplyMute()
		{
			if (_audioSource != null && _parentGroupScript != null)
			{
				_audioSource.mute = _parentGroupScript.isMuted || MasterAudio.RtMixerMuted;
			}
		}

		private void RtBeginFade(float target, float time, bool stops, Action done)
		{
			if (!rtPlaying)
			{
				done?.Invoke();
				return;
			}
			if (time <= 0f)
			{
				rtFade = target;
				RtApplyVolume();
				if (stops)
				{
					Stop();
				}
				done?.Invoke();
				return;
			}
			rtFadeFrom = rtFade;
			rtFadeTo = target;
			rtFadeTime = time;
			rtFadeElapsed = 0f;
			rtFadeStops = stops;
			rtFadeDone = done;
		}

		// Runs every frame while the voice is playing; the updater disables with the voice object.
		internal void RtTick()
		{
			if (!rtPlaying)
			{
				return;
			}
			float delta = MasterAudio.IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
			AudioSource audio = _audioSource;
			if (rtFollowing)
			{
				if (_objectToFollow != null)
				{
					base.transform.position = _objectToFollow.position;
				}
				else
				{
					rtFollowing = false;
					switch (_parentGroupScript.targetDespawnedBehavior)
					{
					case MasterAudioGroup.TargetDespawnedBehavior.Stop:
						Stop();
						return;
					case MasterAudioGroup.TargetDespawnedBehavior.FadeOut:
						FadeOutNowAndStop(_parentGroupScript.despawnFadeTime);
						break;
					}
				}
			}
			if (rtFadeTime > 0f)
			{
				rtFadeElapsed += delta;
				rtFade = Mathf.Lerp(rtFadeFrom, rtFadeTo, rtFadeElapsed / rtFadeTime);
				RtApplyVolume();
				if (rtFadeElapsed >= rtFadeTime)
				{
					rtFadeTime = 0f;
					Action done = rtFadeDone;
					rtFadeDone = null;
					if (rtFadeStops)
					{
						Stop();
					}
					done?.Invoke();
					if (!rtPlaying)
					{
						return;
					}
				}
			}
			AudioClip clip = audio.clip;
			if (useFades && fadeOutTime > 0f && !rtEndFadeStarted && clip != null && !audio.loop && clip.length - audio.time <= fadeOutTime)
			{
				rtEndFadeStarted = true;
				RtBeginFade(0f, fadeOutTime, true, null);
			}
			if (useCustomLooping && audio.time < rtLastTime)
			{
				rtLoopsLeft--;
				SoundLoopedEventHandler looped = m_SoundLooped;
				if (looped != null)
				{
					looped(rtLoopsLeft);
				}
				if (rtLoopsLeft <= 0)
				{
					audio.loop = false;
				}
			}
			rtLastTime = audio.time;
			if (!audio.isPlaying && !_isPaused && !AudioListener.pause)
			{
				RtFinished();
			}
		}

		private void RtFinished()
		{
			bool chain = rtLoopedChain && !_isStopRequested;
			MasterAudioGroup group = _parentGroupScript;
			float percent = rtPercent;
			float? pitch = rtChainPitch;
			Transform source = rtChainSource;
			Vector3? position = rtChainPosition;
			bool follow = rtChainFollow;
			Stop();
			if (!chain || group == null)
			{
				return;
			}
			group.ChainLoopCount++;
			if (group.chainLoopMode == MasterAudioGroup.ChainedLoopLoopMode.NumberOfLoops && group.ChainLoopCount >= group.chainLoopNumLoops)
			{
				group.ChainLoopCount = 0;
				return;
			}
			float delay = UnityEngine.Random.Range(group.chainLoopDelayMin, group.chainLoopDelayMax);
			MasterAudio.RtPlay(group.name, percent, pitch, delay, null, source, position, follow);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using Bibloteka;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LessonBreakManager : CustomSingleton<LessonBreakManager>
{
	private Dictionary<Difficulty, int> lessonTimes;

	private int _LessonTime;

	private int _BreakTime;

	[SerializeField]
	private bool _StartWithLesson = true;

	private bool _lessonInProgress;

	private float _timeLeftInPhase;

	public int LessonTime => _LessonTime;

	public int BreakTime => _BreakTime;

	public bool startWithLesson => _StartWithLesson;

	public bool LessonInProgress => _lessonInProgress;

	public float FullCycleLenght => _BreakTime + _LessonTime;

	public float CurrentPhaseToFullCycleRatio => (_BreakTime + _LessonTime) > 0 ? (float)(_StartWithLesson ? _LessonTime : _BreakTime) / (_BreakTime + _LessonTime) : 0f;

	public float CycleProgress
	{
		get
		{
			float elapsedBase = _lessonInProgress == _StartWithLesson ? (_lessonInProgress ? _LessonTime : _BreakTime) : FullCycleLenght;
			return FullCycleLenght > 0f ? (elapsedBase - _timeLeftInPhase) / FullCycleLenght : 0f;
		}
	}

	private float getCurrentPhaseLength()
	{
		return _lessonInProgress ? _LessonTime : _BreakTime;
	}

	private void Awake()
	{
		lessonTimes = new Dictionary<Difficulty, int>
		{
			{ (Difficulty)0, 55 },
			{ (Difficulty)1, 50 },
			{ (Difficulty)2, 45 },
			{ (Difficulty)3, 40 },
			{ (Difficulty)4, 30 }
		};
		_LessonTime = lessonTimes[UserGameSettings.currentDifficulty];
		_BreakTime = 60 - _LessonTime;
	}

	private IEnumerator Start()
	{
		if (SceneManager.GetActiveScene().name != "School")
		{
			yield break;
		}
		yield return Utils.GetDelay(0.5f);
		_lessonInProgress = _StartWithLesson;
		_timeLeftInPhase = getCurrentPhaseLength();
		MessageKit.post(_lessonInProgress ? 43 : 44);
		StartCoroutine(LessonBreakCycle());
	}

	private IEnumerator LessonBreakCycle()
	{
		while (true)
		{
			yield return null;
			_timeLeftInPhase -= Time.deltaTime;
			if (_timeLeftInPhase <= 0f)
			{
				bool wasLesson = _lessonInProgress;
				_lessonInProgress = !wasLesson;
				_timeLeftInPhase = getCurrentPhaseLength();
				MessageKit.post(wasLesson ? 44 : 43);
				MasterAudio.PlaySound3DAtTransform("Bell", CustomSingleton<PlayerManager>.Instance.Player.transform, 1f, null, 0f, "short");
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}

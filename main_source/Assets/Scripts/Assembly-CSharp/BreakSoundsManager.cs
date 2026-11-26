using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using UnityEngine;

public class BreakSoundsManager : MonoBehaviour
{
	private Transform playerT;

	private void Start()
	{
		MessageKit.addObserver(43, LessonStarted);
		MessageKit.addObserver(44, BreakStarted);
		playerT = CustomSingleton<PlayerManager>.Instance.Player.transform;
	}

	private void LessonStarted()
	{
		MasterAudio.StopSoundGroupOfTransform(playerT, "break");
	}

	private void BreakStarted()
	{
		MasterAudio.PlaySound3DAtTransform("break", playerT);
	}

	private void OnDestroy()
	{
		MessageKit.removeObserver(43, LessonStarted);
		MessageKit.removeObserver(44, BreakStarted);
	}
}

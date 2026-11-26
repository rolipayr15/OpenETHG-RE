using Bibloteka;
using DarkTonic.MasterAudio;
using UnityEngine;

public class SoundTrap : MonoBehaviour
{
	[SerializeField]
	private int chance;

	[SerializeField]
	private float playingInterval = 180f;

	[SerializeField]
	private string soundGroup;

	public static float lastTimePlayed;

	private void OnTriggerEnter()
	{
		if (lastTimePlayed + playingInterval < Time.time && Utils.Probability(chance))
		{
			MasterAudio.PlaySound3DAtTransform(soundGroup, transform);
			lastTimePlayed = Time.time;
		}
	}
}

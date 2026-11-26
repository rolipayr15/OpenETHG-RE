using System.Collections;
using Bibloteka;
using DarkTonic.MasterAudio;
using UnityEngine;

public class PlaySoundWhenPlayerInTrigger : MonoBehaviour
{
	[SerializeField]
	private string variationName;

	[SerializeField]
	private int intervalMin;

	[SerializeField]
	private int intervalMax;

	private Coroutine playSoundCoroutine;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			StartCoroutine(PlaySound());
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			StopAllCoroutines();
		}
	}

	private IEnumerator PlaySound()
	{
		while (true)
		{
			yield return Utils.GetDelay(Random.Range(intervalMin, intervalMax));
			MasterAudio.PlaySound3DAtTransform(variationName, transform);
		}
	}
}

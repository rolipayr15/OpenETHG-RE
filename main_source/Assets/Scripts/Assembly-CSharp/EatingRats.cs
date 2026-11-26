using Bibloteka;
using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EatingRats : MonoBehaviour
{

	[SerializeField]
	private GameObject[] rats;

	private void OnTriggerEnter(Collider coll)
	{
		if (coll.CompareTag("Player"))
		{
			StartCoroutine("StartPatrol");
		}
	}

	private IEnumerator StartPatrol()
	{
		MasterAudio.PlaySound3DAtTransform("fx", transform, 1f, null, 0f, "eatingRats");
		yield return Utils.GetDelay(1f);
		foreach (GameObject rat in rats)
		{
			rat.GetComponent<Patrol>().enabled = true;
		}
		gameObject.SetActive(false);
	}
}

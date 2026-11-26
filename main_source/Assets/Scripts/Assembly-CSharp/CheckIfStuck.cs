using Bibloteka;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class CheckIfStuck : MonoBehaviour
{

	private float closeDistance = 0.5f;

	private Vector3 lastPosition;

	private NavMeshAgent agent;

	private void StartCoroutine()
	{
		StartCoroutine(Check());
	}

	private IEnumerator Check()
	{
		while (true)
		{
			yield return Utils.GetDelay(2f);
			lastPosition = gameObject.transform.position;
		}
	}
}

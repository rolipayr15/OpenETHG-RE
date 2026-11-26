using Bibloteka;
using DarkTonic.MasterAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Teleport : MonoBehaviour
{

	private GameObject[] exitGates;

	private GameObject player;

	private void Start()
	{
		exitGates = GameObject.FindGameObjectsWithTag("MansionPortal");
		player = CustomSingleton<PlayerManager>.Instance.Player;
		MasterAudio.PlaySound3DAtTransform("fx", gameObject.transform, 1f, null, 0f, "teleportLoop");
	}

	private void TeleportPlayer()
	{
		MasterAudio.PlaySound3DAtTransform("fx", gameObject.transform, 1f, null, 0f, "teleportJump");
		player.transform.position = exitGates[UnityEngine.Random.Range(0, exitGates.Length)].transform.position;
	}

	private IEnumerator TempDisableEnemyRaycast()
	{
		if ((int)UserGameSettings.currentEnemy == 4)
		{
			yield break;
		}
		Characters.Agents.EnemyAgent enemy = CustomSingleton<EnemyManager>.Instance.CurrentEnemy;
		if (enemy == null || enemy.EnemyVision == null)
		{
			yield break;
		}
		enemy.EnemyVision.SetRaycastDisabled(true);
		yield return Utils.GetDelay(2.5f);
		enemy = CustomSingleton<EnemyManager>.Instance.CurrentEnemy;
		enemy.EnemyVision.SetRaycastDisabled(false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			StartCoroutine(TempDisableEnemyRaycast());
			TeleportPlayer();
		}
		if (UserGameSettings.IsMultiplayer() && UserGameSettings.isMultiplayerHost && other.CompareTag("Avatar"))
		{
			StartCoroutine(TempDisableEnemyRaycast());
		}
	}
}

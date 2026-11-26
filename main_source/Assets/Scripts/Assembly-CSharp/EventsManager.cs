using Prime31.MessageKit;
using System.Collections.Generic;
using UnityEngine;

public class EventsManager : MonoBehaviour
{
	private readonly Dictionary<string, bool> lastChasingByEnemy = new Dictionary<string, bool>();

	private void Start()
	{
		MessageKit<bool, string>.addObserver(5, GhostIsChasing);
	}

	private void GhostIsChasing(bool isChasing, string enemyName)
	{
		if (lastChasingByEnemy.TryGetValue(enemyName, out bool last) && last == isChasing)
		{
			return;
		}
		lastChasingByEnemy[enemyName] = isChasing;
		if (FirebaseState.IsInitialized)
		{
			Firebase.Crashlytics.Crashlytics.Log("Ghost is chasing " + (isChasing ? "started" : "stopped") + " enemy=" + (string.IsNullOrEmpty(enemyName) ? "unknown" : enemyName));
		}
	}

	private void OnDestroy()
	{
		MessageKit<bool, string>.removeObserver(5, GhostIsChasing);
	}
}

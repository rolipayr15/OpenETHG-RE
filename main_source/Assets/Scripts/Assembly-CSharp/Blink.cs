using System.Collections;
using System.Collections.Generic;
using Bibloteka;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;

public class Blink : CustomSingleton<Blink>
{
	[SerializeField]
	private Text run;

	private bool isChasing;

	private int monsterCounter;

	private Dictionary<string, bool> chasedByEnemy = new Dictionary<string, bool>();

	private Coroutine stopChasingCoroutine;

	private void Start()
	{
		MessageKit<bool, string>.addObserver(5, ToggleBlink);
		MessageKit.addObserver(17, StopBlinking);
		MessageKit.addObserver(30, StopBlinking);
		MessageKit.addObserver(27, StopBlinking);
		MessageKit.addObserver(53, StopBlinking);
		if (UserGameSettings.IsMultiplayer())
		{
			MessageKit.addObserver(105, StopBlinking);
		}
		SetChasingState(false);
	}

	private void ToggleBlink(bool _isChasing, string enemyName)
	{
		if ((int)UserGameSettings.currentEnemy != 5)
		{
			SetChasing(_isChasing);
			return;
		}
		chasedByEnemy.TryGetValue(enemyName, out bool known);
		if (_isChasing)
		{
			if (!known)
			{
				monsterCounter++;
				chasedByEnemy[enemyName] = true;
			}
		}
		else if (known)
		{
			monsterCounter--;
			chasedByEnemy[enemyName] = false;
		}
		SetChasing(monsterCounter > 0);
	}

	private void SetChasing(bool shouldChase)
	{
		if (shouldChase)
		{
			if (stopChasingCoroutine != null)
			{
				StopCoroutine(stopChasingCoroutine);
				stopChasingCoroutine = null;
			}
			SetChasingState(true);
		}
		else if (isChasing && stopChasingCoroutine == null)
		{
			stopChasingCoroutine = StartCoroutine(StopChasingAfterDelay());
		}
	}

	private void SetChasingState(bool value)
	{
		isChasing = value;
		run.enabled = value;
	}

	private IEnumerator StopChasingAfterDelay()
	{
		yield return Utils.GetDelay(3f);
		SetChasingState(false);
		stopChasingCoroutine = null;
	}

	private void StopBlinking()
	{
		if (stopChasingCoroutine != null)
		{
			StopCoroutine(stopChasingCoroutine);
			stopChasingCoroutine = null;
		}
		SetChasingState(false);
	}

	protected override void OnDestroy()
	{
		MessageKit<bool, string>.removeObserver(5, ToggleBlink);
		MessageKit.removeObserver(17, StopBlinking);
		MessageKit.removeObserver(27, StopBlinking);
		if (UserGameSettings.IsMultiplayer())
		{
			MessageKit.removeObserver(105, StopBlinking);
		}
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bibloteka;
using Eyes.DailyChallenges;
using Prime31.MessageKit;
using UnityEngine;

public class PortraitsMinigame : CustomSingleton<PortraitsMinigame>
{
	private List<int> portraitsChosen;

	private List<int> portraitsOrder;

	[SerializeField]
	private GameObject[] sketches;

	[SerializeField]
	private Transform[] slots;

	[SerializeField]
	private GameObject wardrobe;

	private Vector3 wardrobeStartPosition;

	[SerializeField]
	private Transform wardrobeEnd;

	[SerializeField]
	private JournalData secretRoom;

	private void Start()
	{
		if ((int)UserGameSettings.currentLevel != 0)
		{
			return;
		}
		if (!UserGameSettings.IsMultiplayer() || UserGameSettings.isMultiplayerHost)
		{
			portraitsOrder = GetPortraitsOrder();
			SetPortraitsOrder(portraitsOrder);
		}
		if (UserGameSettings.IsMultiplayer())
		{
			MessageKit.addObserver(52, MinigameSolved);
		}
		MessageKit<int>.addObserver(46, PortraitChosen);
		portraitsChosen = new List<int>();
		wardrobeStartPosition = wardrobe.transform.position;
	}

	public List<int> GetPortraitsOrder()
	{
		List<int> order = new List<int> { 0, 1, 2, 3 };
		Utils.Shuffle(order);
		return order;
	}

	public void SetPortraitsOrder(List<int> order)
	{
		portraitsOrder = order;
		for (int i = 0; i < 4; i++)
		{
			sketches[portraitsOrder[i]].transform.position = slots[i].position;
		}
	}

	public void MinigameSolved()
	{
		MessageKit.removeObserver(52, MinigameSolved);
		StartCoroutine(MoveWardrobe());
		MessageKit.post(52);
		if (!secretRoom.IsUnlocked())
		{
			secretRoom.Unlock();
		}
		DailyChallengeRuntime.PuzzleSolved(this);
	}

	private void PortraitChosen(int index)
	{
		portraitsChosen.Add(index);
		if (portraitsChosen.Count == 4)
		{
			if (portraitsChosen.SequenceEqual(portraitsOrder))
			{
				MinigameSolved();
				return;
			}
			MessageKit.post(47);
			portraitsChosen.Clear();
		}
	}

	private IEnumerator MoveWardrobe()
	{
		float t = Time.time;
		Vector3 from = wardrobeStartPosition;
		Vector3 to = wardrobeEnd.position;
		while (Time.time < t + 0.5f)
		{
			wardrobe.transform.position = Vector3.Lerp(from, to, (Time.time - t) * 2f);
			yield return null;
		}
	}

	protected override void OnDestroy()
	{
		MessageKit<int>.removeObserver(46, PortraitChosen);
		MessageKit.removeObserver(52, MinigameSolved);
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}

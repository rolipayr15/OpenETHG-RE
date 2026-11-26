using Prime31.MessageKit;
using System.Collections.Generic;
using UnityEngine;

public class DoubleDoor : MonoBehaviour
{
	[Header("Door leaves (children Door scripts)")]
	[SerializeField]
	private Door leftDoor;

	[SerializeField]
	private Door rightDoor;

	[Header("Which colliders count as 'entities'")]
	[SerializeField]
	private string[] acceptedTags = new string[] { "Player", "Avatar" };

	[Header("Debug")]
	[SerializeField]
	private bool logDebug;

	private int entitiesInside;

	private bool isBreakStarted;

	private BoxCollider passageTrigger;

	private readonly Dictionary<int, float> playerPassageEntryOffsets = new Dictionary<int, float>();

	private void Awake()
	{
		passageTrigger = GetComponent<BoxCollider>();
	}

	private void Start()
	{
		if (!leftDoor || !rightDoor)
		{
			Debug.LogError("[DoorDouble] Missing left/right Door references on " + name, this);
			Door[] doors = GetComponentsInChildren<Door>(true);
			if (doors.Length > 1)
			{
				if (!leftDoor)
				{
					leftDoor = doors[0];
				}
				if (!rightDoor)
				{
					rightDoor = doors[1];
				}
			}
		}
		MessageKit.addObserver(43, LessonStarted);
		MessageKit.addObserver(44, BreakStarted);
	}

	private void OnDestroy()
	{
		MessageKit.removeObserver(43, LessonStarted);
		MessageKit.removeObserver(44, BreakStarted);
	}

	private void OnDisable()
	{
		playerPassageEntryOffsets.Clear();
	}

	private void LessonStarted()
	{
		isBreakStarted = false;
		if (entitiesInside < 1)
		{
			CloseBoth(CustomSingleton<PlayerManager>.Instance.Heart.transform);
		}
		else
		{
			OpenBoth(CustomSingleton<PlayerManager>.Instance.Heart.transform);
		}
	}

	private void BreakStarted()
	{
		isBreakStarted = true;
		OpenBoth(CustomSingleton<PlayerManager>.Instance.Heart.transform);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!IsAccepted(other))
		{
			return;
		}
		if (other.CompareTag("Player") && passageTrigger)
		{
			playerPassageEntryOffsets[other.GetInstanceID()] = DoorUseTracker.GetLocalPassageOffset(passageTrigger, other.transform.position, true);
		}
		entitiesInside++;
		if (logDebug)
		{
			Debug.Log(string.Format("[{0}] Enter {1}, inside={2}", "DoorDouble", other.name, entitiesInside), this);
		}
		if (!isBreakStarted)
		{
			OpenBoth(other.transform);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!IsAccepted(other))
		{
			return;
		}
		if (other.CompareTag("Player") && passageTrigger)
		{
			int id = other.GetInstanceID();
			if (playerPassageEntryOffsets.TryGetValue(id, out float entry) && DoorUseTracker.DidCrossDoor(entry, DoorUseTracker.GetLocalPassageOffset(passageTrigger, other.transform.position, true)))
			{
				DoorUseTracker.RecordPassage(GetInstanceID(), this);
			}
			playerPassageEntryOffsets.Remove(id);
		}
		entitiesInside = Mathf.Max(0, entitiesInside - 1);
		if (logDebug)
		{
			Debug.Log(string.Format("[{0}] Exit {1}, inside={2}", "DoorDouble", other.name, entitiesInside), this);
		}
		if (!isBreakStarted && entitiesInside == 0)
		{
			CloseBoth(other.transform);
		}
	}

	private bool IsAccepted(Collider other)
	{
		foreach (string tag in acceptedTags)
		{
			if (other.CompareTag(tag))
			{
				return true;
			}
		}
		return false;
	}

	private void OpenBoth(Transform who)
	{
		if (leftDoor)
		{
			leftDoor.OpenDoor(who);
		}
		if (rightDoor)
		{
			rightDoor.OpenDoor(who);
		}
	}

	private void CloseBoth(Transform who)
	{
		if (leftDoor)
		{
			leftDoor.CloseDoor(who);
		}
		if (rightDoor)
		{
			rightDoor.CloseDoor(who);
		}
	}
}

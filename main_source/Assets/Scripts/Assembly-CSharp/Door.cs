using Bibloteka;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Door : MonoBehaviour
{



	[SerializeField]
	private Collider raycastBlocker;

	[SerializeField]
	private Collider monsterDoorDetector;

	[SerializeField]
	private NavMeshObstacle navigationBlocker;

	[SerializeField]
	private Transform doorMesh;

	[SerializeField]
	private int angle = 90;

	protected int rotation;

	protected const float animationTime = 0.5f;

	public bool isOpen;

	private bool isBreakStarted;

	private Quaternion startLocalRot;

	private Quaternion targetLocalRot;

	private BoxCollider triggerBox;

	private Coroutine syncCoroutine;

	private Coroutine blockDoorCoroutine;

	private Coroutine enableDoorColliderCoroutine;

	private int entitiesInside;

	private int latchedOpenDir;

	public bool isAnimating;

	private readonly Dictionary<int, float> playerPassageEntryOffsets = new Dictionary<int, float>();

	private Coroutine rotateCoroutine;

	public float animDeadline;

	private readonly HashSet<Collider> inside = new HashSet<Collider>();

	protected Transform DoorMesh => doorMesh;

	protected int Angle => angle;

	protected virtual void Awake()
	{
		if (doorMesh)
		{
			startLocalRot = doorMesh.localRotation;
			targetLocalRot = startLocalRot;
		}
		triggerBox = GetComponent<BoxCollider>();
	}

	public virtual void Start()
	{
		MessageKit.addObserver(43, LessonStarted);
		MessageKit.addObserver(44, BreakStarted);
		if (syncCoroutine == null)
		{
			syncCoroutine = StartCoroutine(SyncDoorStateLoop());
		}
	}

	private void OnDestroy()
	{
		MessageKit.removeObserver(43, LessonStarted);
		MessageKit.removeObserver(44, BreakStarted);
		if (blockDoorCoroutine != null)
		{
			StopCoroutine(blockDoorCoroutine);
		}
		if (syncCoroutine != null)
		{
			StopCoroutine(syncCoroutine);
		}
		if (enableDoorColliderCoroutine != null)
		{
			StopCoroutine(enableDoorColliderCoroutine);
		}
		if (rotateCoroutine != null)
		{
			StopCoroutine(rotateCoroutine);
		}
	}

	private void OnDisable()
	{
		playerPassageEntryOffsets.Clear();
		if (rotateCoroutine != null)
		{
			StopCoroutine(rotateCoroutine);
		}
		rotateCoroutine = null;
		isAnimating = false;
	}

	private void LessonStarted()
	{
		isBreakStarted = false;
		GameObject heart = CustomSingleton<PlayerManager>.Instance.Heart;
		Transform heartT = heart != null ? heart.transform : null;
		if (heartT == null)
		{
			return;
		}
		if (entitiesInside != 0)
		{
			OpenDoor(heartT);
		}
		else
		{
			CloseDoor(heartT);
		}
	}

	private void BreakStarted()
	{
		isBreakStarted = true;
		GameObject heart = CustomSingleton<PlayerManager>.Instance.Heart;
		Transform heartT = heart != null ? heart.transform : null;
		if (heartT != null)
		{
			OpenDoor(heartT);
		}
	}

	private void SetCollidersEnabled(bool enabled)
	{
		if (raycastBlocker)
		{
			raycastBlocker.enabled = enabled;
		}
		if (monsterDoorDetector)
		{
			monsterDoorDetector.enabled = enabled;
		}
		if (navigationBlocker)
		{
			navigationBlocker.enabled = enabled;
		}
	}

	private int GetOpenDir(Transform trans)
	{
		float dot = Vector3.Dot(trans.position - doorMesh.position, doorMesh.forward);
		if (Mathf.Abs(dot) >= 0.01f)
		{
			return dot >= 0f ? 1 : -1;
		}
		return (uint)latchedOpenDir < 2u ? 1 : latchedOpenDir;
	}

	private bool IsInsideTrigger(BoxCollider box, Transform t)
	{
		if (!box || !t)
		{
			return false;
		}
		return (box.ClosestPoint(t.position) - t.position).sqrMagnitude < 0.0001f;
	}

	private IEnumerator VerifyAndCloseDoor(Transform _)
	{
		yield return null;
		if (!triggerBox)
		{
			yield break;
		}
		GameObject player = CustomSingleton<PlayerManager>.Instance != null ? CustomSingleton<PlayerManager>.Instance.Player : null;
		int count = player && IsInsideTrigger(triggerBox, CustomSingleton<PlayerManager>.Instance.Player.transform) ? 1 : 0;
		GameObject avatar = CustomSingleton<GuestManager>.Instance != null ? CustomSingleton<GuestManager>.Instance.Avatar : null;
		if (avatar && IsInsideTrigger(triggerBox, avatar.transform))
		{
			count++;
		}
		if (count != entitiesInside)
		{
			entitiesInside = count;
		}
		if (!isBreakStarted && count == 0)
		{
			latchedOpenDir = 0;
			GameObject heart = CustomSingleton<PlayerManager>.Instance.Heart;
			Transform heartT = heart != null ? heart.transform : null;
			if (heartT != null)
			{
				CloseDoor(heartT);
			}
		}
	}

	public virtual void OpenDoor(Transform trans, bool playSound = true)
	{
		if (!doorMesh)
		{
			return;
		}
		int direction = latchedOpenDir;
		if (direction == 0)
		{
			direction = GetOpenDir(trans);
			if (latchedOpenDir == 0)
			{
				latchedOpenDir = direction;
			}
		}
		Quaternion open = startLocalRot * Quaternion.Euler(0f, angle * direction, 0f);
		if (!isOpen || Quaternion.Angle(targetLocalRot, open) >= 1f)
		{
			if (Quaternion.Angle(doorMesh.localRotation, open) >= 1f || isAnimating)
			{
				targetLocalRot = open;
				isOpen = true;
				SetCollidersEnabled(false);
				StartRotateTo(open, animationTime, null);
				if (playSound)
				{
					MasterAudio.PlaySound3DAtTransform("door", trans, 0.5f);
				}
				return;
			}
		}
		isOpen = true;
		SetCollidersEnabled(false);
	}

	public virtual void CloseDoor(Transform trans, bool playSound = true)
	{
		if (!doorMesh)
		{
			return;
		}
		if (isOpen || Quaternion.Angle(targetLocalRot, startLocalRot) >= 1f)
		{
			if (Quaternion.Angle(doorMesh.localRotation, startLocalRot) >= 1f || isAnimating)
			{
				isOpen = false;
				targetLocalRot = startLocalRot;
				SetCollidersEnabled(true);
				if (playSound)
				{
					MasterAudio.PlaySound3DAtTransform("door", trans, 0.5f);
				}
				StartRotateTo(startLocalRot, 1.5f, delegate
				{
					if (entitiesInside == 0)
					{
						latchedOpenDir = 0;
					}
				});
				return;
			}
		}
		isOpen = false;
		SetCollidersEnabled(true);
	}

	public virtual void SnapVisualToTarget()
	{
		if (doorMesh)
		{
			doorMesh.localRotation = targetLocalRot;
		}
	}

	private void StartRotateTo(Quaternion target, float duration, Action onComplete)
	{
		if (!doorMesh)
		{
			return;
		}
		if (rotateCoroutine != null)
		{
			StopCoroutine(rotateCoroutine);
		}
		rotateCoroutine = StartCoroutine(RotateLocalCoroutine(target, duration, onComplete));
	}

	private IEnumerator RotateLocalCoroutine(Quaternion target, float duration, Action onComplete)
	{
		isAnimating = true;
		Quaternion from = doorMesh.localRotation;
		float t = 0f;
		animDeadline = Time.time + duration + 0.15f;
		while (t < duration && Time.time <= animDeadline)
		{
			t += Time.deltaTime;
			float k = duration <= 0.0001f ? 1f : Mathf.Clamp01(t / duration);
			float eased = k < 0.5f ? k * (k + k) : 1f - (2f - (k + k)) * (2f - (k + k)) * 0.5f;
			doorMesh.localRotation = Quaternion.Slerp(from, target, eased);
			yield return null;
		}
		doorMesh.localRotation = target;
		rotateCoroutine = null;
		targetLocalRot = target;
		isAnimating = false;
		onComplete?.Invoke();
	}

	private IEnumerator SyncDoorStateLoop()
	{
		WaitForSeconds wait = Utils.GetDelay(0.5f);
		while (true)
		{
			yield return wait;
			if (!triggerBox || !doorMesh)
			{
				continue;
			}
			entitiesInside = inside.Count;
			bool shouldBeOpen = isBreakStarted || entitiesInside > 0;
			bool stale = isAnimating && animDeadline < Time.time;
			if (isOpen == shouldBeOpen || (isAnimating && !stale))
			{
				if (!isAnimating)
				{
					SetCollidersEnabled(!isOpen);
				}
				continue;
			}
			if (stale)
			{
				isAnimating = false;
				SnapVisualToTarget();
			}
			GameObject heart = CustomSingleton<PlayerManager>.Instance != null ? CustomSingleton<PlayerManager>.Instance.Heart : null;
			Transform heartT = heart != null ? heart.transform : null;
			if (heartT != null)
			{
				if (shouldBeOpen)
				{
					OpenDoor(heartT, false);
				}
				else
				{
					CloseDoor(heartT, false);
				}
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!IsEntity(other) || !inside.Add(other))
		{
			return;
		}
		if (other.CompareTag("Player") && triggerBox)
		{
			playerPassageEntryOffsets[other.GetInstanceID()] = DoorUseTracker.GetLocalPassageOffset(triggerBox, other.transform.position, false);
		}
		entitiesInside = inside.Count;
		if (entitiesInside == 1)
		{
			latchedOpenDir = GetOpenDir(other.transform);
		}
		if (!isBreakStarted)
		{
			OpenDoor(other.transform);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!IsEntity(other) || !inside.Remove(other))
		{
			return;
		}
		if (other.CompareTag("Player") && triggerBox)
		{
			int id = other.GetInstanceID();
			if (playerPassageEntryOffsets.TryGetValue(id, out float entry) && DoorUseTracker.GetLocalPassageOffset(triggerBox, other.transform.position, false) * entry < 0f)
			{
				DoorUseTracker.RecordPassage(GetInstanceID(), this);
			}
			playerPassageEntryOffsets.Remove(id);
		}
		entitiesInside = inside.Count;
		if (!isBreakStarted && entitiesInside == 0)
		{
			StartCoroutine(VerifyAndCloseDoor(other.transform));
		}
	}

	private bool IsEntity(Collider c)
	{
		return c.CompareTag("Player") || c.CompareTag("Avatar");
	}
}

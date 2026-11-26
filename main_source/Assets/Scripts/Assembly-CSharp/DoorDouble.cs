using System;
using System.Collections;
using UnityEngine;

public class DoorDouble : Door
{
	[SerializeField]
	private Transform doorMesh2;

	[SerializeField]
	private bool reverseAngle = true;

	private Quaternion startLocalRot2;

	private Quaternion targetLocalRot2;

	private Coroutine rotateCoroutine2;

	private bool isAnimating2;

	private float animDeadline2;

	protected override void Awake()
	{
		base.Awake();
		if (doorMesh2)
		{
			startLocalRot2 = doorMesh2.localRotation;
			targetLocalRot2 = startLocalRot2;
		}
	}

	public override void Start()
	{
		base.Start();
	}

	private void OnDestroy()
	{
		if (rotateCoroutine2 != null)
		{
			StopCoroutine(rotateCoroutine2);
		}
	}

	private void OnDisable()
	{
		if (rotateCoroutine2 != null)
		{
			StopCoroutine(rotateCoroutine2);
		}
		rotateCoroutine2 = null;
		isAnimating2 = false;
	}

	public override void OpenDoor(Transform trans, bool playSound = true)
	{
		base.OpenDoor(trans, playSound);
		if (!doorMesh2 || DoorMesh == null || trans == null)
		{
			return;
		}
		int side = DoorMesh.InverseTransformPoint(trans.position).x >= 0f ? 1 : -1;
		int direction = reverseAngle ? -side : side;
		Quaternion target = startLocalRot2 * Quaternion.Euler(0f, Angle * direction, 0f);
		if (AlreadyAt(target))
		{
			targetLocalRot2 = target;
			return;
		}
		targetLocalRot2 = target;
		StartRotateTo2(target, 0.5f, null);
	}

	public override void CloseDoor(Transform trans, bool playSound = true)
	{
		base.CloseDoor(trans, playSound);
		if (!doorMesh2)
		{
			return;
		}
		if (AlreadyAt(startLocalRot2))
		{
			targetLocalRot2 = startLocalRot2;
			return;
		}
		targetLocalRot2 = startLocalRot2;
		StartRotateTo2(startLocalRot2, 1.5f, null);
	}

	private bool AlreadyAt(Quaternion target)
	{
		return Quaternion.Angle(doorMesh2.localRotation, target) < 1f && !isAnimating2;
	}

	public override void SnapVisualToTarget()
	{
		base.SnapVisualToTarget();
		if (doorMesh2)
		{
			doorMesh2.localRotation = targetLocalRot2;
		}
		if (rotateCoroutine2 != null)
		{
			StopCoroutine(rotateCoroutine2);
			rotateCoroutine2 = null;
		}
		isAnimating2 = false;
	}

	private void StartRotateTo2(Quaternion target, float duration, Action onComplete)
	{
		if (!doorMesh2)
		{
			return;
		}
		if (rotateCoroutine2 != null)
		{
			StopCoroutine(rotateCoroutine2);
		}
		rotateCoroutine2 = StartCoroutine(RotateLocalCoroutine2(target, duration, onComplete));
	}

	private IEnumerator RotateLocalCoroutine2(Quaternion target, float duration, Action onComplete)
	{
		isAnimating2 = true;
		Quaternion from = doorMesh2.localRotation;
		animDeadline2 = Time.time + duration + 0.15f;
		float t = 0f;
		while (t < duration && Time.time <= animDeadline2)
		{
			t += Time.deltaTime;
			float p = Mathf.Clamp01(t / duration);
			float eased = p < 0.5f ? 2f * p * p : 1f - Mathf.Pow(-2f * p + 2f, 2f) / 2f;
			doorMesh2.localRotation = Quaternion.Slerp(from, target, eased);
			yield return null;
		}
		doorMesh2.localRotation = target;
		rotateCoroutine2 = null;
		targetLocalRot2 = target;
		isAnimating2 = false;
		onComplete?.Invoke();
	}
}

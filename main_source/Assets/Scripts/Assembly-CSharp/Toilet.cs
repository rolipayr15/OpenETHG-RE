using System.Collections;
using DarkTonic.MasterAudio;
using UnityEngine;

public class Toilet : Interactable
{
	[SerializeField]
	private Transform endTransform;

	[SerializeField]
	private Collider furnitureCollider;

	[SerializeField]
	private float totalTime = 0.5f;

	[SerializeField]
	private Collider enemyDetectorCollider;

	private string soundGroupName = "cupboard";

	private Quaternion startRotation;

	private Rigidbody rigidb;

	private bool isOpen;

	private Coroutine animCo;

	public override void Start()
	{
		base.Start();
		rigidb = GetComponent<Rigidbody>();
		if (rigidb != null)
		{
			startRotation = rigidb.rotation;
			rigidb.isKinematic = true;
			rigidb.interpolation = RigidbodyInterpolation.Interpolate;
			furnitureCollider = GetComponent<Collider>();
		}
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		base.Clicked(isClickedByOtherPlayer);
		if (animCo != null)
		{
			StopCoroutine(animCo);
		}
		animCo = StartCoroutine(OpenOrClose());
		isOpen = !isOpen;
		InteractionEnd();
		MasterAudio.PlaySound3DAtTransform(soundGroupName, transform);
	}

	private IEnumerator OpenOrClose()
	{
		furnitureCollider.enabled = false;
		Quaternion from = isOpen ? endTransform.rotation : startRotation;
		Quaternion to = isOpen ? startRotation : endTransform.rotation;
		float elapsed = 0f;
		while (elapsed < totalTime)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / totalTime);
			rigidb.MoveRotation(Quaternion.Slerp(from, to, t * t * (3f - 2f * t)));
			yield return null;
		}
		rigidb.MoveRotation(to);
		furnitureCollider.enabled = true;
		furnitureCollider.isTrigger = isOpen;
		if (enemyDetectorCollider != null)
		{
			enemyDetectorCollider.enabled = !isOpen;
		}
		animCo = null;
	}
}

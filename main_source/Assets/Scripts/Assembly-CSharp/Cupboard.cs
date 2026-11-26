using System.Collections;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Cupboard : Interactable
{
	private static readonly WaitForFixedUpdate FixedUpdateYield = new WaitForFixedUpdate();

	[SerializeField]
	private Transform endTransform;

	[SerializeField]
	private Collider furnitureCollider;

	[SerializeField]
	protected float totalTime = 0.5f;

	[SerializeField]
	private string soundGroupNameOpen = "cupboard";

	[SerializeField]
	private string soundGroupNameClose = "cupboard";

	public Material materialAfterInteraction;

	private Quaternion startRotation;

	private Rigidbody rigidb;

	protected bool isOpen;

	protected Renderer furnitureRenderer;

	[SerializeField]
	private GameObject collectible;

	private Coroutine rotateCoroutine;

	private bool isAnimating;

	private bool firstTimeOpened = true;

	public override void Start()
	{
		base.Start();
		rigidb = GetComponent<Rigidbody>();
		rigidb.isKinematic = true;
		rigidb.interpolation = RigidbodyInterpolation.Interpolate;
		startRotation = rigidb.rotation;
		furnitureCollider = GetComponent<Collider>();
		furnitureRenderer = GetComponent<Renderer>();
		int difficulty = (int)UserGameSettings.currentDifficulty;
		if (difficulty == 3 || difficulty == 4)
		{
			furnitureRenderer.material = materialAfterInteraction;
		}
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		base.Clicked(isClickedByOtherPlayer);
		if (!isAnimating)
		{
			if (rotateCoroutine != null)
			{
				StopCoroutine(rotateCoroutine);
			}
			bool wasOpen = isOpen;
			isOpen = !wasOpen;
			rotateCoroutine = StartCoroutine(OpenOrClose(!wasOpen, isClickedByOtherPlayer));
			furnitureRenderer.material = materialAfterInteraction;
			InteractionEnd();
			MasterAudio.PlaySound3DAtTransform(wasOpen ? soundGroupNameOpen : soundGroupNameClose, base.transform);
			if (firstTimeOpened && !wasOpen)
			{
				firstTimeOpened = false;
				MessageKit.post(59);
			}
		}
	}

	private IEnumerator OpenOrClose(bool openTarget, bool isClickedByOther)
	{
		isAnimating = true;
		furnitureCollider.enabled = false;
		Quaternion from = rigidb.rotation;
		Quaternion to = (openTarget ? endTransform.rotation : startRotation);
		float elapsed = 0f;
		while (elapsed < totalTime)
		{
			elapsed += Time.fixedDeltaTime;
			float t = Mathf.Clamp01(elapsed / totalTime);
			rigidb.MoveRotation(Quaternion.Slerp(from, to, t * t * (3f - (t + t))));
			yield return FixedUpdateYield;
		}
		rigidb.MoveRotation(to);
		furnitureCollider.isTrigger = openTarget;
		furnitureCollider.enabled = true;
		if (collectible != null && collectible.activeInHierarchy)
		{
			Interactable interactable = collectible.GetComponent<Interactable>();
			if (interactable != null)
			{
				interactable.Clicked(isClickedByOther);
			}
		}
		rotateCoroutine = null;
		isAnimating = false;
	}
}

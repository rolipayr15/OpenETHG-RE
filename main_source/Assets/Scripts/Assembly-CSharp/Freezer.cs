using System.Collections;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using UnityEngine;

public class Freezer : Interactable
{
	[SerializeField]
	private JournalData journalEntry;

	[SerializeField]
	private Transform endTransform;

	private Quaternion startRotation;

	private Rigidbody rigidb;

	[SerializeField]
	private Collider furnitureCollider;

	private bool isOpen;

	[SerializeField]
	protected float totalTime = 0.5f;

	private string soundGroupName = "freezer";

	[SerializeField]
	private GameObject collectible;

	[SerializeField]
	private Material materialAfterInteraction;

	private Renderer furnitureRenderer;

	public override void Start()
	{
		base.Start();
		rigidb = GetComponent<Rigidbody>();
		if (rigidb == null)
		{
			return;
		}
		startRotation = rigidb.rotation;
		furnitureCollider = GetComponent<Collider>();
		furnitureRenderer = GetComponent<Renderer>();
		int difficulty = (int)UserGameSettings.currentDifficulty;
		if ((difficulty == 3 || difficulty == 4) && furnitureRenderer != null)
		{
			furnitureRenderer.material = materialAfterInteraction;
		}
	}

	private IEnumerator OpenOrClose()
	{
		MasterAudio.PlaySound3DAtTransform(soundGroupName, transform);
		furnitureCollider.enabled = false;
		Quaternion from;
		Quaternion to;
		if (!isOpen)
		{
			from = startRotation;
			to = endTransform.rotation;
		}
		else
		{
			from = endTransform.rotation;
			to = startRotation;
		}
		float elapsed = 0f;
		while (elapsed < totalTime)
		{
			elapsed += Time.deltaTime;
			transform.rotation = Quaternion.Slerp(from, to, elapsed / totalTime);
			yield return null;
		}
		if (furnitureCollider != null)
		{
			furnitureCollider.enabled = true;
		}
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		base.Clicked(isClickedByOtherPlayer);
		StartCoroutine(OpenOrClose());
		if (furnitureRenderer == null)
		{
			return;
		}
		furnitureRenderer.material = materialAfterInteraction;
		isOpen = !isOpen;
		InteractionEnd();
		if (journalEntry != null && !journalEntry.IsUnlocked())
		{
			journalEntry.Unlock();
			MessageKit<string>.post(3, journalEntry.GetShortText());
			MasterAudio.PlaySound3DAtTransform("fx", transform, 1f, null, 0f, "paper");
		}
		if (collectible != null && collectible.activeInHierarchy)
		{
			Interactable interactable = collectible.GetComponent<Interactable>();
			if (interactable != null)
			{
				interactable.Clicked(false);
			}
		}
	}
}

using I2.Loc;
using Prime31.MessageKit;
using System.Globalization;
using UnityEngine;

public class MoneyBag : Interactable
{
	[SerializeField]
	private MeshRenderer potionEffectRenderer;

	private MeshRenderer meshRenderer;

	[SerializeField]
	private GameObject treats;

	private Collider bagCollider;

	private bool isCooldownActive;

	private FloorManager floorManager;

	[SerializeField]
	private bool shouldBeAlwaysActivated;

	[SerializeField]
	private Material materialWithoutHighlight;

	public override void Start()
	{
		base.Start();
		meshRenderer = GetComponent<MeshRenderer>();
		if ((int)UserGameSettings.currentMode == 2)
		{
			if (treats != null)
			{
				treats.SetActive(true);
			}
			meshRenderer.enabled = false;
			return;
		}
		bagCollider = GetComponent<Collider>();
		if (treats != null)
		{
			treats.SetActive(false);
		}
		meshRenderer.enabled = true;
		int difficulty = (int)UserGameSettings.currentDifficulty;
		if (difficulty == 3 || difficulty == 4)
		{
			meshRenderer.material = materialWithoutHighlight;
		}
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		string id = transform.position.sqrMagnitude.ToString(CultureInfo.InvariantCulture);
		if (isClickedByOtherPlayer)
		{
			base.Clicked(true);
			InteractionEnd();
			if (CustomSingleton<MoneyBagsManager>.Instance.CanEscape())
			{
				return;
			}
			CustomSingleton<MoneyBagsManager>.Instance.CollectBagByOtherPlayer(id);
			gameObject.SetActive(false);
			return;
		}
		if ((int)UserGameSettings.currentMode == 1)
		{
			GameObject heart = CustomSingleton<PlayerManager>.Instance.Heart;
			if (heart == null || !FloorManager.TryGetInitializedSceneInstance(out floorManager))
			{
				InteractionEnd();
				return;
			}
			if (floorManager.GetFloor(heart.transform.position) != floorManager.GetFloor(transform.position))
			{
				InteractionEnd();
				return;
			}
		}
		base.Clicked(false);
		InteractionEnd();
		if (CustomSingleton<MoneyBagsManager>.Instance.CanEscape())
		{
			MessageKit<string>.post(3, ScriptLocalization.YouHaveEnough);
			return;
		}
		if (isCooldownActive)
		{
			return;
		}
		isCooldownActive = true;
		MessageKit<Interactable>.post(58, this);
		CustomSingleton<MoneyBagsManager>.Instance.CollectBag(id);
		if ((int)UserGameSettings.currentMode == 1)
		{
			meshRenderer.enabled = false;
			bagCollider.enabled = false;
			if (potionEffectRenderer != null)
			{
				potionEffectRenderer.enabled = false;
			}
			return;
		}
		gameObject.SetActive(false);
	}

	private void Update()
	{
		if (isCooldownActive && (int)UserGameSettings.currentMode == 1)
		{
			Cooldown();
		}
	}

	private void Cooldown()
	{
		GameObject heart = CustomSingleton<PlayerManager>.Instance.Heart;
		if (heart == null || floorManager == null || !floorManager.IsInitialized)
		{
			return;
		}
		if (floorManager.GetFloor(heart.transform.position) == floorManager.GetFloor(transform.position))
		{
			return;
		}
		meshRenderer.enabled = true;
		if (potionEffectRenderer != null)
		{
			potionEffectRenderer.enabled = true;
		}
		bagCollider.enabled = true;
		isCooldownActive = false;
	}

	public bool ShouldBeAlwaysActivated()
	{
		return shouldBeAlwaysActivated;
	}
}

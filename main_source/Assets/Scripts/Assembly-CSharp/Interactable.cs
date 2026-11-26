using System.Globalization;
using Prime31.MessageKit;
using UnityEngine;

// Base of everything the player can use. Only one interaction runs at a time (until InteractionEnd).
// In multiplayer each interactable is identified by its squared distance from the origin, which is the same on both
// clients, and interactions are mirrored through MessageKit 81.
public abstract class Interactable : MonoBehaviour
{
	private static bool isAnyActive;

	protected float itemId;

	private bool isObservingMultiplayerInteractions;

	public virtual void Start()
	{
		itemId = transform.position.sqrMagnitude;
		if (UserGameSettings.IsMultiplayer())
		{
			MessageKit<string>.addObserver(81, OtherPlayerInteracts);
			isObservingMultiplayerInteractions = true;
		}
	}

	protected virtual void OnDestroy()
	{
		if (isObservingMultiplayerInteractions)
		{
			MessageKit<string>.removeObserver(81, OtherPlayerInteracts);
		}
	}

	public void TryInteract(bool isClickedByOtherPlayer)
	{
		if (isAnyActive)
		{
			return;
		}
		isAnyActive = true;
		Clicked(isClickedByOtherPlayer);
		if (!isClickedByOtherPlayer)
		{
			EyesAnalytics.Game.CountInteraction();
		}
	}

	public virtual void Clicked(bool isClickedByOtherPlayer)
	{
		if (!isClickedByOtherPlayer && UserGameSettings.IsMultiplayer())
		{
			CustomSingleton<MultiplayerSettings>.Instance.SendInteraction(itemId, MultiplayerProtocol.InteractableIdCodes.GetInteractableId(GetType().Name));
		}
	}

	public virtual void ButtonUp()
	{
	}

	public static void InteractionEnd()
	{
		isAnyActive = false;
	}

	public virtual void OtherPlayerInteracts(string _itemId)
	{
		if (_itemId == itemId.ToString(CultureInfo.InvariantCulture))
		{
			Clicked(true);
		}
	}
}

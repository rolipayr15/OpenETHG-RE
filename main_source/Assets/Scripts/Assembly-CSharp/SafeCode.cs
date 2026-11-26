using Prime31.MessageKit;
public class SafeCode : Interactable
{
	private bool isCollected;

	public override void Start()
	{
		base.Start();
	}

	public override void OtherPlayerInteracts(string itemId)
	{
		base.OtherPlayerInteracts(itemId);
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		if (!isClickedByOtherPlayer)
		{
			if (isCollected)
			{
				InteractionEnd();
				return;
			}
		}
		else if (UserGameSettings.IsMultiplayerVs() || isCollected)
		{
			return;
		}
		isCollected = true;
		base.Clicked(isClickedByOtherPlayer);
		MessageKit<bool>.post(18, isClickedByOtherPlayer);
		if (!isClickedByOtherPlayer)
		{
			MessageKit<Interactable>.post(58, this);
		}
		InteractionEnd();
		gameObject.SetActive(false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}

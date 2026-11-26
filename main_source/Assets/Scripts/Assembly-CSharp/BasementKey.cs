using Prime31.MessageKit;
public class BasementKey : Interactable
{
	public override void Clicked(bool isClickedByOtherPlayer)
	{
		base.Clicked(isClickedByOtherPlayer);
		if (UserGameSettings.IsMultiplayer() && UserGameSettings.IsMultiplayerCoop() && isClickedByOtherPlayer)
		{
			MessageKit.post(4);
		}
		if (!isClickedByOtherPlayer)
		{
			MessageKit.post(4);
			MessageKit<Interactable>.post(58, this);
		}
		MessageKit<string, bool>.post(3, I2.Loc.ScriptLocalization.KeyFound, isClickedByOtherPlayer);
		InteractionEnd();
		gameObject.SetActive(false);
	}
}

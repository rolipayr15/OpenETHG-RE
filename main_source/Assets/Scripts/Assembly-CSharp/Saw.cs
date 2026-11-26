using Prime31.MessageKit;
using DarkTonic.MasterAudio;
public class Saw : Interactable
{
	public override void Clicked(bool isClickedByOtherPlayer)
	{
		base.Clicked(isClickedByOtherPlayer);
		MasterAudio.PlaySound3DAtTransform("fx", gameObject.transform, 1f, null, 0f, "teleportJump");
		if (!isClickedByOtherPlayer || (UserGameSettings.IsMultiplayer() && UserGameSettings.IsMultiplayerCoop()))
		{
			MessageKit.post(20);
		}
		if (!isClickedByOtherPlayer)
		{
			MessageKit<Interactable>.post(58, this);
		}
		MessageKit<string, bool>.post(3, I2.Loc.ScriptLocalization.ThisSawMight, isClickedByOtherPlayer);
		InteractionEnd();
		gameObject.SetActive(false);
	}
}

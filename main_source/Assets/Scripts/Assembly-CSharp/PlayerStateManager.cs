public class PlayerStateManager : CustomSingleton<PlayerStateManager>
{
	protected vp_FPPlayerEventHandler _playerHandler;

	protected vp_FPPlayerEventHandler playerHandler => null;

	protected PlayerStateManager()
	{
	}

	public void PauseStop()
	{
	}

	public void PauseStart()
	{
	}

	protected override void OnDestroy()
	{
	}

	protected override void OnApplicationQuit()
	{
	}
}

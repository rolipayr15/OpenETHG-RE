public class vp_StatePowerup : vp_Powerup
{
	public string State = "MegaSpeed";

	public float Duration;

	protected vp_Timer.Handle m_Timer = new vp_Timer.Handle();

	protected override void Update()
	{
		UpdateMotion();
		if (m_Depleted && !m_Audio.isPlaying)
		{
			Remove();
		}
	}

	protected override bool TryGive(vp_PlayerEventHandler player)
	{
		if (m_Timer.Active)
		{
			return false;
		}
		if (string.IsNullOrEmpty(State))
		{
			return false;
		}
		player.SetState(State);
		vp_Timer.In((Duration <= 0f) ? RespawnDuration : Duration, delegate
		{
			player.SetState(State, setActive: false);
		}, m_Timer);
		return true;
	}
}

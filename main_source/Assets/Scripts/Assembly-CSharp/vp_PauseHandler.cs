using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using UnityEngine;

public class vp_PauseHandler : MonoBehaviour
{
	protected vp_FPPlayerEventHandler m_Player;

	protected bool OnValue_PauseWrapper
	{
		get
		{
			return m_Player.Pause.Get();
		}
		set
		{
			m_Player.Pause.Set(value);
			MessageKit<bool>.post(6, value);
			if (value)
			{
				MasterAudio.MuteEverything();
			}
			else
			{
				MasterAudio.UnmuteEverything();
			}
		}
	}

	private void Awake()
	{
		if (m_Player == null)
		{
			m_Player = Object.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler;
		}
	}

	protected virtual void OnEnable()
	{
		if (m_Player != null)
		{
			m_Player.Register(this);
		}
	}

	protected virtual void OnDisable()
	{
		if (m_Player != null)
		{
			m_Player.Unregister(this);
		}
	}
}

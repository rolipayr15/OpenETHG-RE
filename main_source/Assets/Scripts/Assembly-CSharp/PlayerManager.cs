using UnityEngine;

// Cached lookups of the local player's objects by tag.
public class PlayerManager : CustomSingleton<PlayerManager>
{
	private GameObject player_;

	private GameObject heart_;

	private GameObject fakePlayer_;

	private GameObject playerCamera_;

	private GameObject playerModel_;

	protected vp_FPPlayerEventHandler playerHandler_;

	public GameObject Player
	{
		get
		{
			if (player_ == null)
			{
				player_ = GameObject.FindGameObjectWithTag("Player");
			}
			return player_;
		}
	}

	public GameObject Heart
	{
		get
		{
			if (heart_ == null)
			{
				heart_ = GameObject.FindGameObjectWithTag("Heart");
			}
			return heart_;
		}
	}

	public GameObject PlayerCamera
	{
		get
		{
			if (playerCamera_ == null)
			{
				playerCamera_ = GameObject.FindGameObjectWithTag("MainCamera");
			}
			return playerCamera_;
		}
	}

	public vp_FPPlayerEventHandler playerHandler
	{
		get
		{
			if (playerHandler_ == null)
			{
				playerHandler_ = Object.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler;
			}
			return playerHandler_;
		}
	}

	public GameObject PlayerModel
	{
		get
		{
			if (playerModel_ == null)
			{
				playerModel_ = GameObject.FindGameObjectWithTag("PlayerModel");
			}
			return playerModel_;
		}
	}

	protected PlayerManager()
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}

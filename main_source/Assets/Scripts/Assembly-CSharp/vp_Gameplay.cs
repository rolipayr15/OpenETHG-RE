using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class vp_Gameplay
{
	public static string Version = "0.0.0";

	public static string PlayerName = "Player";

	public static bool IsMultiplayer = false;

	protected static bool m_IsMaster = true;

	public static bool IsVR = false;

	public static bool IsMaster
	{
		get
		{
			if (!IsMultiplayer)
			{
				return true;
			}
			return m_IsMaster;
		}
		set
		{
			if (IsMultiplayer)
			{
				m_IsMaster = value;
			}
		}
	}

	public static bool IsPaused
	{
		get
		{
			return vp_TimeUtility.Paused;
		}
		set
		{
			vp_TimeUtility.Paused = !IsMultiplayer && value;
		}
	}

	[Obsolete("Please use the 'IsMaster' property instead.")]
	public static bool isMaster
	{
		get
		{
			return IsMaster;
		}
		set
		{
			IsMaster = value;
		}
	}

	[Obsolete("Please use the 'IsMultiplayer' property instead.")]
	public static bool isMultiplayer
	{
		get
		{
			return IsMultiplayer;
		}
		set
		{
			IsMultiplayer = value;
		}
	}

	public static int CurrentLevel => SceneManager.GetActiveScene().buildIndex;

	public static string CurrentLevelName => SceneManager.GetActiveScene().name;

	public static void Quit(string webplayerQuitURL = "http://google.com")
	{
		Application.Quit();
	}
}

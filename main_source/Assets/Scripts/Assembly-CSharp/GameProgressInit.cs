using UnityEngine;

public class GameProgressInit : MonoBehaviour
{
	private void Start()
	{
		SaveGameProgressPump.EnsureRunning();
		SaveGameProgress.Init();
	}
}

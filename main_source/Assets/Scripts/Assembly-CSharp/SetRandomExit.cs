using UnityEngine;

public class SetRandomExit : MonoBehaviour
{
	[SerializeField]
	private GameObject simpleWall;

	[SerializeField]
	private GameObject exit;

	public void SetExit(bool isExitDoor)
	{
		if (simpleWall != null)
		{
			simpleWall.SetActive(!isExitDoor);
			if (exit != null)
			{
				exit.SetActive(isExitDoor);
			}
		}
	}
}

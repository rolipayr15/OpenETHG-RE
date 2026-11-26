using UnityEngine;

public class CharliePortal : MonoBehaviour
{
	[SerializeField]
	private Transform entryPoint;

	public bool isNext;

	private void Start()
	{
		int enemy = (int)UserGameSettings.currentEnemy;
		if (enemy != 1 && enemy != 5)
		{
			gameObject.SetActive(false);
		}
	}

	public Transform GetEntryPoint()
	{
		return entryPoint;
	}

	public void SetAsNext()
	{
		isNext = true;
	}

	public void SetAsPrev()
	{
		isNext = false;
	}
}

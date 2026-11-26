using UnityEngine;

public class China : MonoBehaviour
{
	[SerializeField]
	private bool availableOnlyInChina;

	[SerializeField]
	private bool disableForChina;

	private void Start()
	{
		if (availableOnlyInChina)
		{
			gameObject.SetActive(false);
		}
	}
}

using UnityEngine;

public class PlatformDependentActivation : MonoBehaviour
{
	[SerializeField]
	private bool android;

	[SerializeField]
	private bool ios;

	[SerializeField]
	private bool disableWhenNBO;

	[SerializeField]
	private bool disableForChina;

	[SerializeField]
	private bool disableForChina2;

	private void OnEnable()
	{
		if (!android)
		{
			gameObject.SetActive(false);
		}
		if (disableWhenNBO && UserGameSettings.googleNBO && gameObject != null)
		{
			gameObject.SetActive(false);
		}
	}
}

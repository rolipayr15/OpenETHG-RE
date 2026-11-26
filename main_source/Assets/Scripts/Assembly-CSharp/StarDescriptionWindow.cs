using UnityEngine;

public class StarDescriptionWindow : MonoBehaviour
{
	[SerializeField]
	private GameObject starDescriptionWindow;

	public void ShowWindow()
	{
		starDescriptionWindow.SetActive(true);
	}

	public void CloseWindow()
	{
		starDescriptionWindow.SetActive(false);
	}
}

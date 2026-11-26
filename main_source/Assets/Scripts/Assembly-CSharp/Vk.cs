using EyesAnalytics;
using I2.Loc;
using UnityEngine;

public class Vk : MonoBehaviour
{
	private void OnEnable()
	{
		gameObject.SetActive(LocalizationManager.CurrentLanguage == "Russian");
	}

	public void OpenFanpage()
	{
		Breadcrumbs.Log("menu_button_vk");
		Application.OpenURL("http://vk.com/club162211446");
	}
}

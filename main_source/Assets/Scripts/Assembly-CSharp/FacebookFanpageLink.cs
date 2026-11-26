using EyesAnalytics;
using I2.Loc;
using UnityEngine;

public class FacebookFanpageLink : MonoBehaviour
{
	private string facebookAddress = "https://www.facebook.com/EyesTheHorrorGame";

	private void Start()
	{
		gameObject.SetActive(LocalizationManager.CurrentLanguage != "Russian");
	}

	public void OpenFanpage()
	{
		Breadcrumbs.Log("menu_button_fb");
		Application.OpenURL(facebookAddress);
	}
}

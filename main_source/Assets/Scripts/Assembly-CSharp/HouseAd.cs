using EyesAnalytics;
using I2.Loc;
using UnityEngine;

public class HouseAd : MonoBehaviour
{
	[SerializeField]
	private GameObject russianButtonsFb;

	[SerializeField]
	private GameObject russianButtonsVk;

	[SerializeField]
	private GameObject fbButton;

	private void Start()
	{
		bool russian = LocalizationManager.CurrentLanguage == "Russian";
		russianButtonsFb.SetActive(russian);
		russianButtonsVk.SetActive(russian);
		fbButton.SetActive(!russian);
	}

	public void OpenVk()
	{
		Breadcrumbs.Log("House ads - VK");
		Application.OpenURL("https://vk.com/eyesthehorrorgameofficial");
	}
}

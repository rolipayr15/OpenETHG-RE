using UnityEngine;

public class DisableForPlatform : MonoBehaviour
{
	[SerializeField]
	private bool chinaFreemium;

	[SerializeField]
	private bool chinaIAP;

	[SerializeField]
	private bool nonChinaVersion;

	[SerializeField]
	private bool googleNBO;

	[SerializeField]
	private bool nintendoSwitch;

	[SerializeField]
	private bool huaweiStore;

	[SerializeField]
	private bool standalone;

	[SerializeField]
	private bool ios;

	[SerializeField]
	private bool availableForEditor;

	[SerializeField]
	private bool china2;

	[SerializeField]
	private bool nonChina2;

	private void Start()
	{
		if (nonChinaVersion)
		{
			gameObject.SetActive(false);
		}
		if (UserGameSettings.googleNBO && googleNBO)
		{
			gameObject.SetActive(false);
		}
		if (nonChina2)
		{
			gameObject.SetActive(false);
		}
	}
}

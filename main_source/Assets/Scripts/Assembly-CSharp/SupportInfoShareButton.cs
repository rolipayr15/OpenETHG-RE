using GameAuthentication;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class SupportInfoShareButton : MonoBehaviour
{
	[Inject]
	private IAuthCoordinator authCoordinator;

	private Button button;

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void CopySupportInfoToClipboard()
	{
	}
}

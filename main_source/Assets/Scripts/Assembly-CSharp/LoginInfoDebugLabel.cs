using GameAuthentication;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class LoginInfoDebugLabel : MonoBehaviour
{
	[Inject]
	private IAuthCoordinator authCoordinator;

	private Text infoDebugLabel;

	private Button infoDebugButton;

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void Start()
	{
	}

	private void UpdateInfoLabel(AuthState state)
	{
	}

	private void copyIdToClipboard()
	{
	}
}

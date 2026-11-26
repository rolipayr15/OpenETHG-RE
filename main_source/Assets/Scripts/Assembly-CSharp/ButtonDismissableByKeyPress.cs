using UnityEngine;
using UnityEngine.UI;

public class ButtonDismissableByKeyPress : MonoBehaviour
{
	[SerializeField]
	private DialogButtonMapping buttonActionToListenFor;

	private Button button;

	private bool shouldClickButton;

	private bool isAcceptPressed;

	private bool isDeclinePressed;

	private void Start()
	{
		button = GetComponent<Button>();
		shouldClickButton = false;
		isAcceptPressed = false;
		isDeclinePressed = false;
	}

	private void Update()
	{
		if (button == null)
		{
			return;
		}
		switch (buttonActionToListenFor)
		{
		case DialogButtonMapping.GenericDismiss:
			isAcceptPressed = InteractionManager.GetButtonDown(33);
			isDeclinePressed = InteractionManager.GetButtonDown(34);
			shouldClickButton = isAcceptPressed || isDeclinePressed;
			break;
		case DialogButtonMapping.Decline:
			shouldClickButton = InteractionManager.GetButtonDown(34);
			break;
		case DialogButtonMapping.Accept:
			shouldClickButton = InteractionManager.GetButtonDown(33);
			break;
		default:
			shouldClickButton = false;
			break;
		}
		if (shouldClickButton)
		{
			button.onClick.Invoke();
			buttonActionToListenFor = DialogButtonMapping.None;
		}
	}
}

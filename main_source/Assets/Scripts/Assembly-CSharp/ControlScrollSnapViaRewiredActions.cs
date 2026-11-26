using Rewired;
using RewiredConsts;
using UnityEngine;
using UnityEngine.UI.Extensions;

// Pages a scroll snap with gamepad/keyboard actions.
public class ControlScrollSnapViaRewiredActions : MonoBehaviour
{
	[ActionIdProperty(typeof(Action))]
	[SerializeField]
	private int previousAction;

	[ActionIdProperty(typeof(Action))]
	[SerializeField]
	private int nextAction;

	[SerializeField]
	private ScrollSnapBase scrollSnap;

	private void Start()
	{
		scrollSnap = GetComponent<ScrollSnapBase>();
	}

	private void Update()
	{
		if (InteractionManager.GetButtonDown(previousAction))
		{
			scrollSnap.PreviousScreen();
		}
		if (InteractionManager.GetButtonDown(nextAction))
		{
			scrollSnap.NextScreen();
		}
	}
}

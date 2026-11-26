using Prime31.MessageKit;
using UnityEngine;

public class DeactivateOnMonsterVision : MonoBehaviour
{
	[SerializeField]
	private Behaviour componentToControl;

	private void OnEnable()
	{
		MessageKit<bool>.addObserver(7, SetComponentToEnabled);
	}

	private void SetComponentToEnabled(bool enabled)
	{
		componentToControl.enabled = !enabled;
	}

	private void OnDisable()
	{
		MessageKit<bool>.removeObserver(7, SetComponentToEnabled);
	}
}

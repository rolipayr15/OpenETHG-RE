using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class PointerSelectionProbe : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	public bool SelectedByPointer { get; private set; }

	public void OnSelect(BaseEventData eventData)
	{
		SelectedByPointer = eventData is PointerEventData;
	}

	public static bool WasSelectedByPointer(GameObject target)
	{
		PointerSelectionProbe probe = target != null ? target.GetComponent<PointerSelectionProbe>() : null;
		return probe != null && probe.SelectedByPointer;
	}
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AdTimeoutContinuePress : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler
{
	private readonly Dictionary<int, float> pressStartedAt = new Dictionary<int, float>();

	private float releasedPressStartedAt = -1f;

	private int releasedFrame;

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData != null)
		{
			pressStartedAt[eventData.pointerId] = Time.realtimeSinceStartup;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData != null)
		{
			releasedPressStartedAt = pressStartedAt.TryGetValue(eventData.pointerId, out float started) ? started : -1f;
			pressStartedAt.Remove(eventData.pointerId);
			releasedFrame = Time.frameCount;
		}
	}

	public void ResetPresses()
	{
		pressStartedAt.Clear();
		releasedPressStartedAt = -1f;
	}

	public bool IsFreshPointerRelease(float shownAt)
	{
		return releasedFrame == Time.frameCount && releasedPressStartedAt >= 0f && shownAt <= releasedPressStartedAt;
	}
}

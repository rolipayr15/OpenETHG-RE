using System;
using SimpleGDPRConsent;
using UnityEngine;

public class OurVersionOfSimpleGDPR : MonoBehaviour
{
	[SerializeField]
	private RectTransform ourGDPRpanel;

	[SerializeField]
	private SlidingToggle consentToggle;

	private Action methodToCallAfterDismissingTheWindow;

	public void ShowDialog(Action _methodToCallAfterDismissingTheWindow)
	{
	}

	public static void SetConsentState(string identifier, SimpleGDPR.ConsentState value)
	{
	}

	public void CloseButtonUsed()
	{
	}
}

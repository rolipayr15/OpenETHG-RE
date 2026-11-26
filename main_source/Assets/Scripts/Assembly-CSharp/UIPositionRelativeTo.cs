using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class UIPositionRelativeTo : MonoBehaviour
{

	[SerializeField]
	private RectTransform relativeTo;

	[SerializeField]
	private UIRelativeDirection position;

	private RectTransform rectTransform;

	private Vector3[] referenceWorldCorners;

	private Image image;

	private IEnumerator Start()
	{
		image = GetComponent<Image>();
		image.enabled = false;
		yield return null;
		yield return null;
		rectTransform = GetComponent<RectTransform>();
		referenceWorldCorners = new Vector3[4];
		relativeTo.GetWorldCorners(referenceWorldCorners);
		Vector3 target = relativeTo.position;
		switch (position)
		{
		case UIRelativeDirection.Left:
			target = referenceWorldCorners[0];
			break;
		case UIRelativeDirection.Right:
			target = referenceWorldCorners[2];
			break;
		case UIRelativeDirection.Up:
			target = referenceWorldCorners[1];
			break;
		case UIRelativeDirection.Down:
			target = referenceWorldCorners[3];
			break;
		}
		rectTransform.position = target;
		image.enabled = true;
	}
}

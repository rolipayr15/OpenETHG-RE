using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BodyPart
{
	public string bodyName;

	public GameObject[] bodyParts;

	public Button nextBodyPart;

	public Button previosBodyPart;

	public Text bodyNameDisplay;

	public int currentIndex;

	public bool visibleInSuit;
}

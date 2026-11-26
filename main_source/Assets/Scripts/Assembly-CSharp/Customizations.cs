using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Customizations : MonoBehaviour
{
	public List<BodyPart> allParts;

	[Header("Suits Part")]
	public List<BodySuit> allSuits;

	public Button nextSuit;

	public Button previosSuit;

	private int suitIndex;

	public Text suitText;

	private void Start()
	{
	}

	private void NonSuitParts(bool visibility)
	{
	}

	private void ChangeSuit(bool increase)
	{
	}

	private void ChangePart(BodyPart b, bool increase)
	{
	}
}

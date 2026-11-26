using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class JournalScrollView : FancyScrollView<JournalEntry>
{
	[SerializeField]
	private ScrollPositionController scrollPositionController;

	private new void Awake()
	{
	}

	public void UpdateData(List<JournalEntry> data)
	{
	}
}

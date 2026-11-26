using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class LevelScrollView : FancyScrollView<LevelCellData>
{
	[SerializeField]
	private ScrollPositionController scrollPositionController;

	private new void Awake()
	{
		base.Awake();
		scrollPositionController.OnUpdatePosition.AddListener(UpdatePosition);
	}

	public void UpdateData(List<LevelCellData> data)
	{
		cellData = data;
		scrollPositionController.SetDataCount(cellData.Count);
		UpdateContents();
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScrollInit : MonoBehaviour
{
	[SerializeField]
	private List<LevelCellData> sprites;

	[SerializeField]
	private LevelScrollView scrollView;

	private IEnumerator Start()
	{
		scrollView.UpdateData(sprites);
		yield return null;
		scrollView.UpdateData(sprites);
	}
}

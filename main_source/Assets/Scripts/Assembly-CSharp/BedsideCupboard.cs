using UnityEngine;

public class BedsideCupboard : Cupboard
{
	private Material[] materials_;

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		materials_ = new Material[2] { materialAfterInteraction, materialAfterInteraction };
		furnitureRenderer.materials = materials_;
		base.Clicked(isClickedByOtherPlayer);
	}
}

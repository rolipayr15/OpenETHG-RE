using UnityEngine;

public class Call : Interactable
{
	[SerializeField]
	private Phone phone;

	private bool firstTime = true;

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		base.Clicked(isClickedByOtherPlayer);
		if (firstTime)
		{
			phone.Call();
			firstTime = false;
		}
		else
		{
			phone.FinishCall();
		}
		InteractionEnd();
	}
}

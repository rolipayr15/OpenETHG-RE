using UnityEngine;

public class RestorePurchasesButton : MonoBehaviour
{
	public void Restore()
	{
		Purchaser.Instance.RestorePurchasesButton();
	}
}

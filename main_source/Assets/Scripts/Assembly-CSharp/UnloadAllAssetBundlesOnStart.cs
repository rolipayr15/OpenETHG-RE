using UnityEngine;

public class UnloadAllAssetBundlesOnStart : MonoBehaviour
{
	private void Start()
	{
		if (UserGameSettings.dynamicDelivery)
		{
			AssetBundleManager.UnloadAllBundles();
		}
	}
}

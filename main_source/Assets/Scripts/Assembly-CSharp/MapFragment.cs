using Prime31.MessageKit;
using UnityEngine;

public class MapFragment : MonoBehaviour
{
	[SerializeField]
	private int id;

	private void Start()
	{
		MessageKit<int>.addObserver(35, OnMapFragmentDiscovered);
	}

	private void OnMapFragmentDiscovered(int mapFragmentId)
	{
		if (id == mapFragmentId)
		{
			MessageKit<int>.removeObserver(35, OnMapFragmentDiscovered);
			GetComponent<UnityEngine.UI.Image>().enabled = true;
		}
	}

	private void OnDestroy()
	{
		MessageKit<int>.removeObserver(35, OnMapFragmentDiscovered);
	}
}

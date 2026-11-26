using Prime31.MessageKit;
using UnityEngine;

public class PotionEffectsStatus : MonoBehaviour
{
	[SerializeField]
	private GameObject cloak;

	[SerializeField]
	private GameObject bagsVision;

	[SerializeField]
	private GameObject drunk;

	[SerializeField]
	private GameObject limitedVision;

	[SerializeField]
	private GameObject sprint;

	private void Start()
	{
		MessageKit<bool>.addObserver(119, Cloak);
		MessageKit<bool>.addObserver(120, BagsVision);
		MessageKit<bool>.addObserver(121, Drunk);
		MessageKit<bool>.addObserver(122, LimitedVision);
		MessageKit<bool>.addObserver(123, Sprint);
	}

	private void Cloak(bool status)
	{
		cloak.SetActive(status);
	}

	private void BagsVision(bool status)
	{
		bagsVision.SetActive(status);
	}

	private void Drunk(bool status)
	{
		drunk.SetActive(status);
	}

	private void LimitedVision(bool status)
	{
		limitedVision.SetActive(status);
	}

	private void Sprint(bool status)
	{
		sprint.SetActive(status);
	}

	private void OnDestroy()
	{
		MessageKit<bool>.removeObserver(119, Cloak);
		MessageKit<bool>.removeObserver(120, BagsVision);
		MessageKit<bool>.removeObserver(121, Drunk);
		MessageKit<bool>.removeObserver(122, LimitedVision);
		MessageKit<bool>.removeObserver(123, Sprint);
	}
}

using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;

public class PotionEffectButton : MonoBehaviour
{
	[SerializeField]
	private GameObject drunk;

	[SerializeField]
	private GameObject moneyBags;

	[SerializeField]
	private GameObject limitedVision;

	[SerializeField]
	private GameObject cloak;

	[SerializeField]
	private Image circle;

	[SerializeField]
	private Text counter;

	[SerializeField]
	private GameObject button;

	private void Awake()
	{
		MessageKit<bool>.addObserver(12, Drunk);
		MessageKit<bool>.addObserver(10, BagsVision);
		MessageKit<bool>.addObserver(11, LimitedVision);
		MessageKit<bool>.addObserver(14, Cloak);
		MessageKit.addObserver(17, Death);
		MessageKit.addObserver(53, Revive);
		MessageKit<bool>.addObserver(7, Vision);
		MessageKit<bool>.addObserver(6, Pause);
		MessageKit.addObserver(105, MultiplayerGameFinished);
	}

	private void OnDestroy()
	{
		MessageKit<bool>.removeObserver(12, Drunk);
		MessageKit<bool>.removeObserver(10, BagsVision);
		MessageKit<bool>.removeObserver(11, LimitedVision);
		MessageKit<bool>.removeObserver(14, Cloak);
		MessageKit.removeObserver(17, Death);
		MessageKit.removeObserver(53, Revive);
		MessageKit<bool>.removeObserver(7, Vision);
		MessageKit<bool>.removeObserver(6, Pause);
		MessageKit.removeObserver(105, MultiplayerGameFinished);
	}

	private void TogglePotionEffect(bool isActive)
	{
		circle.enabled = isActive;
		counter.enabled = isActive;
	}

	private void Drunk(bool isActive)
	{
		TogglePotionEffect(isActive);
		drunk.SetActive(isActive);
	}

	private void BagsVision(bool isActive)
	{
		TogglePotionEffect(isActive);
		moneyBags.SetActive(isActive);
	}

	private void LimitedVision(bool isActive)
	{
		TogglePotionEffect(isActive);
		limitedVision.SetActive(isActive);
	}

	private void Cloak(bool isActive)
	{
		TogglePotionEffect(isActive);
		cloak.SetActive(isActive);
	}

	private void Death()
	{
		if (button != null)
		{
			button.SetActive(false);
		}
	}

	private void Revive()
	{
		if (button != null)
		{
			button.SetActive(true);
		}
	}

	private void Vision(bool isVisionActive)
	{
		if (button != null)
		{
			button.SetActive(!isVisionActive);
		}
	}

	private void Pause(bool isGamePaused)
	{
		if (button != null)
		{
			button.SetActive(!isGamePaused);
		}
	}

	private void MultiplayerGameFinished()
	{
		if (button != null)
		{
			button.SetActive(false);
		}
	}
}

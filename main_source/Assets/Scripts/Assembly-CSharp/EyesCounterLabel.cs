using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class EyesCounterLabel : MonoBehaviour
{
	[SerializeField]
	private Text labelEyes;

	[SerializeField]
	private Text labelMeat;

	[SerializeField]
	private Image addMoreEyesImage;

	[SerializeField]
	private UICircle addMoreEyesBackground;

	private int meatCounter;

	private void Awake()
	{
		MessageKit.addObserver(2, UpdateCounterEyes);
		MessageKit.addObserver(28, MeatCollected);
		MessageKit.addObserver(29, MeatLeft);
	}

	private void OnDestroy()
	{
		MessageKit.removeObserver(2, UpdateCounterEyes);
		MessageKit.removeObserver(28, MeatCollected);
		MessageKit.removeObserver(29, MeatLeft);
	}

	private void UpdateCounterEyes()
	{
		int eyes = CustomSingleton<EyesManager>.Instance.GetAvailableEyes();
		labelEyes.text = eyes.ToString();
		addMoreEyesImage.enabled = eyes == 0;
		addMoreEyesBackground.enabled = eyes == 0;
	}

	private void MeatCollected()
	{
		meatCounter++;
		labelMeat.text = meatCounter.ToString();
	}

	private void MeatLeft()
	{
		meatCounter--;
		labelMeat.text = meatCounter.ToString();
	}
}

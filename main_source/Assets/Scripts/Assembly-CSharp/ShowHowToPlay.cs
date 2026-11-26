using EyesAnalytics;
using UnityEngine;

public class ShowHowToPlay : MonoBehaviour
{
	[SerializeField]
	private HowToPlayEntry entry;

	public void Show()
	{
		HowToPlay.Show(entry);
		Screens.View(ScreenId.HowToPlay);
	}
}

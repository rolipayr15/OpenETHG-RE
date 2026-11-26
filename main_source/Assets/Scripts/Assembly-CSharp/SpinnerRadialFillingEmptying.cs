using System.Collections;
using Bibloteka;
using UnityEngine;
using UnityEngine.UI;

public class SpinnerRadialFillingEmptying : MonoBehaviour
{
	[SerializeField]
	private Image spinnerImage;

	private void Start()
	{
		if (!spinnerImage)
		{
			spinnerImage = GetComponent<Image>();
		}
		StartCoroutine(KeepSpinnin());
	}

	public void TurnOff()
	{
		StopAllCoroutines();
		StartCoroutine(TurnOffGradually());
	}

	private IEnumerator TurnOffGradually()
	{
		float start = spinnerImage.fillAmount;
		yield return Utils.ChangeOverTime(start * 0.5f, t => spinnerImage.fillAmount = Mathf.Lerp(start, 0f, t));
		spinnerImage.fillAmount = 0f;
	}

	private IEnumerator KeepSpinnin()
	{
		while (true)
		{
			yield return Utils.ChangeOverTime(1f, t => spinnerImage.fillAmount = 1f - t);
			yield return Utils.GetDelay(0.2f);
			spinnerImage.fillClockwise = !spinnerImage.fillClockwise;
			yield return Utils.ChangeOverTime(1f, t => spinnerImage.fillAmount = t);
			yield return Utils.GetDelay(0.2f);
			spinnerImage.fillClockwise = !spinnerImage.fillClockwise;
		}
	}
}

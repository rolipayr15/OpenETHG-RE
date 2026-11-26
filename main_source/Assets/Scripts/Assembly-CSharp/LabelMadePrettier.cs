using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LabelMadePrettier : MonoBehaviour
{
	[SerializeField]
	private string charsBefore;

	[SerializeField]
	private string charsAfter;

	private void Start()
	{
		Text text = GetComponent<Text>();
		StringBuilder sb = new StringBuilder();
		sb.Append(charsBefore + " ");
		sb.Append(text.text);
		sb.Append(" " + charsAfter);
		text.text = sb.ToString();
	}
}

using UnityEngine;
using UnityEngine.UI;

public class ApplicationVersioon : MonoBehaviour
{
	[SerializeField]
	private Text text;

	private void Start()
	{
		text.text = Application.version;
	}
}

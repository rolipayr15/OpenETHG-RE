using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
	[SerializeField]
	private Text fpsLabel;

	private float deltaTime;

	private StringBuilder sb;

	private void Start()
	{
	}

	private void Update()
	{
	}
}

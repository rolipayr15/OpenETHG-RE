using UnityEngine;

public class MeanderGirl : MonoBehaviour
{
	[SerializeField]
	private Transform objectToMeander;

	[SerializeField]
	private float meanderRadiusSideways;

	[SerializeField]
	private float meanderRadiusUpDown;

	[Header("Don't make them equal!")]
	[SerializeField]
	private float meanderFreqSideways;

	[SerializeField]
	private float meanderFreqUpDown;

	private Vector3 startingPos;

	private void Start()
	{
		startingPos = objectToMeander.localPosition;
	}

	private void Update()
	{
		float time = Time.time;
		objectToMeander.localPosition = new Vector3(startingPos.x + meanderRadiusSideways * Mathf.Sin(time * meanderFreqSideways), startingPos.y + meanderRadiusUpDown * Mathf.Cos(time * meanderFreqUpDown), startingPos.z);
	}
}

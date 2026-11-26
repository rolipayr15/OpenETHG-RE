using UnityEngine;

public class RotateMonsterSpinner : MonoBehaviour
{
	[SerializeField]
	private float speed;

	[SerializeField]
	private GameObject[] images;

	[SerializeField]
	private RectTransform rt;

	private Vector3 currentScale;

	private float change = 0.2f;

	private void OnEnable()
	{
		foreach (GameObject image in images)
		{
			image.SetActive(false);
		}
		images[UnityEngine.Random.Range(0, images.Length)].SetActive(true);
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		rt.Rotate(new Vector3(0f, 0f, deltaTime * speed));
		float step = deltaTime * change;
		Vector3 scale = transform.localScale;
		currentScale = new Vector3(scale.x + step, scale.y + step, scale.z + step);
		if (currentScale.x >= 1.25f)
		{
			change = -0.25f;
		}
		else if (currentScale.x <= 0.75f)
		{
			change = 0.25f;
		}
		transform.localScale = currentScale;
	}
}

using UnityEngine;

public class CollectibleBlink : MonoBehaviour
{
	private void Update()
	{
		Shader.SetGlobalFloat("_CollectibleBlink", Mathf.Sin(Time.time * 5f) * 0.3f + 0.3f);
	}
}

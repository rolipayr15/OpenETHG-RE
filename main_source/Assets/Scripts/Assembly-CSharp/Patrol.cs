using UnityEngine;

public class Patrol : MonoBehaviour
{
	private void Start()
	{
		GetComponent<RatsNavAgent>().Patrol();
	}
}

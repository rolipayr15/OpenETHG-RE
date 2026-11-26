using System.Collections;
using Bibloteka;
using DarkTonic.MasterAudio;
using UnityEngine;

public class ShuttingDoor : MonoBehaviour
{
	[SerializeField]
	private Transform doorMesh;

	private float baseRotation;

	private int rotation = 90;

	private const float animationTime = 1.5f;

	private bool wasPlayed;

	public virtual void Start()
	{
		iTween.Init(doorMesh.gameObject);
		baseRotation = doorMesh.rotation.y;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && !CustomSingleton<FloorManager>.Instance.IsEnemyOnTheSameFloor && Utils.Probability(60) && !wasPlayed)
		{
			StartCoroutine(OpenAndCloseTheDoor());
			wasPlayed = true;
		}
	}

	private IEnumerator OpenAndCloseTheDoor()
	{
		iTween.RotateTo(doorMesh.gameObject, iTween.Hash("y", baseRotation + (float)rotation, "time", 1.5f, "easetype", "easeInOutQuad", "islocal", true));
		MasterAudio.PlaySound3DAtTransform("door", base.transform);
		yield return Utils.GetDelay(1.5f);
		MasterAudio.PlaySound3DAtTransform("fx", base.transform, 1f, null, 0f, "doorSlam");
		iTween.RotateTo(doorMesh.gameObject, iTween.Hash("y", baseRotation, "time", 0.5f, "easetype", "easeInOutQuad", "islocal", true));
	}
}

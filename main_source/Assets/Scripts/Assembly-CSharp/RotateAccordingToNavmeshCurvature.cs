using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.AI;

public class RotateAccordingToNavmeshCurvature : MonoBehaviour
{
	[SerializeField]
	private float lookAhead = 0.1f;

	[SerializeField]
	private Transform monsterT;

	private NavMeshAgent agent;

	private Transform monsterParent;

	private bool deathSequence;

	private Vector3 agentAheadPosition;

	private Vector3 hitPoint;

	private void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		monsterParent = monsterT.parent;
		monsterT.parent = null;
		MessageKit.addObserver(17, StartDeathSequence);
		MessageKit.addObserver(53, Revive);
	}

	private void StartDeathSequence()
	{
		deathSequence = true;
	}

	private void Revive()
	{
		deathSequence = false;
	}

	private void Update()
	{
		if (deathSequence)
		{
			monsterT.LookAt(CustomSingleton<PlayerManager>.Instance.Player.transform);
			return;
		}
		monsterT.position = monsterParent.position;
		Transform agentTransform = agent.transform;
		Vector3 agentPosition = agentTransform.position;
		Vector3 monsterPosition = monsterT.position;
		agentAheadPosition = agentPosition + agentTransform.forward * lookAhead;
		Debug.DrawLine(agentPosition, agentAheadPosition, Color.red);
		if (NavMesh.SamplePosition(agentAheadPosition, out NavMeshHit hit, 0.2f, -1))
		{
			hitPoint = hit.position;
			Vector3 target = monsterPosition - agentPosition + hit.position;
			Quaternion look = Quaternion.LookRotation(target - monsterT.position, Vector3.up);
			Quaternion smooth = Quaternion.Lerp(monsterT.rotation, look, Time.deltaTime * 10f);
			monsterT.rotation = Quaternion.Euler(smooth.eulerAngles.x, look.eulerAngles.y, look.eulerAngles.z);
		}
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawSphere(agent.transform.position, 0.05f);
			Gizmos.DrawSphere(agentAheadPosition, 0.05f);
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(hitPoint, 0.05f);
		}
	}
}

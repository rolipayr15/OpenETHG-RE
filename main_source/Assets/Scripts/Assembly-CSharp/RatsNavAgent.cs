using System.Collections;
using Bibloteka;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RatsNavAgent : MonoBehaviour
{
	private Transform target;

	private NavMeshAgent agent;

	private float targetReachedDistance;

	[SerializeField]
	private GameObject[] _nodes;

	private Coroutine patrolCoroutine;

	private Vector3 lastPosition;

	private void Awake()
	{
		if (_nodes == null)
		{
			_nodes = GameObject.FindGameObjectsWithTag("node");
		}
	}

	private void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		targetReachedDistance = agent.stoppingDistance + 0.1f;
		StartCoroutine(UpdateTargetPosition());
	}

	private IEnumerator PatrolCoroutine()
	{
		while (true)
		{
			target = _nodes[Random.Range(0, _nodes.Length)].transform;
			yield return WaitForPathComputedCoroutine();
			yield return WaitForTargetReachedCoroutine();
		}
	}

	public void Patrol()
	{
		patrolCoroutine = StartCoroutine(PatrolCoroutine());
		lastPosition = base.gameObject.transform.position;
	}

	public void SetPlayerAsTarget()
	{
		StopCoroutine(patrolCoroutine);
		target = CustomSingleton<PlayerManager>.Instance.Player.transform;
	}

	public void SetTargetT(Transform t)
	{
		target = t;
	}

	public void SetTargetGO(GameObject go)
	{
		target = go.transform;
	}

	public void SetTargetV3(Vector3 pos)
	{
		target = null;
		agent.SetDestination(pos);
	}

	public void FindNewTarget()
	{
		target = _nodes[Random.Range(0, _nodes.Length)].transform;
		agent.SetDestination(target.position);
	}

	public void WaitForPathComputed()
	{
		StartCoroutine(WaitForPathComputedCoroutine());
	}

	public void WaitForTargetReached()
	{
		StartCoroutine(WaitForTargetReachedCoroutine());
	}

	private IEnumerator UpdateTargetPosition()
	{
		while (true)
		{
			yield return Utils.GetDelay(0.5f);
			if (target != null)
			{
				agent.SetDestination(target.position);
			}
		}
	}

	private IEnumerator WaitForPathComputedCoroutine()
	{
		yield return null;
		while (agent.pathPending)
		{
			yield return Utils.GetDelay(0.5f);
		}
	}

	private IEnumerator WaitForTargetReachedCoroutine()
	{
		yield return null;
		while (agent.remainingDistance >= targetReachedDistance)
		{
			yield return Utils.GetDelay(0.5f);
		}
	}
}

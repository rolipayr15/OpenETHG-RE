using System.Collections;
using Bibloteka;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.AI;

public class RunningPumpkinFSM : MonoBehaviour
{
	public enum State
	{
		Inactive = 0,
		Running = 1,
		WaitingForPlayer = 2,
		Sitting = 3
	}

	private State state;

	protected GameObject[] nodes;

	private NavMeshAgent agent;

	protected Animator animator;

	protected Collider pickUpCollider;

	protected Coroutine mainLoop;

	private Quaternion rotation;

	[SerializeField]
	protected LayerMask playerRaycastLayers;

	protected Transform playerHeart;

	[SerializeField]
	private Transform raycastOrigin;

	private bool pumpkinJustPickedUp;

	private float runningProbingDelay = 0.1f;

	private bool playerNear;

	private Coroutine soundCoroutine;

	private RaycastHit hit;

	private Vector3 dirVector;

	protected void Start()
	{
		if ((int)UserGameSettings.currentMode != 2)
		{
			base.gameObject.SetActive(false);
			return;
		}
		if (animator == null)
		{
			animator = GetComponent<Animator>();
		}
		if (agent == null)
		{
			agent = GetComponent<NavMeshAgent>();
		}
		nodes = GameObject.FindGameObjectsWithTag("node");
		pickUpCollider = GetComponent<Collider>();
		pickUpCollider.enabled = false;
		playerHeart = CustomSingleton<PlayerManager>.Instance.Heart.transform;
		Activate();
	}

	private void PumpkinPickedUp()
	{
		pumpkinJustPickedUp = true;
	}

	public void Activate()
	{
		FindNewTarget();
		state = State.WaitingForPlayer;
		mainLoop = StartCoroutine(MainLoop());
	}

	private void ResetAnimStates()
	{
		animator.ResetTrigger("run");
		animator.ResetTrigger("sit");
		animator.ResetTrigger("wait");
	}

	private IEnumerator DoRunning()
	{
		if (agent.isOnNavMesh)
		{
			agent.isStopped = true;
			yield return StartCoroutine(WaitForPathComputedCoroutine());
			agent.isStopped = false;
		}
		else
		{
			yield return StartCoroutine(WaitForPathComputedCoroutine());
		}
		soundCoroutine = StartCoroutine(PlaySound("pumpkinRun"));
		ResetAnimStates();
		animator.SetTrigger("run");
		float alreadyRunningForSeconds = 0f;
		while (true)
		{
			if (agent.remainingDistance < 1f)
			{
				state = State.Sitting;
				break;
			}
			if (alreadyRunningForSeconds > 2f)
			{
				state = State.WaitingForPlayer;
				break;
			}
			yield return Utils.GetDelay(runningProbingDelay);
			alreadyRunningForSeconds += runningProbingDelay;
		}
		StopCoroutine(soundCoroutine);
		MasterAudio.StopAllOfSound("Halloween");
	}

	private IEnumerator DoSitting()
	{
		if (agent.isOnNavMesh)
		{
			agent.isStopped = true;
		}
		ResetAnimStates();
		animator.SetTrigger("sit");
		pickUpCollider.enabled = true;
		MessageKit.addObserver(21, PumpkinPickedUp);
		while (!pumpkinJustPickedUp)
		{
			yield return null;
		}
		pickUpCollider.enabled = false;
		MessageKit.removeObserver(21, PumpkinPickedUp);
		pumpkinJustPickedUp = false;
		ResetAnimStates();
		animator.SetTrigger("run");
		FindNewTarget();
		state = State.Running;
	}

	private IEnumerator DoWaitingForPlayer()
	{
		soundCoroutine = StartCoroutine(PlaySound("pumpkinSit"));
		if (agent.isOnNavMesh)
		{
			agent.isStopped = true;
		}
		ResetAnimStates();
		animator.SetTrigger("wait");
		while ((base.transform.position - CustomSingleton<PlayerManager>.Instance.Player.transform.position).sqrMagnitude >= 4f)
		{
			base.transform.Rotate(Vector3.up, Time.deltaTime * 180f);
			yield return null;
		}
		StopCoroutine(soundCoroutine);
		MasterAudio.StopAllOfSound("Halloween");
		state = State.Running;
	}

	private IEnumerator PlaySound(string soundVariation)
	{
		bool isPlaying = false;
		while (true)
		{
			FloorManager floorManager = CustomSingleton<FloorManager>.Instance;
			if (floorManager.GetFloor(base.transform.position) == floorManager.GetPlayerFloor() || RaycastPlayer())
			{
				if (!isPlaying)
				{
					MasterAudio.PlaySound3DFollowTransform("Halloween", base.transform, 1f, null, 0f, soundVariation);
					isPlaying = true;
				}
			}
			else if (isPlaying)
			{
				MasterAudio.StopAllOfSound("Halloween");
				isPlaying = false;
			}
			yield return Utils.GetDelay(1f);
		}
	}

	private IEnumerator WaitForPlayerToCatchUp()
	{
		while (!playerNear)
		{
			yield return null;
		}
	}

	protected IEnumerator MainLoop()
	{
		while (true)
		{
			if (state == State.Running)
			{
				yield return StartCoroutine(DoRunning());
			}
			if (state == State.Sitting)
			{
				yield return StartCoroutine(DoSitting());
			}
			if (state == State.WaitingForPlayer)
			{
				yield return StartCoroutine(DoWaitingForPlayer());
			}
			yield return null;
		}
	}

	private void DebugState()
	{
	}

	public void FindNewTarget()
	{
		GameObject node;
		do
		{
			node = nodes[Random.Range(0, nodes.Length)];
		}
		while (Mathf.Abs(CustomSingleton<PlayerManager>.Instance.Player.transform.position.y - node.transform.position.y) <= 1f);
		agent.SetDestination(node.transform.position);
	}

	protected IEnumerator WaitForPathComputedCoroutine()
	{
		yield return null;
		while (agent.pathPending)
		{
			yield return Utils.GetDelay(0.2f);
		}
	}

	protected bool RaycastPlayer()
	{
		dirVector = playerHeart.position - raycastOrigin.position;
		Physics.Raycast(raycastOrigin.position, dirVector, out hit, 18f, playerRaycastLayers);
		if (hit.collider != null)
		{
			return hit.collider.gameObject.layer == LayerMask.NameToLayer("LocalPlayer");
		}
		return false;
	}
}

using UnityEngine;
using UnityEngine.AI;

public class PassSpeedToAnimator : MonoBehaviour
{
	private NavMeshAgent agent;

	[SerializeField]
	private Animator animator;

	private float lastSpeed;

	private void Start()
	{
		agent = GetComponent<NavMeshAgent>();
		if (animator == null)
		{
			animator = GetComponent<Animator>();
		}
		if (agent != null)
		{
			lastSpeed = agent.speed;
			if (animator != null)
			{
				animator.SetFloat("agent_speed", lastSpeed);
			}
		}
	}

	private void Update()
	{
		if (agent != null && agent.speed != lastSpeed)
		{
			lastSpeed = agent.speed;
			animator.SetFloat("agent_speed", lastSpeed);
		}
	}
}

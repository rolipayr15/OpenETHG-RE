using System.Collections.Generic;
using UnityEngine;

public class FreeCameraLogic : MonoBehaviour
{
	private Transform m_currentTarget;

	private float m_distance;

	private float m_height;

	private float m_lookAtAroundAngle;

	[SerializeField]
	private List<Transform> m_targets;

	private int m_currentIndex;

	private void Start()
	{
	}

	private void SwitchTarget(int step)
	{
	}

	public void NextTarget()
	{
	}

	public void PreviousTarget()
	{
	}

	private void Update()
	{
	}

	private void LateUpdate()
	{
	}
}

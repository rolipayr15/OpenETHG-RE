using System.Collections.Generic;
using UnityEngine;

public class SimpleSampleCharacterControl : MonoBehaviour
{
	private enum ControlMode
	{
		Tank = 0,
		Direct = 1
	}

	[SerializeField]
	private float m_moveSpeed;

	[SerializeField]
	private float m_turnSpeed;

	[SerializeField]
	private float m_jumpForce;

	[SerializeField]
	private Animator m_animator;

	[SerializeField]
	private Rigidbody m_rigidBody;

	[SerializeField]
	private ControlMode m_controlMode;

	private float m_currentV;

	private float m_currentH;

	private readonly float m_interpolation;

	private readonly float m_walkScale;

	private readonly float m_backwardsWalkScale;

	private readonly float m_backwardRunScale;

	private bool m_wasGrounded;

	private Vector3 m_currentDirection;

	private float m_jumpTimeStamp;

	private float m_minJumpInterval;

	private bool m_jumpInput;

	private bool m_isGrounded;

	private List<Collider> m_collisions;

	private void Awake()
	{
	}

	private void OnCollisionEnter(Collision collision)
	{
	}

	private void OnCollisionStay(Collision collision)
	{
	}

	private void OnCollisionExit(Collision collision)
	{
	}

	private void Update()
	{
	}

	private void FixedUpdate()
	{
	}

	private void TankUpdate()
	{
	}

	private void DirectUpdate()
	{
	}

	private void JumpingAndLanding()
	{
	}
}

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class vp_CharacterController : vp_Controller
{
	private CharacterController m_CharacterController;

	public CharacterController CharacterController
	{
		get
		{
			if (m_CharacterController == null)
			{
				m_CharacterController = base.gameObject.GetComponent<CharacterController>();
			}
			return m_CharacterController;
		}
	}

	protected virtual float OnValue_StepOffset => CharacterController.stepOffset;

	protected virtual float OnValue_SlopeLimit => CharacterController.slopeLimit;

	protected override float OnValue_Radius => CharacterController.radius;

	protected override float OnValue_Height => CharacterController.height;

	protected override void InitCollider()
	{
		m_NormalHeight = CharacterController.height;
		CharacterController.center = (m_NormalCenter = m_NormalHeight * (Vector3.up * 0.5f));
		CharacterController.radius = m_NormalHeight * 0.25f;
		m_CrouchHeight = m_NormalHeight * PhysicsCrouchHeightModifier;
		m_CrouchCenter = m_NormalCenter * PhysicsCrouchHeightModifier;
	}

	protected override void RefreshCollider()
	{
		if (base.Player.Crouch.Active && (!MotorFreeFly || base.Grounded))
		{
			CharacterController.height = m_NormalHeight * PhysicsCrouchHeightModifier;
			CharacterController.center = m_NormalCenter * PhysicsCrouchHeightModifier;
		}
		else
		{
			CharacterController.height = m_NormalHeight;
			CharacterController.center = m_NormalCenter;
		}
	}

	protected virtual void OnMessage_Move(Vector3 direction)
	{
		if (CharacterController.enabled)
		{
			CharacterController.Move(direction);
		}
	}
}

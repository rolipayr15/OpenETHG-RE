using UnityEngine;

public class CableSway : MonoBehaviour
{
	[Tooltip("Degrees of swing around Z (side-side).")]
	public float swayAngleX;

	[Tooltip("Base speed of sway.")]
	public float swaySpeed;

	private float speedJitter;

	private float angleDrift;

	private float driftSpeed;

	[Header("Axis")]
	[Tooltip("If true, sway is applied in local space (recommended).")]
	public bool useLocalRotation;

	private float speedJitterNormal;

	private float angleDriftNormal;

	private float driftSpeedNormal;

	private float speedJitterBlackout;

	private float angleDriftBlackout;

	private float driftSpeedBlackout;

	private Quaternion _initialRot;

	private float _seed;

	private void Awake()
	{
	}

	private void Update()
	{
	}
}

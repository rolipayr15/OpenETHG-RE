using System;
using UnityEngine;

public class vp_FPCameraSmoothToggle : MonoBehaviour
{
	public Vector3 TargetPos;

	[Range(0f, 30f)]
	public float InterpolationSpeed = 10f;

	[Range(0f, 0.1f)]
	public float RepositionSnap = 0.001f;

	public float RerotateSnap = 0.1f;

	protected int m_WieldedWeapon;

	protected Vector3 m_OriginalAngle = Vector3.zero;

	protected bool m_Restoring;

	public Action InterpolateMethod;

	private vp_FPCamera m_FPCamera;

	private vp_FPController m_FPController;

	private vp_FPPlayerEventHandler m_FPPlayer;

	private vp_FPCamera FPCamera
	{
		get
		{
			if (m_FPCamera == null)
			{
				m_FPCamera = base.transform.GetComponent<vp_FPCamera>();
			}
			return m_FPCamera;
		}
	}

	private vp_FPController FPController
	{
		get
		{
			if (m_FPController == null)
			{
				m_FPController = UnityEngine.Object.FindObjectOfType<vp_FPController>();
			}
			return m_FPController;
		}
	}

	private vp_FPPlayerEventHandler FPPlayer
	{
		get
		{
			if (m_FPPlayer == null)
			{
				m_FPPlayer = UnityEngine.Object.FindObjectOfType<vp_FPPlayerEventHandler>();
			}
			return m_FPPlayer;
		}
	}

	protected void Start()
	{
		if (FPCamera == null)
		{
			Debug.LogError("Error (" + this?.ToString() + ") This script must sit on the same gameobject as a vp_FPCamera component.");
		}
		InterpolateMethod = delegate
		{
			FPCamera.transform.position = Vector3.Lerp(FPCamera.transform.position, TargetPos, Time.deltaTime * InterpolationSpeed);
			FPCamera.transform.eulerAngles = new Vector3(Mathf.LerpAngle(FPCamera.transform.eulerAngles.x, m_OriginalAngle.x, Time.deltaTime * InterpolationSpeed), Mathf.LerpAngle(FPCamera.transform.eulerAngles.y, m_OriginalAngle.y, Time.deltaTime * InterpolationSpeed), 0f);
		};
	}

	public void DisableFPCamera()
	{
		m_OriginalAngle = FPCamera.transform.eulerAngles;
		m_WieldedWeapon = FPPlayer.CurrentWeaponIndex.Get();
		FPPlayer.SetWeapon.TryStart(0);
		FPController.Stop();
		FPPlayer.InputAllowGameplay.Set(o: false);
		FPCamera.SnapSprings();
		FPCamera.StopSprings();
		FPCamera.enabled = false;
	}

	public void EnableFPCamera()
	{
		m_Restoring = true;
	}

	protected void LateUpdate()
	{
		if (m_Restoring)
		{
			TargetPos = FPController.SmoothPosition + FPController.Transform.TransformDirection(FPCamera.PositionOffset);
			InterpolateMethod();
			if (Vector3.Distance(FPCamera.transform.position, TargetPos) <= RepositionSnap && Mathf.DeltaAngle(FPCamera.transform.eulerAngles.x, m_OriginalAngle.x) < RerotateSnap && Mathf.DeltaAngle(FPCamera.transform.eulerAngles.y, m_OriginalAngle.y) < RerotateSnap)
			{
				FPPlayer.SetWeapon.TryStart(m_WieldedWeapon);
				FPPlayer.InputAllowGameplay.Set(o: true);
				FPCamera.enabled = true;
				m_Restoring = false;
			}
		}
	}
}

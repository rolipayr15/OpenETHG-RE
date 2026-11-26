using UnityEngine;

public class vp_Billboard : MonoBehaviour
{
	private Transform m_Transform;

	private Camera mainCamera;

	protected virtual void Start()
	{
		m_Transform = base.transform;
		mainCamera = Camera.main;
	}

	protected virtual void Update()
	{
		if (vp_Gameplay.IsVR && !Application.isEditor)
		{
			if (Camera.current != null)
			{
				m_Transform.LookAt(Camera.current.transform);
			}
		}
		else if (mainCamera != null)
		{
			m_Transform.LookAt(mainCamera.transform);
		}
	}
}

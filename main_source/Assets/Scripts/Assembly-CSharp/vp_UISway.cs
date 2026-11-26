using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_UISway : MonoBehaviour
{
	public bool BobStep = true;

	public float SwaySpeed = 10f;

	public float SwayAmount = 0.5f;

	public List<Transform> ObjectsToSway = new List<Transform>();

	protected List<Vector3> m_ObjectPositions = new List<Vector3>();

	protected List<Vector3> m_ObjectScales = new List<Vector3>();

	protected vp_FPCamera m_Camera;

	protected vp_UIManager m_Manager;

	protected virtual void Awake()
	{
		m_Manager = base.transform.root.GetComponentInChildren<vp_UIManager>();
		m_Camera = m_Manager.Player.GetComponentInChildren<vp_FPCamera>();
		SetObjectPositions();
	}

	protected virtual void SetObjectPositions()
	{
		m_ObjectPositions.Clear();
		for (int i = 0; i < ObjectsToSway.Count; i++)
		{
			m_ObjectPositions.Add(ObjectsToSway[i].localPosition);
			m_ObjectScales.Add(ObjectsToSway[i].localScale);
		}
	}

	protected virtual void OnEnable()
	{
		if (m_Camera != null)
		{
			vp_FPCamera camera = m_Camera;
			camera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Combine(camera.BobStepCallback, new vp_FPCamera.BobStepDelegate(Bob));
		}
		vp_GlobalEvent.Register("Update UI Positions", SetObjectPositions);
	}

	protected virtual void OnDisable()
	{
		if (m_Camera != null)
		{
			vp_FPCamera camera = m_Camera;
			camera.BobStepCallback = (vp_FPCamera.BobStepDelegate)Delegate.Remove(camera.BobStepCallback, new vp_FPCamera.BobStepDelegate(Bob));
		}
		vp_GlobalEvent.Unregister("Update UI Positions", SetObjectPositions);
	}

	protected virtual void Bob()
	{
		if (!(m_Manager == null) && BobStep)
		{
			float num = (m_Manager.Player.Run.Active ? 1f : m_Manager.Player.InputMoveVector.Get().y);
			for (int i = 0; i < ObjectsToSway.Count; i++)
			{
				ObjectsToSway[i].localPosition = Vector3.Slerp(new Vector3(ObjectsToSway[i].localPosition.x - m_Manager.Player.InputMoveVector.Get().x * Time.deltaTime * SwayAmount, ObjectsToSway[i].localPosition.y - num * Time.deltaTime * SwayAmount, m_ObjectPositions[i].z), m_ObjectPositions[i], Time.deltaTime * SwaySpeed);
			}
		}
	}

	protected virtual void Update()
	{
		if (vp_TimeUtility.Paused)
		{
			return;
		}
		for (int i = 0; i < ObjectsToSway.Count; i++)
		{
			Vector3 localPosition = ObjectsToSway[i].localPosition;
			localPosition.x = Mathf.Lerp(localPosition.x - vp_Input.GetAxisRaw("Mouse X") * Time.deltaTime * SwayAmount, m_ObjectPositions[i].x, Time.deltaTime * SwaySpeed);
			localPosition.y = Mathf.Lerp(localPosition.y - vp_Input.GetAxisRaw("Mouse Y") * Time.deltaTime * SwayAmount, m_ObjectPositions[i].y, Time.deltaTime * SwaySpeed);
			if (!(m_Manager == null))
			{
				localPosition.x = Mathf.Lerp(localPosition.x - m_Manager.Player.InputMoveVector.Get().x * Time.deltaTime * SwayAmount, m_ObjectPositions[i].x, Time.deltaTime * SwaySpeed);
				ObjectsToSway[i].localPosition = localPosition;
				float num = (m_Manager.Player.Run.Active ? 2f : m_Manager.Player.InputMoveVector.Get().y) * 0.25f;
				float num2 = Mathf.Lerp(ObjectsToSway[i].localScale.x + num * Time.deltaTime, m_ObjectScales[i].x, Time.deltaTime * SwaySpeed);
				Vector3 localScale = new Vector3(num2, num2, num2);
				ObjectsToSway[i].localScale = localScale;
			}
		}
	}
}

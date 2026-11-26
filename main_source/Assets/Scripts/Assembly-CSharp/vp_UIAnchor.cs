using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class vp_UIAnchor : MonoBehaviour
{
	public enum vp_UIAnchorSide
	{
		Middle,
		TopLeft,
		Top,
		TopRight,
		Right,
		BottomRight,
		Bottom,
		BottomLeft,
		Left
	}

	public vp_UIManager Manager;

	public vp_UIAnchorSide Side;

	public List<vp_UIControl> Controls = new List<vp_UIControl>();

	protected Transform m_Transform;

	protected virtual void Awake()
	{
		Init();
	}

	protected virtual void Init()
	{
		m_Transform = base.transform;
		if (Manager == null)
		{
			Manager = m_Transform.root.GetComponent<vp_UIManager>();
		}
		if (Controls == null || Controls.Count == 0 || Application.isPlaying)
		{
			Controls = m_Transform.ChildComponentsToList<vp_UIControl>();
		}
	}

	public virtual void RefreshUI()
	{
		Init();
		float x = Manager.RelativeResolution.x / Manager.RelativeResolution.y * 2f;
		Bounds bounds = new Bounds(Manager.UICamera.transform.position, new Vector3(x, 2f, 0f));
		Vector3 position = new Vector3(0f, 0f, 0f);
		if (Side == vp_UIAnchorSide.BottomRight || Side == vp_UIAnchorSide.Right || Side == vp_UIAnchorSide.TopRight)
		{
			position.x = bounds.max.x;
		}
		else if (Side == vp_UIAnchorSide.Top || Side == vp_UIAnchorSide.Bottom || Side == vp_UIAnchorSide.Middle)
		{
			position.x = bounds.center.x;
		}
		else
		{
			position.x = bounds.min.x;
		}
		if (Side == vp_UIAnchorSide.Top || Side == vp_UIAnchorSide.TopLeft || Side == vp_UIAnchorSide.TopRight)
		{
			position.y = bounds.max.y;
		}
		else if (Side == vp_UIAnchorSide.Left || Side == vp_UIAnchorSide.Middle || Side == vp_UIAnchorSide.Right)
		{
			position.y = bounds.center.y;
		}
		else
		{
			position.y = bounds.min.y;
		}
		m_Transform.position = position;
		m_Transform.localScale = Vector3.one;
		m_Transform.position = vp_UIManager.PixelPerfect(base.transform.position);
	}

	protected virtual void Update()
	{
		if (!(Manager == null) && !Application.isPlaying)
		{
			Manager.ForceUIRefresh();
		}
	}
}

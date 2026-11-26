using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(BoxCollider))]
public class vp_UIDropdownList : vp_UIControl
{
	public string Items = "New Item 1\nNew Item 2\nNew Item 3";

	public Transform Background;

	public TextMesh Label;

	public Texture ItemBackground;

	public float ItemHeight = 20f;

	public Vector2 ItemLabelPadding = new Vector2(5f, 5f);

	public Font ItemFont;

	public string CurrentItem = "";

	protected Dictionary<string, GameObject> m_Items = new Dictionary<string, GameObject>();

	protected GameObject ListContainer;

	protected bool m_ListShown = true;

	public bool ListShown
	{
		get
		{
			return m_ListShown;
		}
		set
		{
			m_ListShown = value;
			vp_Utility.Activate(ListContainer, m_ListShown);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		string[] array = Items.Split(new string[1] { "\n" }, StringSplitOptions.None);
		if (array.Length != 0)
		{
			CurrentItem = array[0];
			Label.text = CurrentItem;
		}
		ListContainer = new GameObject("List");
		ListContainer.transform.parent = base.transform;
		ListContainer.transform.localScale = Vector3.one;
		ListContainer.transform.localPosition = Vector3.zero;
		for (int i = 0; i < array.Length; i++)
		{
			CreateListItem(array[i], i).parent = ListContainer.transform;
		}
		ListContainer.transform.localPosition = new Vector3(Background.localPosition.x, Background.localPosition.y - (Background.localScale.y * 0.5f + ItemHeight * 0.5f), 0f);
		ListShown = false;
	}

	protected virtual void OnPressControl()
	{
		ListShown = !ListShown;
		if (!ListShown)
		{
			return;
		}
		foreach (KeyValuePair<string, GameObject> item in m_Items)
		{
			if (item.Key != CurrentItem)
			{
				item.Value.GetComponent<Renderer>().material.color = new Color(0.9f, 0.9f, 0.9f, 1f);
			}
			else
			{
				item.Value.GetComponent<Renderer>().material.color = new Color(0.8f, 0.8f, 0.8f, 1f);
			}
		}
	}

	protected virtual Transform CreateListItem(string itemName, int i)
	{
		GameObject gameObject = new GameObject(itemName);
		gameObject.layer = Background.gameObject.layer;
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		GameObject gameObject2 = new GameObject("Background");
		gameObject2.layer = Background.gameObject.layer;
		gameObject2.transform.parent = gameObject.transform;
		MeshRenderer meshRenderer = gameObject2.AddComponent<MeshRenderer>();
		gameObject2.AddComponent<MeshFilter>().mesh = Background.GetComponent<MeshFilter>().mesh;
		meshRenderer.material.mainTexture = ItemBackground;
		meshRenderer.material.shader = Background.GetComponent<MeshRenderer>().material.shader;
		meshRenderer.receiveShadows = false;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		meshRenderer.material.color = Color.white;
		gameObject2.transform.localScale = new Vector3(Background.localScale.x, ItemHeight, 1f);
		gameObject2.transform.localPosition = new Vector3(Background.localPosition.x, 0f - gameObject2.transform.localScale.y * (float)i, Background.localPosition.z - 20f);
		GameObject obj = new GameObject("Label");
		obj.layer = Background.gameObject.layer;
		obj.transform.parent = gameObject.transform;
		TextMesh textMesh = obj.AddComponent<TextMesh>();
		textMesh.GetComponent<Renderer>().material = ItemFont.material;
		textMesh.text = itemName;
		textMesh.font = ItemFont;
		textMesh.fontSize = 32;
		textMesh.color = Color.black;
		textMesh.anchor = TextAnchor.MiddleLeft;
		textMesh.transform.localScale = Label.transform.localScale * 0.75f;
		textMesh.transform.localPosition = new Vector3(gameObject2.transform.localPosition.x - gameObject2.transform.localScale.x * 0.5f + ItemLabelPadding.x, gameObject2.transform.localPosition.y + textMesh.transform.localScale.y * 0.5f - ItemLabelPadding.y, Background.localPosition.z - 21f);
		BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
		boxCollider.size = new Vector3(gameObject2.transform.localScale.x, gameObject2.transform.localScale.y, 1f);
		boxCollider.center = gameObject2.transform.localPosition;
		vp_UIControl obj2 = gameObject.AddComponent<vp_UIControl>();
		obj2.ReleaseControl = (vp_TouchButtonEventBinding)Delegate.Combine(obj2.ReleaseControl, (vp_TouchButtonEventBinding)delegate
		{
			SelectItem(itemName);
		});
		m_Items.Add(itemName, gameObject2);
		return gameObject.transform;
	}

	protected virtual void SelectItem(string item)
	{
		Label.text = item;
		CurrentItem = item;
		if (ChangeControl != null)
		{
			ChangeControl(this);
		}
		ListShown = false;
	}
}

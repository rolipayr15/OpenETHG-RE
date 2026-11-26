using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class ChoiceGroup : MonoBehaviour
{
	[SerializeField]
	private List<Button> buttons = new List<Button>();

	[SerializeField]
	private int selected = -1;

	[Tooltip("Left of the first button goes to the last one and vice versa.")]
	[SerializeField]
	private bool wrap;

	[SerializeField]
	private Color selectedTint = Color.white;

	[SerializeField]
	private Color unselectedTint = new Color(0.45f, 0.45f, 0.45f, 1f);

	public UnityEvent<int> onChanged = new UnityEvent<int>();

	private readonly List<(Button button, UnityAction action)> listeners = new List<(Button button, UnityAction action)>();

	public int Selected => selected;

	public IReadOnlyList<Button> Buttons => buttons;

	public bool Wrap
	{
		get
		{
			return wrap;
		}
		set
		{
			wrap = value;
			LinkHorizontal(buttons, wrap);
		}
	}

	private void Awake()
	{
		Listen();
	}

	private void OnEnable()
	{
		LinkHorizontal(buttons, wrap);
		Retint();
	}

	private void OnDestroy()
	{
		Unlisten();
	}

	public void SetButtons(IEnumerable<Button> newButtons)
	{
		Unlisten();
		buttons = new List<Button>(newButtons);
		Listen();
		LinkHorizontal(buttons, wrap);
		Retint();
	}

	public void Select(int index, bool notify = true)
	{
		int previous = selected;
		selected = index;
		Retint();
		if (previous != index && notify)
		{
			onChanged.Invoke(index);
		}
	}

	private void Listen()
	{
		Unlisten();
		for (int i = 0; i < buttons.Count; i++)
		{
			int index = i;
			Button button = buttons[i];
			if (button == null)
			{
				continue;
			}
			UnityAction action = () => Select(index);
			button.onClick.AddListener(action);
			listeners.Add((button, action));
		}
	}

	private void Unlisten()
	{
		foreach ((Button button, UnityAction action) in listeners)
		{
			if (button != null)
			{
				button.onClick.RemoveListener(action);
			}
		}
		listeners.Clear();
	}

	private void Link()
	{
		LinkHorizontal(buttons, wrap);
	}

	private void Retint()
	{
		for (int i = 0; i < buttons.Count; i++)
		{
			if (buttons[i] != null)
			{
				Tint(buttons[i], i == selected ? selectedTint : unselectedTint);
			}
		}
	}

	public static void Tint(Button button, Color tint)
	{
		ColorBlock colors = button.colors;
		Color highlighted = new Color(tint.r * 0.96f, tint.g * 0.96f, tint.b * 0.96f, 1f);
		colors.normalColor = tint;
		colors.highlightedColor = highlighted;
		colors.pressedColor = new Color(tint.r * 0.78f, tint.g * 0.78f, tint.b * 0.78f, 1f);
		colors.selectedColor = highlighted;
		button.colors = colors;
	}

	// Explicit left/right gamepad navigation between the active buttons, optionally wrapping around.
	public static void LinkHorizontal(IReadOnlyList<Button> items, bool wrap)
	{
		List<Selectable> active = new List<Selectable>();
		foreach (Button item in items)
		{
			if (item != null && item.gameObject.activeSelf)
			{
				active.Add(item);
			}
		}
		for (int i = 0; i < active.Count; i++)
		{
			Navigation navigation = active[i].navigation;
			navigation.mode = Navigation.Mode.Explicit;
			if (i > 0)
			{
				navigation.selectOnLeft = active[i - 1];
			}
			else if (wrap && active.Count > 1)
			{
				navigation.selectOnLeft = active[active.Count - 1];
			}
			if (i < active.Count - 1)
			{
				navigation.selectOnRight = active[i + 1];
			}
			else if (wrap && active.Count > 1)
			{
				navigation.selectOnRight = active[0];
			}
			active[i].navigation = navigation;
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Keeps gamepad/keyboard UI selection inside one window while its input lease is the top context:
// restores a valid selection when it is lost and rewires explicit navigation between the window's selectables
// by geometry (sliders keep their own value axis). Original navigation is restored when the scope ends.
[DisallowMultipleComponent]
[DefaultExecutionOrder(-1100)]
public sealed class GamepadSelectionScope : MonoBehaviour
{
	private GameplayInputContext.Lease owner;

	private Func<Selectable> preferred;

	private Func<Selectable, bool> include;

	private GameObject lastValid;

	private GameObject selectedByScope;

	private int selectedByScopeFrame = -1;

	private bool wasTop;

	private bool linksApplied;

	private readonly List<Selectable> valid = new List<Selectable>();

	private readonly List<Selectable> linked = new List<Selectable>();

	private readonly Dictionary<Selectable, Navigation> originals = new Dictionary<Selectable, Navigation>();

	public bool IsActive => enabled && owner != null && !owner.IsDisposed;

	public static GamepadSelectionScope Begin(GameObject root, GameplayInputContext.Lease owner, Func<Selectable> preferred = null, Func<Selectable, bool> include = null)
	{
		if (root == null)
		{
			throw new ArgumentNullException("root");
		}
		GamepadSelectionScope scope = root.GetComponent<GamepadSelectionScope>();
		if (scope == null)
		{
			scope = root.AddComponent<GamepadSelectionScope>();
		}
		scope.RestoreLinks();
		scope.owner = owner;
		scope.preferred = preferred;
		scope.include = include;
		scope.lastValid = null;
		scope.enabled = true;
		scope.Apply();
		return scope;
	}

	public static void End(GameObject root)
	{
		if (root == null)
		{
			return;
		}
		GamepadSelectionScope scope = root.GetComponent<GamepadSelectionScope>();
		if (scope != null)
		{
			scope.Stop();
		}
	}

	// Lets click handlers ignore the "submit" of a selection this scope made itself this frame.
	public bool SelectedThisFrame(GameObject target)
	{
		return target != null && selectedByScope == target && selectedByScopeFrame == Time.frameCount;
	}

	// Finds the button whose persistent onClick calls target.method (used to pick default focus by intent).
	public static Selectable FindPersistentButton(GameObject root, UnityEngine.Object target, string method)
	{
		if (root == null)
		{
			return null;
		}
		foreach (Button button in root.GetComponentsInChildren<Button>(true))
		{
			for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
			{
				if (button.onClick.GetPersistentTarget(i) == target && button.onClick.GetPersistentMethodName(i) == method)
				{
					return button;
				}
			}
		}
		return null;
	}

	private void Stop()
	{
		RestoreLinks();
		EventSystem eventSystem = EventSystem.current;
		if (eventSystem != null && IsInside(eventSystem.currentSelectedGameObject))
		{
			eventSystem.SetSelectedGameObject(null);
		}
		owner = null;
		preferred = null;
		include = null;
		lastValid = null;
		selectedByScope = null;
		wasTop = false;
		enabled = false;
	}

	private void OnDisable()
	{
		RestoreLinks();
	}

	private void OnDestroy()
	{
		RestoreLinks();
	}

	private void Update()
	{
		Apply();
	}

	private void LateUpdate()
	{
		Apply();
	}

	public void Apply()
	{
		if (IsActive && GameplayInputContext.Shared.Current.TopOwner == owner)
		{
			wasTop = true;
			CollectValid();
			UpdateLinks();
			EventSystem eventSystem = EventSystem.current;
			if (eventSystem == null)
			{
				return;
			}
			GameObject selected = eventSystem.currentSelectedGameObject;
			if (!SelectsForCurrentScheme())
			{
				// Touch or captured mouse: no highlighted selection.
				if (IsInside(selected))
				{
					eventSystem.SetSelectedGameObject(null);
				}
				return;
			}
			if (IsValidInside(selected))
			{
				lastValid = selected;
				return;
			}
			GameObject target = IsValidInside(lastValid) ? lastValid : Preferred();
			if (target == null && valid.Count != 0)
			{
				target = valid[0].gameObject;
			}
			eventSystem.SetSelectedGameObject(target);
			if (target != null)
			{
				lastValid = target;
				selectedByScope = target;
				selectedByScopeFrame = Time.frameCount;
			}
			return;
		}
		// Another window is on top: give up our links and our selection.
		RestoreLinks();
		EventSystem current = EventSystem.current;
		if (wasTop && current != null && lastValid != null && current.currentSelectedGameObject == lastValid)
		{
			current.SetSelectedGameObject(null);
		}
		wasTop = false;
	}

	private static bool SelectsForCurrentScheme()
	{
		ControlModeManager manager = ControlModeManager.Instance;
		if (manager == null)
		{
			return false;
		}
		if (manager.Mode == ControlMode.Gamepad)
		{
			return true;
		}
		return manager.Mode == ControlMode.KeyboardMouse && !MouseManager.HasConfirmedCapture;
	}

	private GameObject Preferred()
	{
		Selectable candidate = preferred != null ? preferred() : null;
		if (candidate == null || !IsValid(candidate) || !Includes(candidate))
		{
			return null;
		}
		return IsInside(candidate.gameObject) ? candidate.gameObject : null;
	}

	private bool IsInside(GameObject target)
	{
		return target != null && target.transform.IsChildOf(transform);
	}

	private bool IsValidInside(GameObject target)
	{
		if (!IsInside(target))
		{
			return false;
		}
		Selectable selectable = target.GetComponent<Selectable>();
		return selectable != null && IsValid(selectable) && Includes(selectable);
	}

	public static bool IsValid(Selectable selectable)
	{
		if (selectable == null || !selectable.isActiveAndEnabled || !selectable.IsInteractable())
		{
			return false;
		}
		foreach (Canvas canvas in selectable.GetComponentsInParent<Canvas>())
		{
			if (!canvas.enabled)
			{
				return false;
			}
		}
		return true;
	}

	private void CollectValid()
	{
		valid.Clear();
		foreach (Selectable selectable in GetComponentsInChildren<Selectable>(false))
		{
			if (IsValid(selectable) && Includes(selectable))
			{
				valid.Add(selectable);
			}
		}
	}

	private bool Includes(Selectable selectable)
	{
		return include == null || include(selectable);
	}

	private void UpdateLinks()
	{
		if (linksApplied && linked.Count == valid.Count)
		{
			bool same = true;
			for (int i = 0; i < valid.Count; i++)
			{
				if (linked[i] != valid[i])
				{
					same = false;
					break;
				}
			}
			if (same)
			{
				return;
			}
		}
		RestoreLinks();
		foreach (Selectable selectable in valid)
		{
			originals[selectable] = selectable.navigation;
			bool horizontalValue = IsValueAxis(selectable, true);
			bool verticalValue = IsValueAxis(selectable, false);
			selectable.navigation = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				selectOnUp = verticalValue ? null : Nearest(selectable, Vector3.up),
				selectOnDown = verticalValue ? null : Nearest(selectable, Vector3.down),
				selectOnLeft = horizontalValue ? null : Nearest(selectable, Vector3.left),
				selectOnRight = horizontalValue ? null : Nearest(selectable, Vector3.right)
			};
			linked.Add(selectable);
		}
		linksApplied = true;
	}

	// Sliders/scrollbars consume the arrows along their own direction.
	private static bool IsValueAxis(Selectable selectable, bool horizontal)
	{
		if (selectable is Slider slider)
		{
			return ((int)slider.direction > 1) ^ horizontal;
		}
		if (selectable is Scrollbar scrollbar)
		{
			return ((int)scrollbar.direction > 1) ^ horizontal;
		}
		return false;
	}

	private static Vector3 Center(Selectable selectable)
	{
		RectTransform rect = selectable.transform as RectTransform;
		return rect != null ? rect.TransformPoint(rect.rect.center) : selectable.transform.position;
	}

	// Best candidate in the given (local) direction: favours alignment and closeness (cos / distance).
	private Selectable Nearest(Selectable from, Vector3 localDirection)
	{
		Vector3 origin = Center(from);
		Vector3 direction = from.transform.rotation * localDirection;
		float bestScore = float.NegativeInfinity;
		Selectable best = null;
		foreach (Selectable candidate in valid)
		{
			if (candidate == from)
			{
				continue;
			}
			Vector3 offset = Center(candidate) - origin;
			float dot = Vector3.Dot(direction, offset);
			if (dot <= 0f)
			{
				continue;
			}
			float score = dot / offset.sqrMagnitude;
			if (score > bestScore)
			{
				bestScore = score;
				best = candidate;
			}
		}
		return best;
	}

	private void RestoreLinks()
	{
		foreach (KeyValuePair<Selectable, Navigation> original in originals)
		{
			if (original.Key != null)
			{
				original.Key.navigation = original.Value;
			}
		}
		originals.Clear();
		linked.Clear();
		linksApplied = false;
	}
}

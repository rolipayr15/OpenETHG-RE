using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

internal sealed class MenuInputScope
{
	private readonly Dictionary<Behaviour, bool> behaviours = new Dictionary<Behaviour, bool>();

	private readonly Dictionary<Selectable, bool> selectables = new Dictionary<Selectable, bool>();

	private GameplayInputContext.Lease inputLease;

	private readonly GameObject selection;

	private readonly Scene menu;

	private bool restored;

	private static int activeScopes;

	public static bool IsAnyActive => activeScopes > 0;

	public MenuInputScope(Scene menu)
	{
		this.menu = menu;
		activeScopes++;
		inputLease = GameplayInputContext.Shared.Acquire(this, GameplayInputContext.Kind.Modal, GameplayInputContext.WindowInput.Cancel | GameplayInputContext.WindowInput.Submit);
		selection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
		if (EventSystem.current != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
		foreach (GameObject root in menu.GetRootGameObjects())
		{
			foreach (GraphicRaycaster raycaster in root.GetComponentsInChildren<GraphicRaycaster>(true))
			{
				Block(raycaster);
			}
			foreach (MenuContentWindow window in root.GetComponentsInChildren<MenuContentWindow>(true))
			{
				Block(window);
			}
			foreach (Selectable selectable in root.GetComponentsInChildren<Selectable>(true))
			{
				selectables[selectable] = selectable.interactable;
				selectable.interactable = false;
			}
		}
	}

	private void Block(Behaviour behaviour)
	{
		if (behaviour != null)
		{
			behaviours[behaviour] = behaviour.enabled;
			behaviour.enabled = false;
		}
	}

	public void Restore(bool sameMenu)
	{
		if (restored)
		{
			return;
		}
		restored = true;
		activeScopes--;
		inputLease?.Dispose();
		inputLease = null;
		foreach (KeyValuePair<Behaviour, bool> item in behaviours)
		{
			if (item.Key != null)
			{
				item.Key.enabled = item.Value;
			}
		}
		foreach (KeyValuePair<Selectable, bool> item in selectables)
		{
			if (item.Key != null)
			{
				item.Key.interactable = item.Value;
			}
		}
		if (sameMenu && EventSystem.current != null)
		{
			EventSystem.current.SetSelectedGameObject(selection != null && selection.activeInHierarchy ? selection : null);
		}
	}

	public void RestoreCurrentScene()
	{
		Restore(menu.IsValid() && menu.isLoaded && SceneManager.GetActiveScene().handle == menu.handle);
	}
}

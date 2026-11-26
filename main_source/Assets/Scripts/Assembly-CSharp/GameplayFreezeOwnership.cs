using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameplayFreezeOwnership
{
	private sealed class ComponentState
	{
		public vp_Component Component;

		public int Owners;

		public bool Restore;

		public bool Transferred;
	}

	public sealed class Lease : IDisposable
	{
		private readonly List<ComponentState> components = new List<ComponentState>();

		private bool released;

		internal Lease(vp_FPPlayerEventHandler handler)
		{
			if (handler == null)
			{
				return;
			}
			vp_Component[] all = handler.transform.root.GetComponentsInChildren<vp_Component>(true);
			bool[] wasFrozen = new bool[all.Length];
			for (int i = 0; i < all.Length; i++)
			{
				wasFrozen[i] = all[i] != null && all[i].StateEnabled("Freeze");
			}
			handler.SetState("Freeze", true, true, false);
			for (int i = 0; i < all.Length; i++)
			{
				vp_Component component = all[i];
				if (component == null || !component.StateEnabled("Freeze"))
				{
					continue;
				}
				ComponentState state;
				if (!states.TryGetValue(component, out state))
				{
					state = new ComponentState { Component = component, Restore = !wasFrozen[i] };
					states.Add(component, state);
				}
				state.Owners++;
				components.Add(state);
			}
		}

		public void Release(bool handoffToLegacyOwner = false)
		{
			if (released)
			{
				return;
			}
			released = true;
			foreach (ComponentState state in components)
			{
				state.Owners--;
				state.Transferred |= handoffToLegacyOwner;
				if (state.Owners < 1)
				{
					states.Remove(state.Component);
					if (state.Component != null && state.Restore && !state.Transferred)
					{
						state.Component.SetState("Freeze", false, false, false);
					}
				}
			}
			components.Clear();
		}

		public void Dispose()
		{
			Release();
		}
	}

	private static readonly Dictionary<vp_Component, ComponentState> states = new Dictionary<vp_Component, ComponentState>();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetStatics()
	{
		states.Clear();
	}

	public static Lease Acquire(vp_FPPlayerEventHandler handler)
	{
		return new Lease(handler);
	}
}

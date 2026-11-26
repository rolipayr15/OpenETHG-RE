using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

// Short names of the gamepad buttons bound to gameplay actions, used by on-screen hints.
public static class GamepadBindingLabels
{
	public enum Family
	{
		Neutral = 0,
		Xbox = 1,
		PlayStation = 2
	}

	private const int GameplayCategory = 4;

	public static readonly KeyValuePair<string, int>[] Placeholders = BuildPlaceholders();

	private static readonly Dictionary<int, string> labels = new Dictionary<int, string>();

	private static Family family;

	private static bool defaults;

	private static int version;

	private static Player cachedPlayer;

	private static Joystick cachedJoystick;

	private static bool cachedReady;

	private static bool cachedConnected;

	private static bool cached;

	private static int checkedFrame = -1;

	public static Family CurrentFamily
	{
		get
		{
			Refresh();
			return family;
		}
	}

	public static int Version
	{
		get
		{
			Refresh();
			return version;
		}
	}

	public static bool ShowsDefaults
	{
		get
		{
			Refresh();
			return defaults;
		}
	}

	private static KeyValuePair<string, int>[] BuildPlaceholders()
	{
		return new KeyValuePair<string, int>[7]
		{
			new KeyValuePair<string, int>("{Run}", 8),
			new KeyValuePair<string, int>("{Interact}", 9),
			new KeyValuePair<string, int>("{EnemyVision}", 32),
			new KeyValuePair<string, int>("{ThrowMeat}", 51),
			new KeyValuePair<string, int>("{Journal}", 50),
			new KeyValuePair<string, int>("{Map}", 31),
			new KeyValuePair<string, int>("{Menu}", 11)
		};
	}

	public static string Label(int actionId)
	{
		Refresh();
		return labels.TryGetValue(actionId, out string label) ? label : null;
	}

	public static void Invalidate()
	{
		cached = false;
	}

	public static Joystick CurrentJoystick(Player player)
	{
		if (player == null)
		{
			return null;
		}
		IList<Joystick> joysticks = player.controllers.Joysticks;
		if (player.controllers.GetLastActiveController() is Joystick last && last.isConnected && joysticks.Contains(last))
		{
			return last;
		}
		foreach (Joystick joystick in joysticks)
		{
			if (joystick.isConnected)
			{
				return joystick;
			}
		}
		return null;
	}

	public static string FormatHints(string format, Func<int, string> label)
	{
		if (string.IsNullOrEmpty(format))
		{
			return format;
		}
		foreach (KeyValuePair<string, int> placeholder in Placeholders)
		{
			format = format.Replace(placeholder.Key, label(placeholder.Value) ?? "—");
		}
		return format;
	}

	public static string FormatHints(string format)
	{
		return FormatHints(format, Label);
	}

	public static Family Identify(string controllerName)
	{
		string name = (controllerName ?? string.Empty).ToLowerInvariant();
		if (name.Contains("dualshock") || name.Contains("dualsense") || name.Contains("playstation") || name.Contains("sony") || name.Contains("ps4") || name.Contains("ps5"))
		{
			return Family.PlayStation;
		}
		return (name.Contains("xbox") || name.Contains("xinput") || name.Contains("microsoft")) ? Family.Xbox : Family.Neutral;
	}

	public static string ElementName(Family family, int templateElementId)
	{
		bool xbox = family == Family.Xbox;
		bool playStation = family == Family.PlayStation;
		switch (templateElementId)
		{
		case 4:
			return xbox ? "A" : (playStation ? "Cross" : "Bottom");
		case 5:
			return xbox ? "B" : (playStation ? "Circle" : "Right");
		case 7:
			return xbox ? "X" : (playStation ? "Square" : "Left");
		case 8:
			return xbox ? "Y" : (playStation ? "Triangle" : "Top");
		case 10:
			return xbox ? "LB" : "L1";
		case 11:
			return xbox ? "LT" : "L2";
		case 12:
			return xbox ? "RB" : "R1";
		case 13:
			return xbox ? "RT" : "R2";
		case 14:
			return xbox ? "View" : (playStation ? "Share" : "Select");
		case 15:
			return xbox ? "Menu" : (playStation ? "Options" : "Start");
		case 17:
			return xbox ? "LS" : "L3";
		case 18:
			return xbox ? "RS" : "R3";
		case 19:
			return "D-pad up";
		case 20:
			return "D-pad right";
		case 21:
			return "D-pad down";
		case 22:
			return "D-pad left";
		default:
			return null;
		}
	}

	private static void Refresh()
	{
		if (checkedFrame == Time.frameCount && cached)
		{
			return;
		}
		checkedFrame = Time.frameCount;
		bool ready = ReInput.isReady;
		ControlModeManager manager = ControlModeManager.HasInstance ? ControlModeManager.Instance : null;
		Player player = (ready && manager != null) ? manager.Player : null;
		Joystick joystick = CurrentJoystick(player);
		bool connected = joystick != null && joystick.isConnected;
		if (!cached || cachedReady != ready || cachedPlayer != player || cachedJoystick != joystick || cachedConnected != connected)
		{
			cachedPlayer = player;
			cached = true;
			cachedReady = ready;
			cachedJoystick = joystick;
			cachedConnected = connected;
			Rebuild(player, joystick);
		}
	}

	private static void Rebuild(Player player, Joystick joystick)
	{
		version++;
		labels.Clear();
		family = Family.Neutral;
		defaults = false;
		if (player == null)
		{
			return;
		}
		if (joystick == null)
		{
			defaults = true;
			foreach (ActionElementMap element in ReInput.mapping.GetDefaultJoystickElements(GameplayCategory, 0))
			{
				AddLabel(element);
			}
			return;
		}
		family = Identify(joystick.hardwareName + " " + joystick.name);
		foreach (ControllerMap map in player.controllers.maps.GetMapsInCategory(joystick, GameplayCategory))
		{
			foreach (ActionElementMap element in map.AllMaps)
			{
				AddLabel(element);
			}
		}
	}

	private static void AddLabel(ActionElementMap element)
	{
		if (!labels.ContainsKey(element.actionId))
		{
			string name = ElementName(family, element.elementIdentifierId);
			if (name != null)
			{
				labels[element.actionId] = name;
			}
		}
	}
}

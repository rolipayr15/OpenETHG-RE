using System;
using Rewired;
using UnityEngine;

public static class NativeKeyboardMouseMaps
{
	public static class AxisIndex
	{
		public const int LookX = 0;

		public const int LookY = 1;

		public const int PointerX = 2;

		public const int PointerY = 3;

		public const int ScrollX = 4;

		public const int ScrollY = 5;
	}

	public static class ButtonIndex
	{
		public const int Left = 0;

		public const int Right = 1;

		public const int Middle = 2;

		public const int W = 3;

		public const int A = 4;

		public const int S = 5;

		public const int D = 6;

		public const int E = 7;

		public const int Shift = 8;

		public const int Q = 9;

		public const int M = 10;

		public const int J = 11;

		public const int G = 12;

		public const int Escape = 13;

		public const int Enter = 14;

		public const int Up = 15;

		public const int Down = 16;

		public const int LeftArrow = 17;

		public const int RightArrow = 18;

		public const int Space = 19;

		public const int K = 20;

		public const int FirstKey = 3;
	}

	public sealed class Binding : IDisposable
	{
		private Player owner;

		public CustomController Controller { get; private set; }

		public bool IsCurrent => owner != null && Controller != null && ReInput.isReady && owner == ReInput.players.GetPlayer(0) && Controller == ReInput.controllers.GetCustomController(Controller.id);

		internal Binding(Player player, CustomController controller)
		{
			owner = player;
			Controller = controller;
		}

		public void Reset()
		{
			if (!IsCurrent)
			{
				return;
			}
			for (int i = 0; i < AxisCount; i++)
			{
				Controller.ClearAxisValue(i);
			}
			for (int i = 0; i < ButtonCount; i++)
			{
				Controller.ClearButtonValue(i);
			}
		}

		public void Dispose()
		{
			if (IsCurrent)
			{
				Reset();
				foreach (int category in Categories)
				{
					owner.controllers.maps.RemoveMap<CustomControllerMap>(Controller.id, category, LayoutId);
				}
				owner.controllers.RemoveController<CustomController>(Controller.id);
				ReInput.controllers.DestroyCustomController(Controller);
			}
			owner = null;
			Controller = null;
		}
	}

	public const string DefinitionName = "EyesNativeKeyboardMouse";

	public const string DefinitionGuid = "99685f03-925a-41c6-b43c-434d4cd201f8";

	public const int LayoutId = 0;

	public const int AxisCount = 6;

	public const int ButtonCount = 21;

	private static readonly int[] Categories = new int[4] { 3, 4, 6, 5 };

	public static int ButtonForKey(KeyCode key)
	{
		switch (key)
		{
		case KeyCode.W:
			return ButtonIndex.W;
		case KeyCode.A:
			return ButtonIndex.A;
		case KeyCode.S:
			return ButtonIndex.S;
		case KeyCode.D:
			return ButtonIndex.D;
		case KeyCode.E:
			return ButtonIndex.E;
		case KeyCode.LeftShift:
			return ButtonIndex.Shift;
		case KeyCode.Q:
			return ButtonIndex.Q;
		case KeyCode.M:
			return ButtonIndex.M;
		case KeyCode.J:
			return ButtonIndex.J;
		case KeyCode.G:
			return ButtonIndex.G;
		case KeyCode.Escape:
			return ButtonIndex.Escape;
		case KeyCode.Return:
		case KeyCode.KeypadEnter:
			return ButtonIndex.Enter;
		case KeyCode.UpArrow:
			return ButtonIndex.Up;
		case KeyCode.DownArrow:
			return ButtonIndex.Down;
		case KeyCode.LeftArrow:
			return ButtonIndex.LeftArrow;
		case KeyCode.RightArrow:
			return ButtonIndex.RightArrow;
		case KeyCode.Space:
			return ButtonIndex.Space;
		case KeyCode.K:
			return ButtonIndex.K;
		default:
			return -1;
		}
	}

	public static Binding Create(Player player, string tag)
	{
		if (!ReInput.isReady)
		{
			throw new InvalidOperationException("Rewired must be initialized first.");
		}
		if (player == null || player.id != 0 || player.name != "Player0" || ReInput.players.GetPlayer(0) != player || ReInput.players.GetPlayer("Player0") != player)
		{
			throw new ArgumentException("Native KBM requires the audited runtime Player0.", "player");
		}
		if (string.IsNullOrEmpty(tag))
		{
			throw new ArgumentException("A backend tag is required.", "tag");
		}
		InputManager manager = UnityEngine.Object.FindFirstObjectByType<InputManager>();
		Rewired.Data.CustomController_Editor definition = manager != null ? manager.userData.GetCustomController(DefinitionName) : null;
		if (definition == null || definition.id == 0 || definition.typeGuid != new Guid(DefinitionGuid))
		{
			throw new InvalidOperationException("The Eyes native KBM definition is missing or incompatible.");
		}
		CustomController controller = ReInput.controllers.CreateCustomController(definition.id, tag);
		Binding binding = new Binding(player, controller);
		if (controller == null || controller.axisCount != AxisCount || controller.buttonCount != ButtonCount || controller.hardwareTypeGuid != new Guid(DefinitionGuid))
		{
			throw new InvalidOperationException("The Eyes native KBM element contract is incompatible.");
		}
		player.controllers.AddController<CustomController>(controller.id, false);
		player.controllers.maps.RemoveMap<CustomControllerMap>(controller.id, 0, LayoutId);
		foreach (int category in Categories)
		{
			player.controllers.maps.LoadMap<CustomControllerMap>(controller.id, category, LayoutId);
			CustomControllerMap map = player.controllers.maps.GetMap<CustomControllerMap>(controller.id, category, LayoutId);
			if (map == null)
			{
				throw new InvalidOperationException("Missing native KBM map for category " + category);
			}
			map.enabled = false;
		}
		binding.Reset();
		return binding;
	}
}

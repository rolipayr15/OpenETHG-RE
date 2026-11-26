using System;
using System.Collections.Generic;
using System.Text;
using Rewired;
using UnityEngine;

// Central input arbitration: base context (gameplay / main menu) plus stacked blocking leases.
public sealed class GameplayInputContext
{
	public enum Kind
	{
		Gameplay = 0,
		MainMenu = 1,
		Vision = 2,
		Map = 3,
		Journal = 4,
		Pause = 5,
		TextEntry = 6,
		Modal = 7,
		Minigame = 8,
		GameBlock = 9,
		Death = 10,
		Transition = 11,
		FocusLost = 12,
		DeathWindow = 13
	}

	[Flags]
	public enum WindowInput
	{
		PointerOnly = 0,
		Cancel = 1,
		Navigate = 2,
		Submit = 4
	}

	public enum ResetReason
	{
		ContextChanged = 0,
		ControlModeChanged = 1,
		DeviceDisconnected = 2,
		FocusChanged = 3,
		SceneChanged = 4,
		CaptureChanged = 5
	}

	public enum CancelResult
	{
		None = 0,
		Blocked = 1,
		OpenPause = 2,
		CloseOwner = 3,
		MainMenuBack = 4
	}

	// A blocking input owner (window, map, pause...). The topmost active lease decides what input is allowed.
	public sealed class Lease : IDisposable
	{
		internal GameplayInputContext Policy;

		internal readonly object Owner;

		internal readonly Func<bool> ReadActive;

		internal readonly WindowInput Window;

		private readonly Action onCancel;

		internal bool Active;

		internal long Order;

		public Kind Context { get; private set; }

		public bool RetainsOrder { get; set; }

		public bool IsDisposed => Policy == null;

		internal Lease(GameplayInputContext policy, object owner, Kind kind, Func<bool> active, WindowInput window, Action onCancel)
		{
			Policy = policy;
			Owner = owner;
			Context = kind;
			ReadActive = active;
			this.onCancel = onCancel;
			Window = window;
		}

		public bool TryHandleCancel()
		{
			if (Policy == null || onCancel == null)
			{
				return false;
			}
			onCancel();
			return true;
		}

		public void Dispose()
		{
			GameplayInputContext policy = Policy;
			if (policy == null)
			{
				return;
			}
			Policy = null;
			policy.leases.Remove(this);
			if (Active)
			{
				policy.Invalidate(ResetReason.ContextChanged);
			}
		}
	}

	public readonly struct Permissions
	{
		public readonly Kind Context;

		public readonly Lease TopOwner;

		public readonly int MapCategory;

		public readonly bool Move;

		public readonly bool Look;

		public readonly bool StartActions;

		public readonly bool RequiresPointer;

		public readonly bool PointerUI;

		public readonly bool NavigateUI;

		public readonly bool SubmitUI;

		public readonly bool TransitionBarrier;

		public readonly uint Revision;

		internal Permissions(Kind context, Lease owner, int category, bool pointer, bool navigate, bool submit, bool barrier, uint revision)
		{
			Context = context;
			TopOwner = owner;
			MapCategory = category;
			TransitionBarrier = barrier;
			Revision = revision;
			bool gameplay = context == Kind.Gameplay && !barrier;
			Move = gameplay;
			Look = gameplay;
			StartActions = gameplay;
			RequiresPointer = pointer;
			PointerUI = pointer && !barrier;
			NavigateUI = navigate && !barrier;
			SubmitUI = submit && !barrier;
		}
	}

	private static GameplayInputContext shared;

	// Rewired action ids whose physical release is tracked (an action only fires after its button was released
	// once since the last context change, so a press that closed one window never leaks into the next one).
	private static readonly int[] Buttons = new int[22]
	{
		9, 8, 32, 31, 50, 51, 52, 11, 39, 38, 47, 48, 33, 34, 45, 46, 41, 42, 43, 44, 37, 36
	};

	private readonly List<Lease> leases = new List<Lease>();

	private readonly Func<int> frame;

	private readonly int[] releasedFrame = new int[Buttons.Length];

	private readonly bool[] held = new bool[Buttons.Length];

	private readonly List<ControllerMap> maps = new List<ControllerMap>();

	private readonly List<ControllerMap> sampledMaps = new List<ControllerMap>();

	private readonly HashSet<int> consumedActions = new HashSet<int>();

	private Kind baseContext;

	private ControlMode controlMode;

	private long order;

	private uint revision;

	private int barrierFrame;

	private int consumptionFrame = -1;

	private int cancelFrame = -1;

	private bool synchronizing;

	private const float NavigationAxisHeldThreshold = 0.5f;

	public static GameplayInputContext Shared
	{
		get
		{
			if (shared == null)
			{
				shared = new GameplayInputContext(Kind.MainMenu);
			}
			return shared;
		}
	}

	public Kind BaseContext => baseContext;

	public Permissions Current
	{
		get
		{
			Synchronize();
			Lease top = null;
			foreach (Lease lease in leases)
			{
				if (!lease.Active)
				{
					continue;
				}
				if (top == null)
				{
					top = lease;
					continue;
				}
				int priority = Priority(lease.Context);
				int topPriority = Priority(top.Context);
				if (priority > topPriority || (priority == topPriority && lease.Order > top.Order))
				{
					top = lease;
				}
			}
			Kind kind = top != null ? top.Context : baseContext;
			int category;
			bool pointer;
			bool navigate = false;
			bool submit = false;
			switch (kind)
			{
			case Kind.Gameplay:
				category = 4;
				pointer = false;
				break;
			case Kind.MainMenu:
				category = 3;
				pointer = true;
				navigate = true;
				submit = true;
				break;
			case Kind.Vision:
				category = 4;
				pointer = controlMode != ControlMode.KeyboardMouse;
				break;
			case Kind.Map:
				category = 6;
				pointer = controlMode != ControlMode.KeyboardMouse;
				break;
			case Kind.Journal:
			case Kind.Pause:
				category = 6;
				pointer = true;
				navigate = true;
				submit = true;
				break;
			case Kind.TextEntry:
			case Kind.Modal:
			case Kind.Minigame:
			case Kind.DeathWindow:
			{
				WindowInput window = top.Window;
				submit = (window & WindowInput.Submit) != 0;
				navigate = (window & WindowInput.Navigate) != 0;
				pointer = true;
				if (kind != Kind.TextEntry && navigate)
				{
					category = 6;
				}
				else
				{
					category = window == WindowInput.PointerOnly || kind == Kind.TextEntry ? -1 : 5;
				}
				break;
			}
			default:
				category = -1;
				pointer = false;
				break;
			}
			return new Permissions(kind, top, category, pointer, navigate, submit, frame() <= barrierFrame, revision);
		}
	}

	public event Action<ResetReason> InputResetRequested;

	public event Action Changed;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetShared()
	{
		shared = null;
	}

	public GameplayInputContext(Kind initialContext = Kind.Gameplay, Func<int> frameNumber = null)
	{
		if (initialContext > Kind.MainMenu)
		{
			throw new ArgumentException("Base context must be Gameplay or MainMenu.", "initialContext");
		}
		baseContext = initialContext;
		frame = frameNumber ?? (() => Time.frameCount);
		barrierFrame = frame();
		ClearReleaseState();
	}

	public bool IsGameplayExcept(Lease ignoredOwner)
	{
		Synchronize();
		if (ignoredOwner == null || baseContext != Kind.Gameplay || ignoredOwner.Policy != this)
		{
			return false;
		}
		foreach (Lease lease in leases)
		{
			if (lease != ignoredOwner && lease.Active)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsGameplayExcept(Func<Lease, bool> acceptedOwner)
	{
		if (acceptedOwner == null)
		{
			throw new ArgumentNullException("acceptedOwner");
		}
		Synchronize();
		if (baseContext != Kind.Gameplay)
		{
			return false;
		}
		foreach (Lease lease in leases)
		{
			if (lease.Active && !acceptedOwner(lease))
			{
				return false;
			}
		}
		return true;
	}

	public string DescribeActiveLeases()
	{
		StringBuilder builder = new StringBuilder();
		foreach (Lease lease in leases)
		{
			if (!lease.Active)
			{
				continue;
			}
			if (builder.Length != 0)
			{
				builder.Append(',');
			}
			builder.Append(lease.Context).Append(':').Append(lease.Owner != null ? lease.Owner.GetType().Name : "none");
		}
		return builder.Length == 0 ? "none" : builder.ToString();
	}

	public bool HasActive(Kind context)
	{
		Synchronize();
		foreach (Lease lease in leases)
		{
			if (lease.Active && lease.Context == context)
			{
				return true;
			}
		}
		return false;
	}

	// The "open text entry" press (action 38) is only accepted during free gameplay.
	public bool TryConsumeTextEntryOpen(bool pressed)
	{
		Permissions state = Current;
		if (!pressed || !state.StartActions || !Ready(38, state))
		{
			return false;
		}
		Consume(38);
		return true;
	}

	public void SetControlMode(ControlMode mode)
	{
		if (controlMode == mode)
		{
			return;
		}
		controlMode = mode;
		Invalidate(ResetReason.ControlModeChanged);
	}

	public void SetBaseContext(Kind context)
	{
		if (context > Kind.MainMenu)
		{
			throw new ArgumentException("Use a lease for a blocking context.", "context");
		}
		if (baseContext == context)
		{
			return;
		}
		baseContext = context;
		Invalidate(ResetReason.SceneChanged);
	}

	public Lease Acquire(object owner, Kind context, WindowInput window = WindowInput.PointerOnly, Action onCancel = null)
	{
		return Register(owner, context, () => true, window, onCancel);
	}

	public Lease Register(object owner, Kind context, Func<bool> active, WindowInput window = WindowInput.PointerOnly, Action onCancel = null)
	{
		if (owner == null || (owner is UnityEngine.Object unityOwner && unityOwner == null))
		{
			throw new ArgumentNullException("owner");
		}
		if (active == null)
		{
			throw new ArgumentNullException("active");
		}
		if (context <= Kind.MainMenu)
		{
			throw new ArgumentException("Use SetBaseContext for a base context.", "context");
		}
		foreach (Lease existing in leases)
		{
			if (existing.Owner == owner && existing.Context == context)
			{
				throw new InvalidOperationException("Owner already registered this context.");
			}
		}
		Lease lease = new Lease(this, owner, context, active, window, onCancel);
		leases.Add(lease);
		Synchronize();
		return lease;
	}

	// Drops leases whose Unity owner was destroyed and re-reads every lease's active state.
	public void Synchronize()
	{
		if (synchronizing)
		{
			return;
		}
		synchronizing = true;
		bool changed = false;
		try
		{
			for (int i = 0; i < leases.Count; i++)
			{
				Lease lease = leases[i];
				if (lease.Owner is UnityEngine.Object unityOwner && unityOwner == null)
				{
					lease.Policy = null;
					changed |= lease.Active;
					leases.RemoveAt(i);
					i--;
					continue;
				}
				bool active = lease.ReadActive();
				if (lease.Active == active)
				{
					continue;
				}
				lease.Active = active;
				if (active && (lease.Order == 0 || !lease.RetainsOrder))
				{
					lease.Order = ++order;
				}
				changed = true;
			}
		}
		finally
		{
			synchronizing = false;
		}
		if (changed)
		{
			Invalidate(ResetReason.ContextChanged);
		}
	}

	public Vector2 FilterMove(Vector2 value)
	{
		return Current.Move ? value : Vector2.zero;
	}

	public Vector2 FilterLook(Vector2 value)
	{
		return Current.Look ? value : Vector2.zero;
	}

	public void ResetInput(ResetReason reason)
	{
		Invalidate(reason);
	}

	private void Invalidate(ResetReason reason)
	{
		revision++;
		barrierFrame = frame();
		ClearReleaseState();
		InputResetRequested?.Invoke(reason);
		Changed?.Invoke();
	}

	private void ClearReleaseState()
	{
		for (int i = 0; i < releasedFrame.Length; i++)
		{
			releasedFrame[i] = -1;
		}
	}

	// Records the first frame (after the last barrier) on which each tracked button was seen released.
	public void ObserveButtons(Func<int, bool> isPhysicallyHeld)
	{
		if (isPhysicallyHeld == null)
		{
			throw new ArgumentNullException("isPhysicallyHeld");
		}
		Synchronize();
		int now = frame();
		if (now <= barrierFrame)
		{
			return;
		}
		for (int i = 0; i < Buttons.Length; i++)
		{
			if (releasedFrame[i] < 0 && !isPhysicallyHeld(Buttons[i]))
			{
				releasedFrame[i] = now;
			}
		}
	}

	// Samples the raw physical state of every element bound to a tracked action (ignoring map enable state,
	// which this class itself toggles) and feeds it to ObserveButtons.
	public void ObserveRewiredButtons(Player player, Func<ControllerMap, bool> sourceAllowed)
	{
		if (sourceAllowed == null)
		{
			throw new ArgumentNullException("sourceAllowed");
		}
		if (!ReInput.isReady || player == null)
		{
			return;
		}
		Synchronize();
		Array.Clear(held, 0, held.Length);
		sampledMaps.Clear();
		player.controllers.maps.GetAllMaps(sampledMaps);
		foreach (ControllerMap map in sampledMaps)
		{
			if (!sourceAllowed(map) || map.controller == null)
			{
				continue;
			}
			foreach (ActionElementMap element in map.AllMaps)
			{
				int index = Array.IndexOf(Buttons, element.actionId);
				if (index < 0)
				{
					continue;
				}
				Controller controller = map.controller;
				if (controller is Keyboard keyboard)
				{
					int button = keyboard.GetButtonIndexByKeyCode(element.keyCode);
					held[index] |= button >= 0 && keyboard.GetButton(button);
				}
				else if (element.elementType == ControllerElementType.Button)
				{
					held[index] |= controller.GetButtonById(element.elementIdentifierId);
				}
				else if (controller is ControllerWithAxes axes)
				{
					float value;
					float threshold;
					if ((element.actionId & ~1) == 36)
					{
						value = axes.GetAxisById(element.elementIdentifierId);
						threshold = NavigationAxisHeldThreshold;
					}
					else
					{
						value = axes.GetAxisRawById(element.elementIdentifierId);
						threshold = 0.1f;
					}
					bool pushed;
					switch (element.axisRange)
					{
					case AxisRange.Positive:
						pushed = value > threshold;
						break;
					case AxisRange.Negative:
						pushed = value < -threshold;
						break;
					default:
						pushed = Mathf.Abs(value) > threshold;
						break;
					}
					held[index] |= pushed;
				}
			}
		}
		ObserveButtons((int action) => held[Array.IndexOf(Buttons, action)]);
	}

	// Enables only the controller maps of the current category and, inside them, only the actions the
	// current context allows.
	public bool ApplyRewiredMaps(Player player, Func<ControllerMap, bool> sourceAllowed)
	{
		if (sourceAllowed == null)
		{
			throw new ArgumentNullException("sourceAllowed");
		}
		if (!ReInput.isReady || player == null)
		{
			return false;
		}
		Permissions state = Current;
		maps.Clear();
		player.controllers.maps.GetAllMaps(maps);
		foreach (ControllerMap map in maps)
		{
			bool enable = state.MapCategory >= 0 && map.categoryId == state.MapCategory && sourceAllowed(map);
			foreach (ActionElementMap element in map.AllMaps)
			{
				element.enabled = enable && AllowsMappedAction(state, element.actionId);
			}
			if (map.enabled != enable)
			{
				map.enabled = enable;
			}
		}
		return true;
	}

	public bool CanClose(Lease owner, int actionId)
	{
		Permissions state = Current;
		return owner != null && !state.TransitionBarrier && owner.Policy == this && state.TopOwner == owner && ClosingAction(owner, actionId);
	}

	public bool TryConsumeAction(int actionId, bool pressed, Lease owner = null)
	{
		Permissions state = Current;
		if (!pressed || IsCancel(actionId) || !Ready(actionId, state) || !AllowsAction(state, actionId, owner))
		{
			return false;
		}
		Consume(actionId);
		return true;
	}

	private static bool AllowsAction(Permissions state, int actionId, Lease owner)
	{
		if (owner != null && state.TopOwner == owner && ClosingAction(owner, actionId))
		{
			return true;
		}
		if (state.StartActions && IsGameplayAction(actionId))
		{
			return true;
		}
		if (state.Context == Kind.MainMenu && IsMainMenuAction(actionId))
		{
			return true;
		}
		if (!state.SubmitUI || (state.TopOwner != null && state.TopOwner != owner))
		{
			return false;
		}
		switch (state.Context)
		{
		case Kind.Modal:
		case Kind.Minigame:
		case Kind.DeathWindow:
			return actionId == (state.MapCategory == 6 ? 38 : 47);
		case Kind.Journal:
		case Kind.Pause:
			return actionId == 38;
		default:
			return actionId == 38 && state.Context == Kind.TextEntry;
		}
	}

	public bool IsMappedActionEnabled(int action)
	{
		return AllowsMappedAction(Current, action);
	}

	private bool AllowsMappedAction(Permissions state, int action)
	{
		if (state.TransitionBarrier)
		{
			return false;
		}
		switch (action)
		{
		case 16:
		case 17:
			return state.Move;
		case 18:
		case 19:
			return state.Look;
		case 36:
		case 37:
			return state.NavigateUI && Armed(action);
		}
		if (IsCancel(action))
		{
			if (!CancelArmed())
			{
				return false;
			}
			if (state.Context < Kind.Journal)
			{
				return true;
			}
			return state.TopOwner != null && ClosingAction(state.TopOwner, action);
		}
		return Armed(action) && AllowsAction(state, action, state.TopOwner);
	}

	// Cancel (Back/Escape/B): opens pause in gameplay, goes back in the main menu, or closes the top owner if
	// that owner accepts cancel. Only one cancel is consumed per frame.
	public CancelResult TryConsumeCancel(bool pressed, out Lease owner)
	{
		Permissions state = Current;
		owner = null;
		if (!pressed || state.TransitionBarrier)
		{
			return CancelResult.None;
		}
		int now = frame();
		if (cancelFrame == now || !CancelArmed())
		{
			return CancelResult.None;
		}
		cancelFrame = now;
		switch (state.Context)
		{
		case Kind.Gameplay:
		case Kind.Vision:
		case Kind.Map:
			return CancelResult.OpenPause;
		case Kind.MainMenu:
			return CancelResult.MainMenuBack;
		}
		if (state.TopOwner != null && ClosingAction(state.TopOwner, 39))
		{
			owner = state.TopOwner;
			return CancelResult.CloseOwner;
		}
		return CancelResult.Blocked;
	}

	public bool TryConsumePointer(Lease owner = null)
	{
		Permissions state = Current;
		if (!state.PointerUI || state.TopOwner != owner || !Ready(9, state))
		{
			return false;
		}
		Consume(9);
		return true;
	}

	private bool Ready(int action, Permissions state)
	{
		if (state.TransitionBarrier)
		{
			return false;
		}
		int now = frame();
		if (cancelFrame == now)
		{
			return false;
		}
		if (consumptionFrame == now && consumedActions.Contains(ConsumptionKey(action)))
		{
			return false;
		}
		return Armed(action);
	}

	private bool CancelArmed()
	{
		return Armed(11) && Armed(39) && Armed(48) && Armed(34);
	}

	private void Consume(int action)
	{
		int now = frame();
		if (consumptionFrame != now)
		{
			consumedActions.Clear();
			consumptionFrame = now;
		}
		consumedActions.Add(ConsumptionKey(action));
	}

	// Submit-like actions share one key so a single press cannot fire two of them in one frame.
	private static int ConsumptionKey(int action)
	{
		if (action == 33 || action == 47)
		{
			return 38;
		}
		if (action == 46)
		{
			return 45;
		}
		return action;
	}

	private bool Armed(int action)
	{
		int index = Array.IndexOf(Buttons, action);
		if (index < 0 || releasedFrame[index] < 0)
		{
			return false;
		}
		return releasedFrame[index] < frame();
	}

	private static bool ClosingAction(Lease owner, int action)
	{
		if (IsCancel(action))
		{
			if (owner == null)
			{
				return false;
			}
			switch (owner.Context)
			{
			case Kind.Modal:
			case Kind.Minigame:
			case Kind.DeathWindow:
			case Kind.TextEntry:
				return (owner.Window & WindowInput.Cancel) != 0;
			case Kind.Journal:
			case Kind.Pause:
				return true;
			default:
				return false;
			}
		}
		if (owner == null)
		{
			return false;
		}
		return (action == 31 && owner.Context == Kind.Map) || (action == 50 && owner.Context == Kind.Journal) || (action == 32 && owner.Context == Kind.Vision);
	}

	private static bool IsCancel(int action)
	{
		return action >= 0 && action < 49 && ((0x1008400000800L >> action) & 1) != 0;
	}

	private static bool IsGameplayAction(int action)
	{
		return action >= 0 && action < 53 && ((0x1C000180000300L >> action) & 1) != 0;
	}

	private static bool IsMainMenuAction(int action)
	{
		return action >= 0 && action < 47 && ((0x7E0200000000L >> action) & 1) != 0;
	}

	private static int Priority(Kind kind)
	{
		switch (kind)
		{
		case Kind.Vision:
			return 40;
		case Kind.Map:
		case Kind.Journal:
			return 50;
		case Kind.Pause:
			return 60;
		case Kind.TextEntry:
		case Kind.Modal:
		case Kind.Minigame:
			return 70;
		case Kind.GameBlock:
		case Kind.Transition:
			return 90;
		case Kind.Death:
			return 88;
		case Kind.FocusLost:
			return 100;
		case Kind.DeathWindow:
			return 89;
		default:
			return 0;
		}
	}
}

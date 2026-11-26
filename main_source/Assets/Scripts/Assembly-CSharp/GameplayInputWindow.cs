using System;
using UnityEngine;

// Gives a UI window an input-context lease (and optionally a world pause) for as long as it is open/enabled.
[DisallowMultipleComponent]
public sealed class GameplayInputWindow : MonoBehaviour
{
	private GameplayInputContext.Lease lease;

	private IDisposable pause;

	public GameplayInputContext.Lease Lease => lease;

	public static GameplayInputWindow GetOrAdd(Component window)
	{
		GameplayInputWindow existing = window.GetComponent<GameplayInputWindow>();
		if (existing == null)
		{
			existing = window.gameObject.AddComponent<GameplayInputWindow>();
		}
		return existing;
	}

	public void Open(GameplayInputContext.Kind kind = GameplayInputContext.Kind.Modal, GameplayInputContext.WindowInput input = GameplayInputContext.WindowInput.PointerOnly, Action onCancel = null, bool pauseWorld = false)
	{
		if (!isActiveAndEnabled)
		{
			return;
		}
		if (lease == null || lease.Policy == null)
		{
			lease = GameplayInputContext.Shared.Acquire(this, kind, input, onCancel);
		}
		if (pauseWorld && pause == null && CustomSingleton<PauseManager>.HasInstance)
		{
			pause = CustomSingleton<PauseManager>.Instance.AcquirePause(this);
		}
	}

	public void Close()
	{
		lease?.Dispose();
		lease = null;
		pause?.Dispose();
		pause = null;
	}

	private void OnDisable()
	{
		Close();
	}

	private void OnDestroy()
	{
		Close();
	}
}

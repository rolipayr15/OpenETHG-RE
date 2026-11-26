using System;

public sealed class PushNotificationPermissionPolicy
{
	private readonly object gate;

	private readonly Func<PushPermissionDecision, bool> save;

	private PushPermissionDecision decision;

	private bool persistenceUnavailable;

	private bool saving;

	private bool acceptedInCurrentFlow;

	private int flowRevision;

	private int? gameScene;

	private bool gameFinished;

	private bool menuOpportunity;

	public PushPermissionDecision Decision => PushPermissionDecision.Unseen;

	public PushNotificationPermissionPolicy(PushPermissionDecision savedDecision, Func<PushPermissionDecision, bool> save)
	{
	}

	public void BeginGame(int sceneHandle)
	{
	}

	public void FinishGame(int sceneHandle, bool qualifies)
	{
	}

	public void SceneDestroyed(int sceneHandle)
	{
	}

	public void Cancel()
	{
	}

	private void ClearGame()
	{
	}

	public bool TakeMenuOpportunity()
	{
		return false;
	}

	public bool CanAsk(PushPermissionStatus status)
	{
		return false;
	}

	public bool MarkShown()
	{
		return false;
	}

	public void Decline()
	{
	}

	public bool Accept()
	{
		return false;
	}

	public bool TryRequest(PushPermissionStatus status, bool contextValid)
	{
		return false;
	}

	private bool SaveDecision(PushPermissionDecision next)
	{
		return false;
	}
}

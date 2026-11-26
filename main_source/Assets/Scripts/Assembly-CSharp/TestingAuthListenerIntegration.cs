using Reflex.Attributes;
using UnityEngine;

public class TestingAuthListenerIntegration : MonoBehaviour
{
	[Inject]
	private IPlayerContextManager playerContextManager;

	private string currentPlayerIdPrefix;

	private static TestingAuthListenerIntegration instance;

	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void OnPlayerChanged(string oldPlayerId, string newPlayerId)
	{
	}

	private void UpdatePlayerIdPrefix()
	{
	}

	private Texture2D MakeTexture(int width, int height, Color color)
	{
		return null;
	}

	private void OnGUI()
	{
	}

	private void OnDestroy()
	{
	}
}

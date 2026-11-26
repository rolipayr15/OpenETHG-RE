using Characters.Agents;
using UnityEngine;

public class EnemyManager : CustomSingleton<EnemyManager>
{
	private EnemyAgent currentMonster;

	[SerializeField]
	private GameObject krasue;

	[SerializeField]
	private GameObject witch;

	[SerializeField]
	private GameObject charlie;

	[SerializeField]
	private GameObject dog;

	[Tooltip("Goose charger; activated for Enemy.Goose.")]
	[SerializeField]
	private GameObject goose;

	public EnemyAgent CurrentEnemy => currentMonster != null ? currentMonster : (currentMonster = FindObjectOfType<EnemyAgent>());

	private void Awake()
	{
		if (!UserGameSettings.IsMultiplayer() || UserGameSettings.isMultiplayerHost)
		{
			ActivateFor(UserGameSettings.currentEnemy);
		}
	}

	private void ActivateFor(Enemy enemy)
	{
		switch ((int)enemy)
		{
		case 0:
		case 3:
		case 4:
			krasue.SetActive(true);
			break;
		case 1:
			charlie.SetActive(true);
			break;
		case 2:
			dog.SetActive(true);
			break;
		case 5:
			krasue.SetActive(true);
			charlie.SetActive(true);
			break;
		case 6:
			witch.SetActive(true);
			break;
		case 7:
			goose.SetActive(true);
			break;
		}
	}

	public void ActivateEnemyWhenHostLeftGame()
	{
		((int)UserGameSettings.currentEnemy == 7 ? goose : krasue).SetActive(true);
		CurrentEnemy.EnemyMover.MoveAgentAwayFromPlayer();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}

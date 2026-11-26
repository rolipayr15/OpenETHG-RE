using UnityEngine;

public class EnemyAvatarManager : CustomSingleton<EnemyAvatarManager>
{
	private GameObject currentMonsterAvatar;

	[SerializeField]
	private GameObject krasue;

	[SerializeField]
	private GameObject witch;

	[SerializeField]
	private GameObject charlie;

	[SerializeField]
	private GameObject dog;

	[SerializeField]
	private GameObject goose;

	private bool wasInitiated;

	public GameObject GetCurrentEnemy()
	{
		if (currentMonsterAvatar == null)
		{
			InitEnemy();
		}
		return currentMonsterAvatar;
	}

	private void InitEnemy()
	{
		switch ((int)UserGameSettings.currentEnemy)
		{
		case 1:
			currentMonsterAvatar = charlie;
			break;
		case 2:
			currentMonsterAvatar = dog;
			break;
		case 6:
			currentMonsterAvatar = witch;
			break;
		case 7:
			currentMonsterAvatar = goose != null ? goose : krasue;
			break;
		default:
			currentMonsterAvatar = krasue;
			break;
		}
		currentMonsterAvatar.SetActive(true);
	}

	private void Awake()
	{
		if (UserGameSettings.IsMultiplayer() && !UserGameSettings.isMultiplayerHost && !wasInitiated)
		{
			InitEnemy();
		}
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

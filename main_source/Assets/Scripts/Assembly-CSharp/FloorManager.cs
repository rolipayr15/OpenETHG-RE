using System.Collections.Generic;
using UnityEngine;

public class FloorManager : CustomSingleton<FloorManager>
{
	private Transform playerT;

	[SerializeField]
	private Transform charlieT;

	[SerializeField]
	private Transform krasueT;

	[SerializeField]
	private Transform witchT;

	[SerializeField]
	private Transform dogT;

	[SerializeField]
	private Transform gooseT;

	[SerializeField]
	private Transform enemyAvatarT;

	[SerializeField]
	private Transform avatarT;

	private int krasueFloor;

	private int witchFloor;

	private int charlieFloor;

	private int dogFloor;

	private int gooseFloor;

	private int playerFloor;

	private int avatarFloor;

	private bool isEnemyOnTheSameFloor;

	public bool IsEnemyOnTheSameFloor => isEnemyOnTheSameFloor;

	[SerializeField]
	private float floor1;

	[SerializeField]
	private float floor2;

	private List<MeshRenderer> stairs;

	private List<Collider> stairsCollider;

	private Transform currentEnemy;

	private float lastCheckTime;

	private float lastCheckIfTheSameFloor;

	public bool IsInitialized { get; private set; }

	public static bool TryGetInitializedSceneInstance(out FloorManager floorManager)
	{
		floorManager = Object.FindFirstObjectByType<FloorManager>(FindObjectsInactive.Include);
		return floorManager != null && floorManager.IsInitialized;
	}

	// Floors are horizontal bands split at the authored heights floor1 and floor2.
	public int GetFloor(Vector3 position)
	{
		if (position.y < floor1)
		{
			return 0;
		}
		return position.y < floor2 ? 1 : 2;
	}

	private void Start()
	{
		UserGameSettings.isEnemyOnTheSameFloor = isEnemyOnTheSameFloor;
		if (CustomSingleton<PlayerManager>.Instance != null && CustomSingleton<PlayerManager>.Instance.PlayerCamera != null)
		{
			playerT = CustomSingleton<PlayerManager>.Instance.PlayerCamera.transform;
		}
		// The hospital's stairs are invisible volumes, elsewhere they are the stair meshes themselves.
		if (UserGameSettings.currentLevel == Level.Hospital)
		{
			stairsCollider = new List<Collider>();
			foreach (GameObject stair in GameObject.FindGameObjectsWithTag("stairs"))
			{
				if (stair != null)
				{
					Collider stairCollider = stair.GetComponent<Collider>();
					if (stairCollider != null)
					{
						stairsCollider.Add(stairCollider);
					}
				}
			}
		}
		else
		{
			stairs = new List<MeshRenderer>();
			foreach (GameObject stair in GameObject.FindGameObjectsWithTag("stairs"))
			{
				if (stair != null)
				{
					MeshRenderer stairRenderer = stair.GetComponent<MeshRenderer>();
					if (stairRenderer != null)
					{
						stairs.Add(stairRenderer);
					}
				}
			}
		}
		if (UserGameSettings.IsMultiplayer() && !UserGameSettings.isMultiplayerHost)
		{
			GameObject avatar = CustomSingleton<EnemyAvatarManager>.Instance != null ? CustomSingleton<EnemyAvatarManager>.Instance.GetCurrentEnemy() : null;
			if (avatar != null)
			{
				avatarT = avatar.transform;
			}
		}
		switch (UserGameSettings.currentEnemy)
		{
		case Enemy.Krasue:
		case Enemy.Custom:
		case Enemy.Double:
			currentEnemy = krasueT;
			break;
		case Enemy.Charlie:
			currentEnemy = charlieT;
			break;
		case Enemy.Dog:
			currentEnemy = dogT;
			break;
		case Enemy.Witch:
			currentEnemy = witchT;
			break;
		case Enemy.Goose:
			currentEnemy = gooseT;
			break;
		}
		IsInitialized = true;
	}

	// Floors are re-evaluated once a second; the "same floor" flag drives enemy sounds and HUD warnings.
	private void Update()
	{
		if (UserGameSettings.currentEnemy == Enemy.Sandbox)
		{
			isEnemyOnTheSameFloor = false;
			return;
		}
		if (lastCheckTime + 1f < Time.time)
		{
			playerFloor = GetFloor(playerT.position);
			krasueFloor = GetFloor(krasueT.position);
			witchFloor = GetFloor(witchT.position);
			charlieFloor = GetFloor(charlieT.position);
			dogFloor = GetFloor(dogT.position);
			if (gooseT != null)
			{
				gooseFloor = GetFloor(gooseT.position);
			}
			avatarFloor = GetFloor(avatarT.position);
			lastCheckTime = Time.time;
		}
		if (Time.time <= lastCheckIfTheSameFloor + 1f)
		{
			return;
		}
		bool same;
		if (UserGameSettings.IsMultiplayer() && !UserGameSettings.isMultiplayerHost)
		{
			same = avatarFloor == playerFloor;
		}
		else
		{
			switch (UserGameSettings.currentEnemy)
			{
			case Enemy.Double:
				same = charlieFloor == playerFloor || krasueFloor == playerFloor;
				break;
			case Enemy.Krasue:
			case Enemy.Custom:
				same = krasueFloor == playerFloor;
				break;
			case Enemy.Witch:
				same = witchFloor == playerFloor;
				break;
			case Enemy.Charlie:
				same = charlieFloor == playerFloor;
				break;
			case Enemy.Dog:
				same = dogFloor == playerFloor;
				break;
			case Enemy.Goose:
				same = gooseFloor == playerFloor;
				break;
			default:
				same = false;
				break;
			}
		}
		isEnemyOnTheSameFloor = same;
		UserGameSettings.isEnemyOnTheSameFloor = same;
		lastCheckIfTheSameFloor = Time.time;
	}

	public bool TryGetPlayerPosition(out Vector3 position)
	{
		position = playerT != null ? playerT.position : Vector3.zero;
		return playerT != null;
	}

	public int GetPlayerFloor()
	{
		if (UserGameSettings.currentEnemy == Enemy.Sandbox)
		{
			return GetFloor(playerT.position);
		}
		return playerFloor;
	}

	public bool IsEnemyOnTheFloor()
	{
		return isEnemyOnTheSameFloor;
	}

	// Within 12 m of the player.
	public bool IsEnemyClose()
	{
		if (UserGameSettings.IsMultiplayer() && !UserGameSettings.isMultiplayerHost)
		{
			return IsNear(avatarT);
		}
		switch (UserGameSettings.currentEnemy)
		{
		case Enemy.Double:
			return IsNear(krasueT) || IsNear(charlieT);
		case Enemy.Krasue:
		case Enemy.Custom:
			return IsNear(krasueT);
		case Enemy.Charlie:
			return IsNear(charlieT);
		case Enemy.Witch:
			return IsNear(witchT);
		case Enemy.Dog:
			return IsNear(dogT);
		case Enemy.Goose:
			return gooseT != null && IsNear(gooseT);
		default:
			return false;
		}
	}

	private bool IsNear(Transform enemy)
	{
		return (enemy.position - playerT.position).sqrMagnitude < 144f;
	}

	private int GetCurrentEnemyFloor()
	{
		Transform enemy = UserGameSettings.IsMultiplayer() && !UserGameSettings.isMultiplayerHost ? avatarT : currentEnemy;
		return GetFloor(enemy.position);
	}

	public bool IsOnStairs(Vector3 position)
	{
		if (UserGameSettings.currentLevel == Level.Hospital)
		{
			if (stairsCollider == null)
			{
				return false;
			}
			foreach (Collider stair in stairsCollider)
			{
				if (stair != null && stair.bounds.Contains(position))
				{
					return true;
				}
			}
			return false;
		}
		if (stairs == null)
		{
			return false;
		}
		foreach (MeshRenderer stair in stairs)
		{
			if (stair != null && stair.bounds.Contains(position))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsEnemyOnStairs()
	{
		Transform enemy = UserGameSettings.IsMultiplayer() && !UserGameSettings.isMultiplayerHost ? avatarT : currentEnemy;
		return IsOnStairs(enemy.position);
	}

	// Stairs connect floors, so anyone on them counts as "close" to both.
	public bool AreEnemyAndGuestClose(Vector3 guestPosition)
	{
		if (currentEnemy == null && avatarT == null)
		{
			return false;
		}
		return GetFloor(guestPosition) == GetCurrentEnemyFloor() || IsOnStairs(guestPosition) || IsEnemyOnStairs();
	}

	protected override void OnDestroy()
	{
		IsInitialized = false;
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}

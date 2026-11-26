using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay.Eggs
{
	[DisallowMultipleComponent]
	public sealed class MansionGooseEggSpawns : MonoBehaviour
	{
		[HideInInspector]
		public EggPickup[] Upstairs;

		[HideInInspector]
		public EggPickup[] Middle;

		[HideInInspector]
		public EggPickup[] Basement;

		private bool initialized;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void RegisterSceneHook()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			InitializeScene(scene, UserGameSettings.currentEnemy);
		}

		public static MansionGooseEggSpawns InitializeScene(Scene scene, Enemy enemy)
		{
			if (!scene.IsValid() || !scene.isLoaded)
			{
				return null;
			}
			List<EggPickup> pickups = new List<EggPickup>();
			FloorManager floorManager = null;
			MansionGooseEggSpawns spawns = null;
			foreach (GameObject root in scene.GetRootGameObjects())
			{
				pickups.AddRange(root.GetComponentsInChildren<EggPickup>(true));
				if (floorManager == null)
				{
					floorManager = root.GetComponentInChildren<FloorManager>(true);
				}
				if (spawns == null)
				{
					spawns = root.GetComponentInChildren<MansionGooseEggSpawns>(true);
				}
			}
			if (floorManager == null || pickups.Count == 0)
			{
				return null;
			}
			if (spawns != null && spawns.initialized)
			{
				return spawns;
			}
			if (spawns == null)
			{
				GameObject holder = new GameObject("GoosePumpkinSelection");
				SceneManager.MoveGameObjectToScene(holder, scene);
				spawns = holder.AddComponent<MansionGooseEggSpawns>();
			}
			List<EggPickup> upstairs = new List<EggPickup>();
			List<EggPickup> middle = new List<EggPickup>();
			List<EggPickup> basement = new List<EggPickup>();
			foreach (EggPickup pickup in pickups)
			{
				SphereCollider sphere = pickup.GetComponent<SphereCollider>();
				Vector3 position = sphere != null ? pickup.transform.TransformPoint(sphere.center) : pickup.transform.position + Vector3.up * 0.01f;
				int floor = floorManager.GetFloor(position);
				if (floor == 1)
				{
					middle.Add(pickup);
				}
				else if (floor == 2)
				{
					upstairs.Add(pickup);
				}
				else
				{
					basement.Add(pickup);
				}
			}
			spawns.Upstairs = upstairs.ToArray();
			spawns.Middle = middle.ToArray();
			spawns.Basement = basement.ToArray();
			spawns.InitializeForEnemy(enemy);
			return spawns;
		}

		private void Awake()
		{
			DisableCandidates();
		}

		private void Start()
		{
			InitializeScene(gameObject.scene, UserGameSettings.currentEnemy);
		}

		public void InitializeForEnemy(Enemy enemy)
		{
			if (initialized)
			{
				return;
			}
			initialized = true;
			DisableCandidates();
			if ((int)enemy != 7)
			{
				return;
			}
			HashSet<EggPickup> seen = new HashSet<EggPickup>();
			List<EggPickup> upstairs = UniqueCandidates(Upstairs, seen);
			List<EggPickup> middle = UniqueCandidates(Middle, seen);
			List<EggPickup> basement = UniqueCandidates(Basement, seen);
			if (upstairs.Count < 3 || middle.Count < 1 || basement.Count < 2)
			{
				Debug.LogWarning("[MansionGooseEggSpawns] At least three upstairs, one middle-floor and two basement pumpkins are required, all distinct.", this);
				return;
			}
			EggPickup[] selected = new EggPickup[] { Draw(upstairs), Draw(upstairs), Draw(upstairs), Draw(middle), Draw(basement), Draw(basement) };
			int remainingTwo = 3;
			for (int i = 0; i < selected.Length; i++)
			{
				bool two = UnityEngine.Random.Range(0, selected.Length - i) < remainingTwo;
				selected[i].PrankLastsUntilRoundEnd = false;
				selected[i].OutcomeCategory = (EggEffectCategory)(two ? 2 : 1);
				if (two)
				{
					remainingTwo--;
				}
				selected[i].gameObject.SetActive(true);
			}
		}

		private void DisableCandidates()
		{
			Disable(Upstairs);
			Disable(Middle);
			Disable(Basement);
		}

		private static void Disable(EggPickup[] candidates)
		{
			if (candidates == null)
			{
				return;
			}
			foreach (EggPickup candidate in candidates)
			{
				if (candidate != null)
				{
					candidate.gameObject.SetActive(false);
				}
			}
		}

		private static List<EggPickup> UniqueCandidates(EggPickup[] candidates, HashSet<EggPickup> seen)
		{
			List<EggPickup> unique = new List<EggPickup>();
			if (candidates == null)
			{
				return unique;
			}
			foreach (EggPickup candidate in candidates)
			{
				if (candidate != null && seen.Add(candidate))
				{
					unique.Add(candidate);
				}
			}
			return unique;
		}

		private static EggPickup Draw(List<EggPickup> candidates)
		{
			int index = UnityEngine.Random.Range(0, candidates.Count);
			EggPickup item = candidates[index];
			candidates.RemoveAt(index);
			return item;
		}
	}
}

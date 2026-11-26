using System.Linq;
using UnityEngine;

namespace Eyes.DailyChallenges
{
	public static class DailyChallengeRuntime
	{
		private sealed class RunSettings
		{
			public readonly Level Level = UserGameSettings.currentLevel;

			public readonly Difficulty Difficulty = UserGameSettings.currentDifficulty;

			public readonly Mode Mode = UserGameSettings.currentMode;

			public readonly Enemy Enemy = UserGameSettings.currentEnemy;

			public readonly GameMode GameMode = UserGameSettings.currentGameMode;

			public readonly bool PlayedWithRandom = UserGameSettings.isPlayingWithRandom;
		}

		private static MyGameSettings owner;

		private static DailyChallengeAttempt attempt;

		private static RunSettings settings;

		private static DistanceTracker stepTracker;

		private static uint stepSessionRevision;

		public static DailyChallengeService Service { get; private set; }

		public static DailyChallengeCompletion LatestCompletion => Service?.LatestCompletion;

		public static void Configure(DailyChallengeService service)
		{
			Reset();
			Service = service;
			Application.quitting += OnApplicationQuitting;
		}

		public static DailyChallengeView Refresh()
		{
			return Service != null ? Service.Refresh() : DailyChallengeView.Unavailable;
		}

		public static void Start(MyGameSettings source)
		{
			if (source == null || Service == null || owner == source)
			{
				return;
			}
			owner = source;
			settings = new RunSettings();
			int scene = source.gameObject.scene.handle.GetHashCode();
			attempt = Service.StartAttempt(scene);
			stepTracker = FindStepTracker(source.gameObject.scene);
			stepSessionRevision = stepTracker != null ? stepTracker.SessionRevision : 0U;
		}

		public static void BagCollected(Component source)
		{
			if (Owns(source))
			{
				Service.AddBag(attempt);
			}
		}

		public static void PumpkinCaught(Component source)
		{
			if (Owns(source))
			{
				Service.AddPumpkinCatch(attempt);
			}
		}

		public static void GoodBoyFed(Component source)
		{
			if (Owns(source))
			{
				Service.AddGoodBoyFeed(attempt);
			}
		}

		public static void PositivePotionEffectApplied(Component source, PositivePotionEffect effect)
		{
			if (Owns(source) && settings != null && (int)settings.Level == 1)
			{
				Service.AddPositivePotionEffect(attempt, effect);
			}
		}

		public static void PuzzleSolved(Component source)
		{
			if (Owns(source) && settings != null && (int)settings.Level == 0 && (int)settings.Enemy != 4)
			{
				Service.AddPuzzleSolved(attempt);
			}
		}

		public static void MapFragmentDiscovered(Component source, int fragmentId)
		{
			if (Owns(source) && settings != null && (int)settings.Enemy != 4)
			{
				Service.AddMapFragment(attempt, settings.Level, fragmentId);
			}
		}

		public static void EyeUsed(Component source)
		{
			if (Owns(source))
			{
				Service.AddEyeUse(attempt);
			}
		}

		public static void ChaseStarted(Component source)
		{
			if (Owns(source))
			{
				Service.AddChase(attempt);
			}
		}

		public static void EyeCollected(Eye source)
		{
			if (!Owns(source) || attempt == null || !DailyChallengeCatalog.TryGet(attempt.QuestId, out DailyChallengeDefinition definition) || definition.Kind != DailyChallengeKind.CollectEyesBeforeBags)
			{
				return;
			}
			EyesManager manager = attempt.RemainingEyeIds == null ? FindEyesManager(source.gameObject.scene) : null;
			Service.AddEye(attempt, source.gameObject.GetInstanceID(), manager != null ? manager.ActivatedEyeIds : null);
		}

		public static void Finish(Component source, string endReason)
		{
			if (Owns(source) && settings != null)
			{
				Service.FinishAttempt(attempt, endReason, new DailyChallengeRunResult(false, settings.Level, settings.Difficulty, settings.Mode, settings.Enemy, settings.GameMode, false, null, null, false));
			}
		}

		public static void MultiplayerFinished(Component source, bool isWon, string endReason)
		{
			if (Owns(source) && settings != null)
			{
				bool multiplayer = (int)settings.GameMode - 1U < 2U;
				Service.FinishAttempt(attempt, endReason, new DailyChallengeRunResult(isWon && multiplayer, settings.Level, settings.Difficulty, settings.Mode, settings.Enemy, settings.GameMode, false, null, null, settings.PlayedWithRandom));
			}
		}

		public static void Win(Component source)
		{
			if (!Owns(source) || settings == null)
			{
				return;
			}
			int? steps = null;
			if (stepTracker != null && stepTracker.gameObject.scene.handle.GetHashCode() == attempt.SceneHandle && stepSessionRevision != 0 && stepTracker.SessionRevision == stepSessionRevision && stepTracker.TryGetCurrentGameSteps(out int counted))
			{
				steps = counted;
			}
			Service.FinishAttempt(attempt, "exit_door", new DailyChallengeRunResult((int)settings.GameMode == 0, settings.Level, settings.Difficulty, settings.Mode, settings.Enemy, settings.GameMode, DoorUseTracker.IsRunEligible, steps, Time.timeSinceLevelLoad, false));
		}

		private static EyesManager FindEyesManager(UnityEngine.SceneManagement.Scene scene)
		{
			return Object.FindObjectsByType<EyesManager>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault((EyesManager manager) => manager.gameObject.scene == scene);
		}

		private static DistanceTracker FindStepTracker(UnityEngine.SceneManagement.Scene scene)
		{
			return Object.FindObjectsByType<DistanceTracker>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault((DistanceTracker tracker) => tracker.gameObject.scene == scene);
		}

		public static void SceneDestroyed(MyGameSettings source)
		{
			if (owner == source && Owns(source))
			{
				Finish(source, "scene_unloaded");
				owner = null;
				attempt = null;
				settings = null;
				stepTracker = null;
				stepSessionRevision = 0U;
			}
		}

		public static void ApplicationQuit(MyGameSettings source)
		{
			if (owner == source)
			{
				Finish(source, "application_quit");
			}
		}

		private static bool Owns(Component source)
		{
			return Service != null && attempt != null && owner != null && source != null && source.gameObject.scene.handle.GetHashCode() == attempt.SceneHandle && owner.gameObject.scene.handle.GetHashCode() == attempt.SceneHandle;
		}

		private static void OnApplicationQuitting()
		{
			ApplicationQuit(owner);
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Reset()
		{
			Application.quitting -= OnApplicationQuitting;
			Service = null;
			owner = null;
			attempt = null;
			settings = null;
			stepTracker = null;
			stepSessionRevision = 0U;
		}
	}
}

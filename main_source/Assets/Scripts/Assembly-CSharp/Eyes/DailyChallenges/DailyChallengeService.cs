using System;
using System.Collections.Generic;
using Eyes.GameplayConditions;
using Newtonsoft.Json;

namespace Eyes.DailyChallenges
{
	public sealed class DailyChallengeService : IDisposable
	{
		[JsonObject(MemberSerialization.OptIn)]
		private sealed class StoredState
		{
			[JsonProperty]
			public int Version = 1;

			[JsonProperty]
			public string QuestId;

			[JsonProperty]
			public long ExpiryTicks;

			[JsonProperty]
			public int Progress;

			[JsonProperty]
			public bool Completed;

			[JsonProperty]
			public PositivePotionEffect PositivePotionEffects;

			[JsonProperty]
			public long NextSequence;

			[JsonProperty]
			public long LastSettledSequence;
		}

		public const string StorageKey = "DailyChallenges.State.v1";

		private readonly IPlayerAwareKeyValueStore store;

		private readonly IPlayerContextManager contexts;

		private readonly IDailyChallengeAssignmentProvider provider;

		private readonly Func<DateTime> utcNow;

		private readonly Action onCompleted;

		private readonly Action<DailyChallengeCompletion> onCompletionTracked;

		private readonly object gate = new object();

		private StoredState state;

		private string loadedContext;

		private DailyChallengeAttempt activeAttempt;

		private DailyChallengeCompletion latestCompletion;

		private DailyChallengeView view = DailyChallengeView.Unavailable;

		private bool disposed;

		public DailyChallengeView View
		{
			get
			{
				lock (gate)
				{
					return view;
				}
			}
		}

		public DailyChallengeAttempt ActiveAttempt
		{
			get
			{
				lock (gate)
				{
					SynchronizeContext();
					return activeAttempt;
				}
			}
		}

		public DailyChallengeCompletion LatestCompletion
		{
			get
			{
				lock (gate)
				{
					SynchronizeContext();
					return latestCompletion;
				}
			}
		}

		public DailyChallengeService(IPlayerAwareKeyValueStore store, IPlayerContextManager contexts, IDailyChallengeAssignmentProvider provider, Func<DateTime> utcNow, Action onCompleted, Action<DailyChallengeCompletion> onCompletionTracked)
		{
			this.store = store ?? throw new ArgumentNullException("store");
			this.contexts = contexts ?? throw new ArgumentNullException("contexts");
			this.provider = provider;
			this.utcNow = utcNow ?? (() => DateTime.UtcNow);
			this.onCompleted = onCompleted;
			this.onCompletionTracked = onCompletionTracked;
			contexts.OnPlayerChanged += OnPlayerChanged;
		}

		public DailyChallengeView Refresh()
		{
			lock (gate)
			{
				return RefreshCore();
			}
		}

		public DailyChallengeAttempt StartAttempt(int sceneHandle)
		{
			lock (gate)
			{
				CloseActiveAttempt();
				latestCompletion = null;
				DailyChallengeView current = RefreshCore();
				if (current.Definition == null || state.NextSequence == long.MaxValue)
				{
					return null;
				}
				long sequence = state.NextSequence + 1;
				state.NextSequence = sequence;
				if (!Save())
				{
					return null;
				}
				activeAttempt = new DailyChallengeAttempt(current.QuestId, current.ExpiresAtUtc, loadedContext, sequence, sceneHandle);
				return activeAttempt;
			}
		}

		public void AddBag(DailyChallengeAttempt attempt)
		{
			lock (gate)
			{
				if (CanAcceptPickup(attempt, DailyChallengeKind.CollectBags) && attempt.BagsCollected != int.MaxValue)
				{
					attempt.BagsCollected++;
				}
				if (CanAcceptPickup(attempt, DailyChallengeKind.CollectEyesBeforeBags) && (attempt.RemainingEyeIds == null || attempt.RemainingEyeIds.Count > 0))
				{
					attempt.EyeOrderFailed = true;
				}
			}
		}

		public void AddPumpkinCatch(DailyChallengeAttempt attempt)
		{
			lock (gate)
			{
				if (CanAcceptPickup(attempt, DailyChallengeKind.PumpkinParty) && attempt.PumpkinsCaught != int.MaxValue)
				{
					attempt.PumpkinsCaught++;
				}
			}
		}

		public void AddGoodBoyFeed(DailyChallengeAttempt attempt)
		{
			lock (gate)
			{
				if (CanAcceptPickup(attempt, DailyChallengeKind.FeedGoodBoy) && attempt.GoodBoyFeedings != int.MaxValue)
				{
					attempt.GoodBoyFeedings++;
				}
			}
		}

		public void AddEye(DailyChallengeAttempt attempt, int eyeId, IEnumerable<int> activatedEyeIds)
		{
			lock (gate)
			{
				if (!CanAcceptPickup(attempt, DailyChallengeKind.CollectEyesBeforeBags))
				{
					return;
				}
				if (attempt.RemainingEyeIds == null)
				{
					HashSet<int> eyes = activatedEyeIds != null ? new HashSet<int>(activatedEyeIds) : null;
					if (eyes == null || eyes.Count == 0)
					{
						attempt.EyeOrderFailed = true;
						return;
					}
					attempt.RemainingEyeIds = eyes;
				}
				attempt.RemainingEyeIds.Remove(eyeId);
			}
		}

		public void AddPositivePotionEffect(DailyChallengeAttempt attempt, PositivePotionEffect effect)
		{
			int bit = (int)effect;
			if (bit >= 9 || ((1 << bit) & 0x116) == 0)
			{
				return;
			}
			lock (gate)
			{
				if (CanAcceptPickup(attempt, DailyChallengeKind.HospitalPositivePotions))
				{
					attempt.PositivePotionEffects = (PositivePotionEffect)((int)attempt.PositivePotionEffects | bit);
				}
			}
		}

		public void AddMapFragment(DailyChallengeAttempt attempt, Level level, int fragmentId)
		{
			if (!MapExplorationRequirements.TryGet(level, out MapExplorationRequirements requirements) || !requirements.Contains(fragmentId))
			{
				return;
			}
			DailyChallengeKind kind;
			switch ((int)level)
			{
			case 0:
				kind = DailyChallengeKind.ExploreMansion;
				break;
			case 1:
				kind = DailyChallengeKind.ExploreHospital;
				break;
			case 3:
				kind = DailyChallengeKind.ExploreSchool;
				break;
			default:
				return;
			}
			lock (gate)
			{
				if (CanAcceptPickup(attempt, kind))
				{
					attempt.DiscoveredMapFragmentIds.Add(fragmentId);
				}
			}
		}

		public void AddPuzzleSolved(DailyChallengeAttempt attempt)
		{
			lock (gate)
			{
				if (CanAcceptPickup(attempt, DailyChallengeKind.FamilyMatters))
				{
					attempt.PuzzleSolved = true;
				}
			}
		}

		public void AddEyeUse(DailyChallengeAttempt attempt)
		{
			lock (gate)
			{
				if (CanAcceptPickup(attempt, DailyChallengeKind.SilentEscape))
				{
					attempt.EyeUsed = true;
				}
			}
		}

		public void AddChase(DailyChallengeAttempt attempt)
		{
			lock (gate)
			{
				if (CanAcceptPickup(attempt, DailyChallengeKind.SilentEscape))
				{
					attempt.WasChased = true;
				}
			}
		}

		public bool FinishAttempt(DailyChallengeAttempt attempt, string endReason, DailyChallengeRunResult result = null)
		{
			lock (gate)
			{
				SynchronizeContext();
				if (attempt == null || disposed || activeAttempt != attempt || attempt.IsClosed)
				{
					return false;
				}
				attempt.EndReason = endReason ?? string.Empty;
				CloseActiveAttempt();
				DateTime now = utcNow();
				DailyChallengeView current = RefreshCore(now);
				if (current.Definition == null || now.Kind != DateTimeKind.Utc || now >= attempt.ExpiresAtUtc || attempt.PlayerContext != loadedContext || attempt.QuestId != current.QuestId || attempt.ExpiresAtUtc != current.ExpiresAtUtc || state.LastSettledSequence >= attempt.Sequence)
				{
					return false;
				}
				bool wasCompleted = current.IsCompleted;
				DailyChallengeDefinition definition = current.Definition;
				int earned = DailyChallengeProgressEvaluator.EarnedProgress(definition, attempt, result, state.PositivePotionEffects);
				if (definition.Kind == DailyChallengeKind.HospitalPositivePotions)
				{
					state.PositivePotionEffects = (PositivePotionEffect)((int)attempt.PositivePotionEffects | (int)state.PositivePotionEffects);
				}
				state.Progress = (int)Math.Min((long)definition.Goal, (long)state.Progress + earned);
				state.Completed = definition.Goal <= state.Progress;
				state.LastSettledSequence = attempt.Sequence;
				if (!Save())
				{
					return false;
				}
				DailyChallengeView updated = new DailyChallengeView(definition, state.Progress, state.Completed, current.ExpiresAtUtc);
				view = updated;
				if (!wasCompleted && updated.IsCompleted)
				{
					latestCompletion = new DailyChallengeCompletion(updated, loadedContext, attempt.Sequence, now);
					Track(onCompletionTracked, latestCompletion);
					onCompleted?.Invoke();
				}
				return true;
			}
		}

		private static void Track<T>(Action<T> callback, T value)
		{
			callback?.Invoke(value);
		}

		private bool CanAcceptPickup(DailyChallengeAttempt attempt, DailyChallengeKind kind)
		{
			SynchronizeContext();
			DateTime now = utcNow();
			if (disposed || attempt == null || activeAttempt != attempt || attempt.IsClosed || attempt.PlayerContext != loadedContext)
			{
				return false;
			}
			if (now.Kind != DateTimeKind.Utc || now >= attempt.ExpiresAtUtc)
			{
				return false;
			}
			return DailyChallengeCatalog.TryGet(attempt.QuestId, out DailyChallengeDefinition definition) && definition.Kind == kind;
		}

		private DailyChallengeView RefreshCore(DateTime? atUtc = null)
		{
			if (!disposed)
			{
				SynchronizeContext();
				if (Load())
				{
					DateTime now = atUtc ?? utcNow();
					DailyChallengeAssignment assignment = provider?.GetAssignment(now);
					if (assignment != null && now.Kind == DateTimeKind.Utc && assignment.ExpiresAtUtc.Kind == DateTimeKind.Utc && assignment.ExpiresAtUtc > now && DailyChallengeCatalog.TryGet(assignment.Id, out DailyChallengeDefinition definition))
					{
						if (state.QuestId != assignment.Id || state.ExpiryTicks != assignment.ExpiresAtUtc.Ticks)
						{
							state.QuestId = assignment.Id;
							state.ExpiryTicks = assignment.ExpiresAtUtc.Ticks;
							state.Progress = 0;
							state.Completed = false;
							state.PositivePotionEffects = (PositivePotionEffect)0;
							Save();
						}
						if (definition.Kind == DailyChallengeKind.HospitalPositivePotions)
						{
							state.Progress = DailyChallengeProgressEvaluator.CountPotionEffects(state.PositivePotionEffects);
						}
						else
						{
							state.Progress = Math.Max(0, Math.Min(definition.Goal, state.Progress));
						}
						state.Completed = definition.Goal <= state.Progress;
						view = new DailyChallengeView(definition, state.Progress, state.Completed, assignment.ExpiresAtUtc);
						return view;
					}
				}
			}
			view = DailyChallengeView.Unavailable;
			return view;
		}

		private void SynchronizeContext()
		{
			string context = store.CurrentPlayerContext;
			if (loadedContext != null && !string.Equals(loadedContext, context, StringComparison.Ordinal))
			{
				ResetContext();
			}
		}

		private bool Load()
		{
			if (state != null)
			{
				return true;
			}
			string context = store.CurrentPlayerContext;
			string json = store.GetString(StorageKey, "");
			StoredState loaded = null;
			if (!string.IsNullOrEmpty(json))
			{
				try
				{
					StoredState parsed = JsonConvert.DeserializeObject<StoredState>(json);
					if (parsed != null && parsed.Version == 1 && parsed.NextSequence >= 0 && parsed.LastSettledSequence >= 0 && parsed.LastSettledSequence <= parsed.NextSequence)
					{
						loaded = parsed;
					}
				}
				catch (JsonException)
				{
				}
			}
			state = loaded ?? new StoredState();
			state.Version = 1;
			if ((uint)state.PositivePotionEffects > 15U)
			{
				state.PositivePotionEffects = (PositivePotionEffect)0;
			}
			loadedContext = context;
			return true;
		}

		private bool Save()
		{
			if (!string.Equals(loadedContext, store.CurrentPlayerContext, StringComparison.Ordinal))
			{
				ResetContext();
				return false;
			}
			store.SetValue(StorageKey, JsonConvert.SerializeObject(state));
			return true;
		}

		private void OnPlayerChanged(string previousPlayer, string nextPlayer)
		{
			lock (gate)
			{
				ResetContext();
			}
		}

		private void ResetContext()
		{
			if (activeAttempt != null)
			{
				activeAttempt.IsClosed = true;
			}
			activeAttempt = null;
			latestCompletion = null;
			state = null;
			loadedContext = null;
			view = DailyChallengeView.Unavailable;
		}

		private void CloseActiveAttempt()
		{
			if (activeAttempt != null)
			{
				activeAttempt.IsClosed = true;
			}
			activeAttempt = null;
		}

		public void Dispose()
		{
			lock (gate)
			{
				if (!disposed)
				{
					disposed = true;
					contexts.OnPlayerChanged -= OnPlayerChanged;
					ResetContext();
				}
			}
		}
	}
}

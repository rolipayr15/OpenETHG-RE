using UnityEngine;
using System;
using System.Threading.Tasks;
using ExternalApi.Models;
using Multiplayer.ExternalApi;

namespace Eyes.DailyChallenges
{
	public sealed class ApiDailyChallengeAssignmentProvider : IDailyChallengeAssignmentProvider
	{
		private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(30.0);

		private readonly IChallengesApi api;

		private readonly object gate = new object();

		private DailyChallengeAssignment cached;

		private Task<CurrentDailyChallengeResponse?> pending;

		private DateTime retryAfterUtc;

		public ApiDailyChallengeAssignmentProvider(IChallengesApi api)
		{
			this.api = api ?? throw new ArgumentNullException("api");
			OnlineConfig.ServerChanged += delegate
			{
				lock (gate)
				{
					cached = null;
					pending = null;
					retryAfterUtc = default(DateTime);
				}
			};
		}

		public DailyChallengeAssignment GetAssignment(DateTime utcNow)
		{
			if (utcNow.Kind != DateTimeKind.Utc)
			{
				return null;
			}
			lock (gate)
			{
				if (cached != null && cached.ExpiresAtUtc > utcNow)
				{
					return cached;
				}
				cached = null;
				if (pending == null)
				{
					if (!(utcNow < retryAfterUtc))
					{
						pending = api.GetCurrentDailyChallengeAsync();
					}
					return null;
				}
				if (!pending.IsCompleted)
				{
					return null;
				}
				Task<CurrentDailyChallengeResponse?> finished = pending;
				pending = null;
				if (finished.Status == TaskStatus.RanToCompletion)
				{
					cached = Map(finished.Result, utcNow);
					if (cached != null)
					{
						return cached;
					}
				}
				else
				{
					string reason = finished.Exception != null ? finished.Exception.InnerException.GetType().Name : finished.Status.ToString();
					Debug.LogWarning("[DailyChallenge] Request failed (" + reason + "). Retrying in 30 seconds.");
				}
				retryAfterUtc = utcNow + RetryDelay;
				return null;
			}
		}

		private static DailyChallengeAssignment Map(CurrentDailyChallengeResponse? response, DateTime utcNow)
		{
			if (!response.HasValue)
			{
				Debug.LogWarning("[DailyChallenge] No assignment returned. Check API authentication/HTTP logs; retrying in 30 seconds.");
				return null;
			}
			DateTime deadline = response.Value.deadline.UtcDateTime;
			Debug.Log(string.Format("[DailyChallenge] Received challengeId={0}, deadline={1:O}, nowUtc={2:O}.", response.Value.challengeId, deadline, utcNow));
			if (deadline <= utcNow)
			{
				Debug.LogWarning("[DailyChallenge] Assignment rejected: deadline is missing or expired. Retrying in 30 seconds.");
				return null;
			}
			if (!string.IsNullOrEmpty(response.Value.title))
			{
				return new DailyChallengeAssignment(DailyChallengeCatalog.RegisterServerDefinition(response.Value).Id, deadline);
			}
			string id = response.Value.challengeId == 1 ? DailyChallengeCatalog.DailyCollection.Id : response.Value.challengeId.ToString();
			if (!DailyChallengeCatalog.TryGet(id, out DailyChallengeDefinition definition))
			{
				definition = DailyChallengeCatalog.DailyCollection;
				Debug.LogWarning(string.Format("[DailyChallenge] Unsupported challengeId={0}; using fallback {1} until {2:O}.", response.Value.challengeId, definition.Id, deadline));
			}
			return new DailyChallengeAssignment(definition.Id, deadline);
		}
	}
}

using System.Collections.Generic;
using ExternalApi.Models;

namespace Eyes.DailyChallenges
{
	public static class DailyChallengeCatalog
	{
		public static DailyChallengeDefinition DailyCollection { get; } = new DailyChallengeDefinition("bags_50", (DailyChallengeKind)0, 50, 50, "Daily collection", "Collect 50 money bags yourself across games today. \nItems count when each game ends, even if you lose or quit.", "Bags", "Collect 50 money bags across games today.");

		public static DailyChallengeDefinition MansionDoorLimit { get; } = new DailyChallengeDefinition("2", (DailyChallengeKind)1, 2, 1, "Door limit", "Win a single-player game in the Mansion on Normal difficulty, passing through each door at most twice. \nStandard, Halloween and Pixel modes count. \nSandbox games do not count.", "MansionDoorLimit", "Beat Normal in the Mansion, using each door at most twice.");

		public static DailyChallengeDefinition WinWithStepLimit { get; } = new DailyChallengeDefinition("3", (DailyChallengeKind)2, 500, 1, "Win in 500 steps", "Win a single-player game on Normal difficulty, taking at most 500 steps. \nAny level and Standard, Halloween or Pixel mode count. \nSandbox games do not count.", "WinWithStepLimit", "Beat Normal, taking at most 500 steps.");

		public static DailyChallengeDefinition HospitalPositivePotions { get; } = new DailyChallengeDefinition("4", (DailyChallengeKind)3, 4, 4, "Hospital potion discoveries", "Use potions in the Hospital to experience all four positive effects today: extra eyes, invisibility, speed and bag vision. \nDiscoveries carry across games and count when each game ends, even if you lose or quit.", "HospitalPositivePotions", "Discover all four positive potion effects in the Hospital today.");

		public static DailyChallengeDefinition CollectEyesBeforeBags { get; } = new DailyChallengeDefinition("5", (DailyChallengeKind)4, 1, 1, "Eyes before bags", "Collect all eyes on the map before any bag, then win the game on Normal or higher. \nAny map; single-player Standard, Halloween or Pixel, without Sandbox. \nBonus eyes do not count; using eyes is allowed. Win before the deadline for the challenge to count.", "CollectEyesBeforeBags", "Collect all eyes before any bag, then win on Normal or higher.");

		public static DailyChallengeDefinition WinWithTimeLimit { get; } = new DailyChallengeDefinition("6", (DailyChallengeKind)5, 300, 1, "Beat the clock", "Win a single-player game in less than 5 minutes on Normal or higher. \nAny map; Standard, Halloween or Pixel, without Sandbox. \nWin before the deadline for the challenge to count.", "WinWithTimeLimit", "Win in less than 5 minutes on Normal or higher.");

		public static DailyChallengeDefinition FamilyMatters { get; } = new DailyChallengeDefinition("7", (DailyChallengeKind)6, 1, 1, "Family matters", "Solve the portrait puzzle in the Mansion and unlock the secret room. \nAny difficulty and mode, including multiplayer; Sandbox games do not count. \nYour solution counts when the game ends before the challenge deadline, even if you lose or quit.", "FamilyMatters", "Solve the portrait puzzle in the Mansion and unlock the secret room.");

		public static DailyChallengeDefinition WinVersusRandom { get; } = new DailyChallengeDefinition("8", (DailyChallengeKind)7, 1, 1, "Win against a stranger", "Win a multiplayer VS game against a randomly matched opponent. \nAny map and difficulty count. Games with friends and co-op games do not count. \nWin before the challenge deadline for the challenge to count.", "WinVersusRandom", "Win a multiplayer VS game against a randomly matched opponent.");

		public static DailyChallengeDefinition SilentEscape { get; } = new DailyChallengeDefinition("9", (DailyChallengeKind)8, 1, 1, "Silent escape", "Win a single-player game on Normal or higher difficulty without using an eye or ever being chased. \nAny map counts. Collecting eyes is allowed. \nWin before the challenge deadline for the challenge to count.", "SilentEscape", "Win solo on Normal or higher without using an eye or ever being chased.");

		public static DailyChallengeDefinition PumpkinParty { get; } = new DailyChallengeDefinition("10", (DailyChallengeKind)9, 4, 1, "Pumpkin Party", "Catch the pumpkin at least 4 times in one game, then win before the deadline. \nHalloween mode, any map and difficulty; single-player, without Sandbox.", "PumpkinParty", "Catch the pumpkin at least 4 times in one Halloween game and win.");

		public static DailyChallengeDefinition FeedGoodBoy { get; } = new DailyChallengeDefinition("11", (DailyChallengeKind)10, 5, 1, "Feed Good Boy", "Feed Good Boy at least 5 times in one single-player game on Normal or higher. \nAny map and mode; Sandbox games do not count. \nFinish the game before the challenge deadline; losing or quitting also counts.", "FeedGoodBoy", "Feed Good Boy at least 5 times in one single-player game on Normal or higher.");

		public static DailyChallengeDefinition WinCoopRandom { get; } = new DailyChallengeDefinition("12", (DailyChallengeKind)11, 1, 1, "Together in Fear", "Win a multiplayer co-op match with a random player. \nAny map and difficulty count. Games with friends and VS games do not count. \nWin before the challenge deadline for the challenge to count.", "WinCoopRandom", "Win a multiplayer co-op match with a random player.");

		public static DailyChallengeDefinition WinCoopFriend { get; } = new DailyChallengeDefinition("13", (DailyChallengeKind)12, 1, 1, "Bound by Terror", "Win a multiplayer co-op match with an invited friend. \nAny map and difficulty count. Random matchmaking and VS games do not count. \nWin before the challenge deadline for the challenge to count.", "WinCoopFriend", "Win a multiplayer co-op match with an invited friend.");

		public static DailyChallengeDefinition WinVersusFriend { get; } = new DailyChallengeDefinition("14", (DailyChallengeKind)13, 1, 1, "No Mercy for Friends", "Win a versus multiplayer match against an invited friend. \nThe victory must be achieved through gameplay — not because the opponent leaves the match or exits the game. \nAny map and difficulty count. Win before the challenge deadline.", "WinVersusFriend", "Win a VS match against an invited friend; wins from an opponent leaving do not count.");

		public static DailyChallengeDefinition ExploreMansion { get; } = new DailyChallengeDefinition("15", (DailyChallengeKind)14, 24, 1, "No Secrets Left Behind: Mansion", "Discover 24 distinct areas in the Mansion in one game. \nAny difficulty and mode, including multiplayer; Sandbox games do not count. \nNo win required. Finish before the challenge deadline; losing or quitting also counts.", "ExploreMansion", "Discover 24 different Mansion areas in one game; no win required.");

		public static DailyChallengeDefinition ExploreSchool { get; } = new DailyChallengeDefinition("16", (DailyChallengeKind)15, 25, 1, "No Secrets Left Behind: School", "Discover all 25 areas in the School in one game. \nAny difficulty and mode, including multiplayer; Sandbox games do not count. \nNo win required. Finish before the challenge deadline; losing or quitting also counts.", "ExploreSchool", "Discover all 25 School areas in one game; no win required.");

		public static DailyChallengeDefinition ExploreHospital { get; } = new DailyChallengeDefinition("17", (DailyChallengeKind)16, 17, 1, "No Secrets Left Behind: Hospital", "Discover all 17 areas in the Hospital in one game. \nAny difficulty and mode, including multiplayer; Sandbox games do not count. \nNo win required. Finish before the challenge deadline; losing or quitting also counts.", "ExploreHospital", "Discover all 17 Hospital areas in one game; no win required.");

		public static DailyChallengeDefinition Current => DailyCollection;

		private static readonly DailyChallengeDefinition[] All = new DailyChallengeDefinition[] { DailyCollection, MansionDoorLimit, WinWithStepLimit, HospitalPositivePotions, CollectEyesBeforeBags, WinWithTimeLimit, FamilyMatters, WinVersusRandom, SilentEscape, PumpkinParty, FeedGoodBoy, WinCoopRandom, WinCoopFriend, WinVersusFriend, ExploreMansion, ExploreSchool, ExploreHospital };

		private static readonly Dictionary<string, DailyChallengeDefinition> ServerDefinitions = new Dictionary<string, DailyChallengeDefinition>();

		// Recovery addition: challenges written in the custom server's admin panel.
		public static DailyChallengeDefinition RegisterServerDefinition(CurrentDailyChallengeResponse response)
		{
			string id = "srv_" + response.challengeId;
			string term = "Server" + response.challengeId;
			DailyChallengeDefinition definition = new DailyChallengeDefinition(id, (DailyChallengeKind)response.kind, response.parameter, response.goal < 1 ? 1 : response.goal, response.title, response.description ?? "", term, response.shortDescription ?? response.title);
			ServerDefinitions[id] = definition;
			return definition;
		}

		public static bool TryGet(string id, out DailyChallengeDefinition definition)
		{
			foreach (DailyChallengeDefinition candidate in All)
			{
				if (string.Equals(candidate.Id, id, System.StringComparison.Ordinal))
				{
					definition = candidate;
					return true;
				}
			}
			if (id != null && ServerDefinitions.TryGetValue(id, out definition))
			{
				return true;
			}
			definition = null;
			return false;
		}
	}
}

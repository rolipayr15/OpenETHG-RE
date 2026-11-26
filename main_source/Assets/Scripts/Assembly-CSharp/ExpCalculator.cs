using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;

public class ExpCalculator
{
	private CalcResult calcResult;

	private bool calculated;

	private Dictionary<ExpSourceType, IEventCounter> gameEvents = new Dictionary<ExpSourceType, IEventCounter>();

	public ExpCalculator()
	{
		foreach (KeyValuePair<ExpSourceType, Rule> rule in GetCalcRules())
		{
			if ((int)rule.Value.Type <= 1)
			{
				gameEvents[rule.Key] = ((rule.Value.Type == ExpEventType.Bool) ? ((IEventCounter)new BoolCounter()) : new IntCounter());
			}
		}
	}

	private Dictionary<ExpSourceType, Rule> GetCalcRules()
	{
		return new Dictionary<ExpSourceType, Rule>
		{
			{ ExpSourceType.EyesCollected, new Rule { Mode = CalcMode.Add, Phase = Phase.Base, Type = ExpEventType.Counter, Value = UserGameSettings.ExpPoints.pointsPerEye, Label = ScriptLocalization.Eyes } },
			{ ExpSourceType.BagsCollected, new Rule { Mode = CalcMode.Add, Phase = Phase.Base, Type = ExpEventType.Counter, Value = UserGameSettings.ExpPoints.pointsPerBag, Label = ScriptLocalization.Bags } },
			{ ExpSourceType.SawFound, new Rule { Mode = CalcMode.Add, Phase = Phase.Base, Type = ExpEventType.Bool, Value = UserGameSettings.ExpPoints.pointsForSaw, Label = ScriptLocalization.SawFound } },
			{ ExpSourceType.KeyFound, new Rule { Mode = CalcMode.Add, Phase = Phase.Base, Type = ExpEventType.Bool, Value = UserGameSettings.ExpPoints.pointsForKey, Label = ScriptLocalization.KeyFound } },
			{ ExpSourceType.PortraitsMinigameSolved, new Rule { Mode = CalcMode.Add, Phase = Phase.Base, Type = ExpEventType.Bool, Value = UserGameSettings.ExpPoints.pointsForPortraitsMinigame, Label = ScriptLocalization.PortraitsSolved } },
			{ ExpSourceType.SafeUnlocked, new Rule { Mode = CalcMode.Add, Phase = Phase.Base, Type = ExpEventType.Bool, Value = UserGameSettings.ExpPoints.pointsForSafe, Label = ScriptLocalization.SafeUnlocked } },
			{ ExpSourceType.EggPickup, new Rule { Mode = CalcMode.Add, Phase = Phase.Base, Type = ExpEventType.Counter, Value = 1, Label = ScriptLocalization.SpecialEvent } },
			{ ExpSourceType.GameWon, new Rule { Mode = CalcMode.MultiplyPercent, Phase = Phase.Modifiers, Type = ExpEventType.Bool, Value = UserGameSettings.ExpPoints.winPercent, Label = ScriptLocalization.MultiplayerWin } },
			{ ExpSourceType.MapPlayed, new Rule { Mode = CalcMode.MultiplyPercent, Phase = Phase.Modifiers, Type = ExpEventType.Setting, Value = GetMapPoints(), Label = ScriptLocalization.Map } },
			{ ExpSourceType.GameDifficulty, new Rule { Mode = CalcMode.MultiplyPercent, Phase = Phase.Modifiers, Type = ExpEventType.Setting, Value = GetDifficultyPoints(), Label = ScriptLocalization.DifficultyLevel } },
			{ ExpSourceType.MultiplayerPlayed, new Rule { Mode = CalcMode.MultiplyPercent, Phase = Phase.Modifiers, Type = ExpEventType.Setting, Value = GetMultiplayerPoints(), Label = ScriptLocalization.Multiplayer } },
			{ ExpSourceType.BonusEvent, new Rule { Mode = CalcMode.MultiplyPercent, Phase = Phase.Modifiers, Type = ExpEventType.Setting, Value = UserGameSettings.ExpPoints.bonusEventPercent, Label = ScriptLocalization.SpecialEvent } }
		};
	}

	public Dictionary<string, int> ExportGameEvents()
	{
		return gameEvents.ToDictionary((KeyValuePair<ExpSourceType, IEventCounter> kv) => kv.Key.ToString(), (KeyValuePair<ExpSourceType, IEventCounter> kv) => kv.Value.Count);
	}

	public void RegisterEvent(ExpSourceType e)
	{
		calculated = false;
		gameEvents[e].Register(1);
	}

	public void RegisterEvent(ExpSourceType e, int count)
	{
		calculated = false;
		gameEvents[e].Register(count);
	}

	public CalcResult Calculate()
	{
		if (calculated)
		{
			return calcResult;
		}
		Dictionary<ExpSourceType, Rule> rules = GetCalcRules();
		List<PhaseResult> phases = new List<PhaseResult>();
		int total = 0;
		foreach (Phase phase in Enum.GetValues(typeof(Phase)).Cast<Phase>().OrderBy((Phase p) => (int)p))
		{
			PhaseResult phaseResult = new PhaseResult(phase);
			List<(Rule rule, int count, int value)> modifiers = new List<(Rule, int, int)>();
			int phaseDelta = 0;
			foreach (KeyValuePair<ExpSourceType, Rule> pair in rules)
			{
				Rule rule = pair.Value;
				if (rule.Phase != phase)
				{
					continue;
				}
				int count;
				if ((int)rule.Type < 2)
				{
					if (!gameEvents.TryGetValue(pair.Key, out IEventCounter counter))
					{
						continue;
					}
					count = counter.Count;
				}
				else
				{
					if (rule.Type != ExpEventType.Setting)
					{
						continue;
					}
					count = 1;
				}
				if (rule.Mode == CalcMode.Add)
				{
					int delta = count * rule.Value;
					phaseDelta += delta;
					phaseResult.Steps.Add(new Step(rule.Label, count, CalcMode.Add, rule.Value, delta, phaseDelta));
				}
				else
				{
					modifiers.Add((rule, count, rule.Value));
				}
			}
			int baseValue = phaseDelta + total;
			foreach ((Rule rule, int count, int value) modifier in modifiers)
			{
				int delta = (int)((float)(modifier.count * baseValue * (modifier.value - 100)) / 100f);
				phaseDelta += delta;
				phaseResult.Steps.Add(new Step(modifier.rule.Label, modifier.count, modifier.rule.Mode, modifier.value, delta, phaseDelta + total));
			}
			total += phaseDelta;
			phaseResult.Seal(total, phaseDelta);
			phases.Add(phaseResult);
		}
		calcResult = new CalcResult(total, phases);
		calculated = true;
		return calcResult;
	}

	private static int GetMapPoints()
	{
		switch ((int)UserGameSettings.currentLevel)
		{
		case 3:
			return UserGameSettings.ExpPoints.mapSchoolPercent;
		case 1:
			return UserGameSettings.ExpPoints.mapHospitalPercent;
		case 0:
			return UserGameSettings.ExpPoints.mapMansionPercent;
		default:
			return 0;
		}
	}

	private static int GetDifficultyPoints()
	{
		switch ((int)UserGameSettings.currentDifficulty)
		{
		case 0:
			return UserGameSettings.ExpPoints.difficultyNewbiePercent;
		case 1:
			return UserGameSettings.ExpPoints.difficultyEasyPercent;
		case 2:
			return UserGameSettings.ExpPoints.difficultyNormalPercent;
		case 3:
			return UserGameSettings.ExpPoints.difficultyHardPercent;
		case 4:
			return UserGameSettings.ExpPoints.difficultyNightmarePercent;
		default:
			return 0;
		}
	}

	private static int GetMultiplayerPoints()
	{
		if (!UserGameSettings.IsMultiplayer())
		{
			return 100;
		}
		return UserGameSettings.ExpPoints.multiplayerPercent;
	}

	public void Reset()
	{
		foreach (IEventCounter counter in gameEvents.Values)
		{
			counter.Reset();
		}
		calculated = false;
	}
}
